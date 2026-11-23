package ec.edu.uta.notificaciones.config;

import org.springframework.amqp.core.Queue;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

@Configuration
public class RabbitConfig {

    @Bean
    public Queue colaVentas(@Value("${app.rabbitmq.queue:notificaciones.ventas}") String nombreCola) {
        return new Queue(nombreCola, true);
    }
}
