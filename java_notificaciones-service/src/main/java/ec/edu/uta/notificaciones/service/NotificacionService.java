package ec.edu.uta.notificaciones.service;

import ec.edu.uta.notificaciones.dto.NotificacionRequest;
import ec.edu.uta.notificaciones.dto.NotificacionResponse;
import ec.edu.uta.notificaciones.model.Notificacion;
import ec.edu.uta.notificaciones.repository.NotificacionRepository;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.time.Instant;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.UUID;

@Service
public class NotificacionService {

    private static final Logger log = LoggerFactory.getLogger(NotificacionService.class);

    private static final Set<String> TIPOS_PERMITIDOS =
            Set.of("REGISTRO", "ACTUALIZACION", "ELIMINACION", "VENTA", "VENTA_CONFIRMADA");

    private final NotificacionRepository repository;

    public NotificacionService(NotificacionRepository repository) {
        this.repository = repository;
    }

    @Transactional(readOnly = true)
    public List<Notificacion> listar() {
        return repository.findAllByOrderByIdDesc();
    }

    @Transactional(readOnly = true)
    public long contar() {
        return repository.count();
    }

    @Transactional
    public NotificacionResponse procesar(NotificacionRequest request) {

        if (request.getVehiculoId() == null || request.getVehiculoId() <= 0) {
            throw new IllegalArgumentException("vehiculoId debe estar presente y ser válido");
        }

        if (request.getDestinatario() == null ||
                request.getDestinatario().trim().isEmpty()) {
            throw new IllegalArgumentException("El destinatario no puede estar vacío");
        }

        if (request.getMensaje() == null ||
                request.getMensaje().trim().isEmpty()) {
            throw new IllegalArgumentException("El mensaje no puede estar vacío");
        }

        if (request.getTipo() == null ||
                !TIPOS_PERMITIDOS.contains(request.getTipo().trim().toUpperCase(Locale.ROOT))) {
            throw new IllegalArgumentException("Tipo de notificación no válido");
        }

        String tipoNormalizado = request.getTipo().trim().toUpperCase(Locale.ROOT);
        String destinatarioNormalizado = request.getDestinatario().trim();
        String mensajeNormalizado = request.getMensaje().trim();

        Notificacion n = new Notificacion();
        n.setEventoId("rest-" + UUID.randomUUID());
        n.setVehiculoId(request.getVehiculoId());
        n.setDestinatario(destinatarioNormalizado);
        n.setTipo(tipoNormalizado);
        n.setMensaje(mensajeNormalizado);
        n.setEstado("PROCESADA");
        n.setCanal("REST");
        n.setFecha(Instant.now().toString());
        repository.saveAndFlush(n);

        log.info("Notificacion REST procesada: vehiculoId={}, tipo={}", request.getVehiculoId(), tipoNormalizado);

        return new NotificacionResponse(
                true,
                "ENVIADA",
                "Notificación procesada correctamente"
        );
    }
}
