package ec.edu.com;

import javafx.animation.Animation;
import javafx.animation.KeyFrame;
import javafx.animation.Timeline;
import javafx.application.Application;
import javafx.application.Platform;
import javafx.beans.property.ReadOnlyObjectWrapper;
import javafx.geometry.Insets;
import javafx.geometry.Pos;
import javafx.scene.Scene;
import javafx.scene.control.Button;
import javafx.scene.control.Label;
import javafx.scene.control.TableColumn;
import javafx.scene.control.TableView;
import javafx.scene.control.TextArea;
import javafx.scene.control.TextField;
import javafx.scene.layout.HBox;
import javafx.scene.layout.Priority;
import javafx.scene.layout.Region;
import javafx.scene.layout.VBox;
import javafx.scene.paint.Color;
import javafx.scene.text.Font;
import javafx.scene.text.FontWeight;
import javafx.stage.Stage;
import javafx.util.Duration;

import java.time.LocalTime;
import java.time.format.DateTimeFormatter;
import java.util.List;
import java.util.function.Function;

/**
 * Monitor de Escritorio JavaFX en Tiempo Real para Notificaciones de Ventas (Actividad 9 / P21-P22).
 * Consume automáticamente GET /api/notificaciones cada 3 segundos mediante Timeline,
 * mostrando los eventos consumidos desde la cola durable RabbitMQ (notificaciones.ventas).
 */
public final class VehiculosApp extends Application {

    private static final DateTimeFormatter FORMATO_HORA = DateTimeFormatter.ofPattern("HH:mm:ss");

    private final VehiculosApiClient api = new VehiculosApiClient();

    private final TextField txtEndpoint = new TextField(VehiculosApiClient.URL_NGINX_DEFAULT);
    private final Button btnUsarNginx = new Button("NGINX (8090)");
    private final Button btnUsarDirecto = new Button("Directo (8081)");
    private final Button btnActualizarAhora = new Button("Actualizar Ahora");
    private final Button btnTogglePolling = new Button("Pausar Polling (3s)");

    private final Label lblBadgeConexion = new Label("VERIFICANDO CONEXIÓN...");
    private final Label lblEstado = new Label("Iniciando monitor automático cada 3 segundos...");

    // Tarjetas KPI
    private final Label lblKpiTotal = new Label("0");
    private final Label lblKpiUltimaVenta = new Label("Sin ventas notificadas");
    private final Label lblKpiCola = new Label("notificaciones.ventas (Durable)");

    private final TableView<NotificacionVenta> tabla = new TableView<>();
    private final TextArea txtDetallePayload = new TextArea();

    private Timeline pollingTimeline;
    private boolean pollingActivo = true;
    private boolean consultaEnCurso = false;

    @Override
    public void start(Stage ventana) {
        // 1. Cabecera Principal
        Label lblEyebrow = new Label("APE 04 · CLIENTE DE ESCRITORIO JAVAFX 17 · CONSUMIDOR DE EVENTOS RABBITMQ");
        lblEyebrow.setFont(Font.font("Segoe UI", FontWeight.BOLD, 11));
        lblEyebrow.setTextFill(Color.web("#38bdf8"));

        Label lblTitulo = new Label("Monitor en Tiempo Real de Notificaciones de Ventas");
        lblTitulo.setFont(Font.font("Segoe UI", FontWeight.BOLD, 20));
        lblTitulo.setTextFill(Color.WHITE);

        Label lblSubtitulo = new Label(
                "Rol Arquitectónico: Visualización automática sin recarga manual de notificaciones originadas en FastAPI -> RabbitMQ -> java_notificaciones-service"
        );
        lblSubtitulo.setFont(Font.font("Segoe UI", 12));
        lblSubtitulo.setTextFill(Color.web("#cbd5e1"));

        estilizarBadgeConexion("#334155", "#f8fafc", "INICIANDO MONITOR...");

        VBox bloqueTitulos = new VBox(3, lblEyebrow, lblTitulo, lblSubtitulo);
        Region espaciadorCabecera = new Region();
        HBox.setHgrow(espaciadorCabecera, Priority.ALWAYS);

        HBox cabecera = new HBox(16, bloqueTitulos, espaciadorCabecera, lblBadgeConexion);
        cabecera.setAlignment(Pos.CENTER_LEFT);
        cabecera.setPadding(new Insets(16, 20, 16, 20));
        cabecera.setStyle("-fx-background-color: #0f172a; -fx-background-radius: 12;");

        // 2. Barra de Endpoint y Controles de Polling
        Label lblUrl = new Label("Endpoint REST:");
        lblUrl.setFont(Font.font("Segoe UI", FontWeight.BOLD, 12.5));
        HBox.setHgrow(txtEndpoint, Priority.ALWAYS);

        btnUsarNginx.setOnAction(e -> {
            txtEndpoint.setText(VehiculosApiClient.URL_NGINX_DEFAULT);
            consultarNotificaciones();
        });
        btnUsarDirecto.setOnAction(e -> {
            txtEndpoint.setText(VehiculosApiClient.URL_DIRECTA_8081);
            consultarNotificaciones();
        });
        btnActualizarAhora.setStyle("-fx-background-color: #0284c7; -fx-text-fill: white; -fx-font-weight: bold;");
        btnActualizarAhora.setOnAction(e -> consultarNotificaciones());

        btnTogglePolling.setStyle("-fx-background-color: #0f766e; -fx-text-fill: white; -fx-font-weight: bold;");
        btnTogglePolling.setOnAction(e -> alternarPolling());

        HBox barraControles = new HBox(10,
                lblUrl, txtEndpoint, btnUsarNginx, btnUsarDirecto, btnActualizarAhora, btnTogglePolling
        );
        barraControles.setAlignment(Pos.CENTER_LEFT);
        barraControles.setPadding(new Insets(12, 16, 12, 16));
        barraControles.setStyle("-fx-background-color: #ffffff; -fx-border-color: #cbd5e1; -fx-border-radius: 10; -fx-background-radius: 10;");

        // 3. Tarjetas KPI
        VBox cardTotal = crearTarjetaKpi("TOTAL NOTIFICACIONES", lblKpiTotal, "Eventos recibidos desde RabbitMQ", "#0284c7");
        VBox cardUltima = crearTarjetaKpi("ÚLTIMA VENTA NOTIFICADA", lblKpiUltimaVenta, "Vinculada por ventaId y operacionId", "#16a34a");
        VBox cardCola = crearTarjetaKpi("COLA RABBITMQ", lblKpiCola, " Consumo asíncrono @RabbitListener", "#7c3aed");

        HBox filaKpis = new HBox(14, cardTotal, cardUltima, cardCola);
        HBox.setHgrow(cardTotal, Priority.ALWAYS);
        HBox.setHgrow(cardUltima, Priority.ALWAYS);
        HBox.setHgrow(cardCola, Priority.ALWAYS);

        // 4. Tabla de Notificaciones
        configurarColumnasTabla();
        tabla.setPlaceholder(new Label("0 notificaciones registradas. Cuando la PWA Móvil registre una venta, aparecerá automáticamente aquí."));
        tabla.getSelectionModel().selectedItemProperty().addListener((obs, anterior, actual) -> mostrarDetalle(actual));
        VBox.setVgrow(tabla, Priority.ALWAYS);

        // 5. Panel Inferior de Detalle del Evento
        Label lblDetalleTitulo = new Label("Detalle Completo del Evento Seleccionado (Payload RabbitMQ -> Notificación Persistida):");
        lblDetalleTitulo.setFont(Font.font("Segoe UI", FontWeight.BOLD, 12.5));

        txtDetallePayload.setEditable(false);
        txtDetallePayload.setPrefRowCount(4);
        txtDetallePayload.setWrapText(true);
        txtDetallePayload.setStyle("-fx-font-family: 'Consolas'; -fx-font-size: 12.5px;");
        txtDetallePayload.setText("Seleccione una notificación en la tabla para inspeccionar su eventoId, ventaId, operacionId, mensaje y fecha.");

        VBox panelDetalle = new VBox(6, lblDetalleTitulo, txtDetallePayload, lblEstado);
        panelDetalle.setPadding(new Insets(12, 16, 12, 16));
        panelDetalle.setStyle("-fx-background-color: #ffffff; -fx-border-color: #cbd5e1; -fx-border-radius: 10; -fx-background-radius: 10;");

        VBox raiz = new VBox(12, cabecera, barraControles, filaKpis, tabla, panelDetalle);
        raiz.setPadding(new Insets(16));
        raiz.setStyle("-fx-background-color: #f1f5f9;");

        ventana.setScene(new Scene(raiz, 1220, 740));
        ventana.setTitle("APE 04 — Monitor JavaFX en Tiempo Real de Notificaciones de Ventas (RabbitMQ)");
        ventana.show();

        iniciarPollingAutomatico();
        consultarNotificaciones();
    }

    private void configurarColumnasTabla() {
        agregarColumna("ID", NotificacionVenta::id, 55);
        agregarColumna("Venta ID", n -> n.ventaId() != null ? "#" + n.ventaId() : "—", 80);
        agregarColumna("Vehículo ID", n -> n.vehiculoId() != null ? "#" + n.vehiculoId() : "—", 90);
        agregarColumna("Comprador", n -> texto(n.compradorNombre()), 150);
        agregarColumna("Destinatario", n -> texto(n.destinatario()), 165);
        agregarColumna("Monto", n -> n.precioAcordado() != null ? "$" + n.precioAcordado() : "—", 95);
        agregarColumna("Moneda", n -> texto(n.moneda()), 70);
        agregarColumna("Tipo", n -> texto(n.tipo()), 140);
        agregarColumna("Canal", n -> texto(n.canal()), 95);
        agregarColumna("Estado", n -> texto(n.estado()), 100);
        agregarColumna("Evento ID", n -> texto(n.eventoId()), 190);
        agregarColumna("Fecha", n -> texto(n.fecha()), 175);
    }

    private void iniciarPollingAutomatico() {
        pollingTimeline = new Timeline(
                new KeyFrame(Duration.seconds(3), evento -> consultarNotificaciones())
        );
        pollingTimeline.setCycleCount(Animation.INDEFINITE);
        pollingTimeline.play();
    }

    private void alternarPolling() {
        if (pollingActivo) {
            pollingTimeline.pause();
            pollingActivo = false;
            btnTogglePolling.setText("Reanudar Polling (3s)");
            btnTogglePolling.setStyle("-fx-background-color: #b45309; -fx-text-fill: white; -fx-font-weight: bold;");
            mostrarEstado("Polling automático en pausa. Use 'Actualizar Ahora' o reanude el monitor.", false);
        } else {
            pollingTimeline.play();
            pollingActivo = true;
            btnTogglePolling.setText("Pausar Polling (3s)");
            btnTogglePolling.setStyle("-fx-background-color: #0f766e; -fx-text-fill: white; -fx-font-weight: bold;");
            consultarNotificaciones();
        }
    }

    private void consultarNotificaciones() {
        if (consultaEnCurso) {
            return;
        }
        consultaEnCurso = true;
        btnActualizarAhora.setDisable(true);
        String url = txtEndpoint.getText().trim();

        api.consultarNotificaciones(url).whenComplete((resultado, error) ->
                Platform.runLater(() -> {
                    consultaEnCurso = false;
                    btnActualizarAhora.setDisable(false);
                    String horaActual = LocalTime.now().format(FORMATO_HORA);

                    if (error != null) {
                        estilizarBadgeConexion("#991b1b", "#fef2f2", "SIN CONEXIÓN / FALLO DE SERVICIO");
                        tabla.setPlaceholder(new Label(
                                "ERROR DE CONEXIÓN: No se pudo contactar con el servicio de notificaciones en " + url
                        ));
                        mostrarEstado(
                                "[" + horaActual + "] Fallo de red/servicio al consultar " + url + ": verifique NGINX (8090) o Notificaciones (8081).",
                                true
                        );
                        return;
                    }

                    int codigo = resultado.codigoHttp();
                    if (codigo != 200) {
                        estilizarBadgeConexion("#991b1b", "#fef2f2", "FALLO DE SERVICIO (HTTP " + codigo + ")");
                        tabla.setPlaceholder(new Label("El servidor respondió con error HTTP " + codigo));
                        mostrarEstado(
                                "[" + horaActual + "] HTTP " + codigo + " recibido desde " + url + ".",
                                true
                        );
                        return;
                    }

                    List<NotificacionVenta> lista = resultado.notificaciones();
                    estilizarBadgeConexion("#065f46", "#ecfdf5", "CONECTADO (HTTP 200)");

                    NotificacionVenta seleccionPrevia = tabla.getSelectionModel().getSelectedItem();
                    Integer idPrevio = seleccionPrevia != null ? seleccionPrevia.id() : null;

                    tabla.getItems().setAll(lista);
                    lblKpiTotal.setText(String.valueOf(lista.size()));

                    if (lista.isEmpty()) {
                        lblKpiUltimaVenta.setText("0 notificaciones registradas");
                        tabla.setPlaceholder(new Label(
                                "HTTP 200 OK: 0 notificaciones registradas en este momento (la lista está vacía, sin error de servicio)."
                        ));
                        mostrarEstado(
                                "[" + horaActual + "] HTTP 200 OK · Servicio activo · 0 notificaciones registradas.",
                                false
                        );
                    } else {
                        NotificacionVenta primera = lista.get(0);
                        String resumenUltima = primera.ventaId() != null
                                ? "Venta #" + primera.ventaId() + " (" + texto(primera.compradorNombre()) + " · $" + texto(primera.precioAcordado()) + ")"
                                : "Evento " + texto(primera.tipo()) + " (Vehículo #" + texto(primera.vehiculoId()) + ")";
                        lblKpiUltimaVenta.setText(resumenUltima);

                        if (idPrevio != null) {
                            for (NotificacionVenta item : lista) {
                                if (idPrevio.equals(item.id())) {
                                    tabla.getSelectionModel().select(item);
                                    break;
                                }
                            }
                        }
                        if (tabla.getSelectionModel().getSelectedItem() == null) {
                            tabla.getSelectionModel().selectFirst();
                        }

                        mostrarEstado(
                                "[" + horaActual + "] HTTP 200 OK · Sincronizadas " + lista.size()
                                        + " notificación(es) en tiempo real desde " + url
                                        + (pollingActivo ? " (Polling automático 3s activo)." : "."),
                                false
                        );
                    }
                })
        );
    }

    private void mostrarDetalle(NotificacionVenta n) {
        if (n == null) {
            txtDetallePayload.setText("Seleccione una notificación en la tabla para inspeccionar su payload completo.");
            return;
        }
        String detalle = String.format(
                "eventoId: %s  |  ventaId: %s  |  operacionId: %s  |  vehiculoId: %s%n"
                        + "comprador: %s  |  destinatario: %s  |  monto: %s %s  |  canal: %s  |  estado: %s%n"
                        + "mensaje: %s  |  fecha: %s",
                texto(n.eventoId()),
                texto(n.ventaId()),
                texto(n.operacionId()),
                texto(n.vehiculoId()),
                texto(n.compradorNombre()),
                texto(n.destinatario()),
                texto(n.precioAcordado()),
                texto(n.moneda()),
                texto(n.canal()),
                texto(n.estado()),
                texto(n.mensaje()),
                texto(n.fecha())
        );
        txtDetallePayload.setText(detalle);
    }

    private VBox crearTarjetaKpi(String titulo, Label lblValor, String subtitulo, String colorBorde) {
        Label lblTit = new Label(titulo);
        lblTit.setFont(Font.font("Segoe UI", FontWeight.BOLD, 11));
        lblTit.setTextFill(Color.web("#64748b"));

        lblValor.setFont(Font.font("Segoe UI", FontWeight.BOLD, 17));
        lblValor.setTextFill(Color.web("#0f172a"));

        Label lblSub = new Label(subtitulo);
        lblSub.setFont(Font.font("Segoe UI", 11.5));
        lblSub.setTextFill(Color.web("#475569"));

        VBox card = new VBox(4, lblTit, lblValor, lblSub);
        card.setPadding(new Insets(12, 16, 12, 16));
        card.setStyle(
                "-fx-background-color: #ffffff; -fx-background-radius: 10; -fx-border-radius: 10; "
                        + "-fx-border-color: " + colorBorde + "; -fx-border-width: 0 0 0 5;"
        );
        return card;
    }

    private void estilizarBadgeConexion(String bgHex, String textHex, String textoBadge) {
        lblBadgeConexion.setText(textoBadge);
        lblBadgeConexion.setFont(Font.font("Segoe UI", FontWeight.BOLD, 12));
        lblBadgeConexion.setPadding(new Insets(8, 14, 8, 14));
        lblBadgeConexion.setStyle(
                "-fx-background-color: " + bgHex + "; -fx-text-fill: " + textHex
                        + "; -fx-background-radius: 999; -fx-border-radius: 999; -fx-border-color: " + textHex + ";"
        );
    }

    private void mostrarEstado(String texto, boolean esError) {
        lblEstado.setText(texto);
        lblEstado.setFont(Font.font("Segoe UI", FontWeight.BOLD, 12));
        lblEstado.setTextFill(esError ? Color.web("#b91c1c") : Color.web("#0f766e"));
    }

    private static String texto(Object valor) {
        if (valor == null) return "—";
        String s = valor.toString().trim();
        return s.isEmpty() ? "—" : s;
    }

    @Override
    public void stop() {
        if (pollingTimeline != null) {
            pollingTimeline.stop();
        }
    }

    private <T> void agregarColumna(String titulo, Function<NotificacionVenta, T> extractor, double ancho) {
        TableColumn<NotificacionVenta, T> columna = new TableColumn<>(titulo);
        columna.setCellValueFactory(fila -> new ReadOnlyObjectWrapper<>(extractor.apply(fila.getValue())));
        columna.setPrefWidth(ancho);
        tabla.getColumns().add(columna);
    }
}
