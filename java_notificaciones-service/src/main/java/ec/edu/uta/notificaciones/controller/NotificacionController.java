package ec.edu.uta.notificaciones.controller;

import java.util.List;
import java.util.Map;

import ec.edu.uta.notificaciones.dto.NotificacionRequest;
import ec.edu.uta.notificaciones.dto.NotificacionResponse;
import ec.edu.uta.notificaciones.model.Notificacion;
import ec.edu.uta.notificaciones.service.NotificacionService;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.http.HttpStatus;
import org.springframework.http.ResponseEntity;
import org.springframework.web.bind.annotation.*;

@RestController
@CrossOrigin(origins = "*")
@RequestMapping("/api/notificaciones")
public class NotificacionController {

    private final NotificacionService notificacionService;
    private final String nombreCola;

    public NotificacionController(
            NotificacionService notificacionService,
            @Value("${app.rabbitmq.queue:notificaciones.ventas}") String nombreCola) {
        this.notificacionService = notificacionService;
        this.nombreCola = nombreCola;
    }

    @GetMapping
    public List<Notificacion> listar() {
        return notificacionService.listar();
    }

    @GetMapping("/estado")
    public Map<String, Object> estado() {
        return Map.of(
                "status", "UP",
                "servicio", "notificaciones-service",
                "cola", nombreCola,
                "total", notificacionService.contar()
        );
    }

    @PostMapping
    public ResponseEntity<NotificacionResponse> crear(
            @RequestBody NotificacionRequest request) {

        NotificacionResponse response =
                notificacionService.procesar(request);

        return ResponseEntity
                .status(HttpStatus.CREATED)
                .body(response);
    }
}
