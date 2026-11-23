package ec.edu.uta.notificaciones.model;

import java.math.BigDecimal;
import jakarta.persistence.*;

@Entity
@Table(name = "notificaciones_ventas")
public class Notificacion {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Integer id;

    @Column(name = "evento_id", nullable = false, unique = true, length = 100)
    private String eventoId;

    @Column(name = "venta_id")
    private Integer ventaId;

    @Column(name = "operacion_id", length = 64)
    private String operacionId;

    @Column(name = "vehiculo_id")
    private Integer vehiculoId;

    @Column(name = "comprador_nombre", length = 150)
    private String compradorNombre;

    @Column(length = 200)
    private String destinatario;

    @Column(name = "precio_acordado", precision = 12, scale = 2)
    private BigDecimal precioAcordado;

    @Column(length = 10)
    private String moneda;

    @Column(length = 40)
    private String tipo;

    @Column(length = 300)
    private String mensaje;

    @Column(length = 30)
    private String estado;

    @Column(length = 30)
    private String canal;

    @Column(length = 50)
    private String fecha;

    public Notificacion() {
    }

    public Integer getId() {
        return id;
    }

    public void setId(Integer id) {
        this.id = id;
    }

    public String getEventoId() {
        return eventoId;
    }

    public void setEventoId(String eventoId) {
        this.eventoId = eventoId;
    }

    public Integer getVentaId() {
        return ventaId;
    }

    public void setVentaId(Integer ventaId) {
        this.ventaId = ventaId;
    }

    public String getOperacionId() {
        return operacionId;
    }

    public void setOperacionId(String operacionId) {
        this.operacionId = operacionId;
    }

    public Integer getVehiculoId() {
        return vehiculoId;
    }

    public void setVehiculoId(Integer vehiculoId) {
        this.vehiculoId = vehiculoId;
    }

    public String getCompradorNombre() {
        return compradorNombre;
    }

    public void setCompradorNombre(String compradorNombre) {
        this.compradorNombre = compradorNombre;
    }

    public String getDestinatario() {
        return destinatario;
    }

    public void setDestinatario(String destinatario) {
        this.destinatario = destinatario;
    }

    public BigDecimal getPrecioAcordado() {
        return precioAcordado;
    }

    public void setPrecioAcordado(BigDecimal precioAcordado) {
        this.precioAcordado = precioAcordado;
    }

    public String getMoneda() {
        return moneda;
    }

    public void setMoneda(String moneda) {
        this.moneda = moneda;
    }

    public String getTipo() {
        return tipo;
    }

    public void setTipo(String tipo) {
        this.tipo = tipo;
    }

    public String getMensaje() {
        return mensaje;
    }

    public void setMensaje(String mensaje) {
        this.mensaje = mensaje;
    }

    public String getEstado() {
        return estado;
    }

    public void setEstado(String estado) {
        this.estado = estado;
    }

    public String getCanal() {
        return canal;
    }

    public void setCanal(String canal) {
        this.canal = canal;
    }

    public String getFecha() {
        return fecha;
    }

    public void setFecha(String fecha) {
        this.fecha = fecha;
    }
}
