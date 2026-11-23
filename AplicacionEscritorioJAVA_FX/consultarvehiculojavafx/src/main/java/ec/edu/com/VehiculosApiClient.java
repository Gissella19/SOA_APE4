package ec.edu.com;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.core.type.TypeReference;
import com.fasterxml.jackson.databind.ObjectMapper;

import java.net.URI;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.time.Duration;
import java.util.List;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.CompletionException;

/**
 * Cliente HTTP asíncrono para consultar las notificaciones de ventas procesadas
 * por java_notificaciones-service desde RabbitMQ, enrutadas a través de NGINX (8090)
 * o directamente en el puerto 8081.
 */
public final class VehiculosApiClient {

    public static final String URL_NGINX_DEFAULT = "http://127.0.0.1:8090/api/notificaciones";
    public static final String URL_DIRECTA_8081 = "http://127.0.0.1:8081/api/notificaciones";

    private final HttpClient clienteHttp = HttpClient.newBuilder()
            .connectTimeout(Duration.ofSeconds(5))
            .build();

    private final ObjectMapper mapper = new ObjectMapper();

    public CompletableFuture<ResultadoNotificaciones> consultarNotificaciones(String urlDestino) {
        String urlLimpia = (urlDestino == null || urlDestino.isBlank())
                ? URL_NGINX_DEFAULT
                : urlDestino.trim();

        HttpRequest solicitud;
        try {
            solicitud = HttpRequest.newBuilder(URI.create(urlLimpia))
                    .timeout(Duration.ofSeconds(10))
                    .header("Accept", "application/json")
                    .GET()
                    .build();
        } catch (IllegalArgumentException ex) {
            return CompletableFuture.failedFuture(ex);
        }

        return clienteHttp.sendAsync(solicitud, HttpResponse.BodyHandlers.ofString())
                .thenApply(respuesta -> {
                    int codigo = respuesta.statusCode();
                    if (codigo != 200) {
                        return new ResultadoNotificaciones(codigo, List.of(), respuesta.body());
                    }
                    try {
                        List<NotificacionVenta> lista = mapper.readValue(
                                respuesta.body(),
                                new TypeReference<List<NotificacionVenta>>() {}
                        );
                        return new ResultadoNotificaciones(codigo, lista, "");
                    } catch (JsonProcessingException ex) {
                        throw new CompletionException(ex);
                    }
                });
    }

    public record ResultadoNotificaciones(
            int codigoHttp,
            List<NotificacionVenta> notificaciones,
            String detalleError
    ) {}
}
