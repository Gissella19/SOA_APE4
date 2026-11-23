package ec.edu.com;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import java.math.BigDecimal;

/**
 * Representa un registro de notificación de venta consumido desde
 * GET /api/notificaciones (originado en la cola durable RabbitMQ notificaciones.ventas).
 */
@JsonIgnoreProperties(ignoreUnknown = true)
public record NotificacionVenta(
    Integer id,
    String eventoId,
    Integer ventaId,
    String operacionId,
    Integer vehiculoId,
    String compradorNombre,
    String destinatario,
    BigDecimal precioAcordado,
    String moneda,
    String tipo,
    String mensaje,
    String estado,
    String canal,
    String fecha
) {}
