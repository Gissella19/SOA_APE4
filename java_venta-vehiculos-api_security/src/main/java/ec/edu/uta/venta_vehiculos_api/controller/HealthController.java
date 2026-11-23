package ec.edu.uta.venta_vehiculos_api.controller;

import java.util.Map;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class HealthController {

    @Value("${server.port:8080}")
    private int serverPort;

    @GetMapping("/actuator/health")
    public Map<String, Object> health() {
        return Map.of(
                "status", "UP",
                "port", serverPort,
                "instancia", serverPort == 8080 ? "A" : "B"
        );
    }
}
