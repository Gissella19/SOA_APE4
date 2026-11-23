package ec.edu.uta.notificaciones.messaging;

import java.math.BigDecimal;
import java.time.Instant;
import java.util.UUID;

import com.fasterxml.jackson.databind.JsonNode;
import com.fasterxml.jackson.databind.ObjectMapper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.amqp.rabbit.annotation.RabbitListener;
import org.springframework.stereotype.Component;
import org.springframework.transaction.annotation.Transactional;

import ec.edu.uta.notificaciones.model.Notificacion;
import ec.edu.uta.notificaciones.repository.NotificacionRepository;

@Component
public class VentaNotificacionListener {

    private static final Logger log = LoggerFactory.getLogger(VentaNotificacionListener.class);

    private final NotificacionRepository repository;
    private final ObjectMapper objectMapper;

    public VentaNotificacionListener(
            NotificacionRepository repository,
            ObjectMapper objectMapper) {
        this.repository = repository;
        this.objectMapper = objectMapper;
    }

    @RabbitListener(queues = "${app.rabbitmq.queue:notificaciones.ventas}")
    @Transactional
    public void recibirEventoVenta(String payloadJson) {
        try {
            JsonNode nodo = objectMapper.readTree(payloadJson);

            Integer ventaId = nodo.hasNonNull("ventaId") ? nodo.get("ventaId").asInt() : null;
            String operacionId = texto(nodo, "operacionId", null);
            String eventoId = texto(nodo, "eventoId", null);
            if (eventoId == null || eventoId.isBlank()) {
                if (operacionId != null && !operacionId.isBlank()) {
                    eventoId = operacionId;
                } else if (ventaId != null) {
                    eventoId = "venta-" + ventaId;
                } else {
                    eventoId = "evt-" + UUID.randomUUID();
                }
            }

            if (repository.existsByEventoId(eventoId)) {
                log.info("Evento duplicado ignorado en consumidor RabbitMQ: eventoId={}, ventaId={}", eventoId, ventaId);
                return;
            }

            Notificacion n = new Notificacion();
            n.setEventoId(eventoId);
            n.setVentaId(ventaId);
            n.setOperacionId(operacionId);
            n.setVehiculoId(nodo.hasNonNull("vehiculoId") ? nodo.get("vehiculoId").asInt() : null);
            n.setCompradorNombre(texto(nodo, "compradorNombre", ""));
            n.setDestinatario(texto(nodo, "destinatario", ""));
            n.setMoneda(texto(nodo, "moneda", "USD"));
            n.setTipo(texto(nodo, "tipo", "VENTA_CONFIRMADA"));
            n.setMensaje(texto(nodo, "mensaje", "Venta confirmada"));
            n.setEstado("PROCESADA");
            n.setCanal("RABBITMQ");
            n.setFecha(texto(nodo, "fecha", Instant.now().toString()));

            String precioTexto = texto(nodo, "precioAcordado", null);
            if (precioTexto != null && !precioTexto.isBlank()) {
                try {
                    n.setPrecioAcordado(new BigDecimal(precioTexto));
                } catch (NumberFormatException ignored) {
                    n.setPrecioAcordado(null);
                }
            }

            repository.saveAndFlush(n);
            log.info("Notificacion de venta consumida desde RabbitMQ y persistida: eventoId={}, ventaId={}, vehiculoId={}",
                    eventoId, ventaId, n.getVehiculoId());
        } catch (Exception ex) {
            log.error("Error al procesar mensaje de la cola RabbitMQ: {}", ex.getMessage());
        }
    }

    private String texto(JsonNode nodo, String campo, String porDefecto) {
        if (nodo.hasNonNull(campo)) {
            String valor = nodo.get(campo).asText();
            return valor != null ? valor.trim() : porDefecto;
        }
        return porDefecto;
    }
}
