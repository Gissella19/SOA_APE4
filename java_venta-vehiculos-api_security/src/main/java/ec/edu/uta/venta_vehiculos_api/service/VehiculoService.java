package ec.edu.uta.venta_vehiculos_api.service; // Paquete de servicios.

// Permite manejar precios con precisión decimal.
import java.math.BigDecimal;

// Permite obtener el año actual.
import java.time.Year;

// Importa List, Map y Locale.
import java.util.*;

// Identifica la clase como servicio de Spring Boot.
import org.springframework.stereotype.Service;

// Permite manejar transacciones con MySQL.
import org.springframework.transaction.annotation.Transactional;

// Importa los DTO de entrada y salida.
import ec.edu.uta.venta_vehiculos_api.dto.*;

// Importa la clase personalizada de errores.
import ec.edu.uta.venta_vehiculos_api.exception.ApiError;

// Importa la entidad Vehiculo.
import ec.edu.uta.venta_vehiculos_api.model.Vehiculo;

// Importa los repositorios para acceder a MySQL.
import ec.edu.uta.venta_vehiculos_api.repository.*;

// Indica que esta clase contiene lógica de negocio.
@Service

// Ejecuta sus operaciones dentro de transacciones.
// Si ocurre un error de ejecución, la transacción se revierte.
@Transactional
public class VehiculoService {

    // Repositorio para consultar y modificar vehículos.
    private final VehiculoRepository repo;

    // Repositorio para consultar las reservas.
    private final ReservaRepository reservas;

    // Constructor que recibe los repositorios mediante Spring.
    public VehiculoService(
            VehiculoRepository repo,
            ReservaRepository reservas) {

        // Guarda el repositorio de vehículos.
        this.repo = repo;

        // Guarda el repositorio de reservas.
        this.reservas = reservas;
    }

    // Indica que esta operación solamente consulta información.
    @Transactional(readOnly = true)
    public List<VehiculoResponse> listar(String marca) {
        return listar(marca, null);
    }

    // Lista vehículos sin filtro, filtrados por marca, por estado o por ambos.
    @Transactional(readOnly = true)
    public List<VehiculoResponse> listar(String marca, String estado) {
        String marcaLimpia = (marca != null && !marca.trim().isEmpty()) ? marca.trim() : null;
        String estadoLimpio = (estado != null && !estado.trim().isEmpty()) ? estado.trim().toUpperCase(Locale.ROOT) : null;

        List<Vehiculo> lista;
        if (marcaLimpia != null && estadoLimpio != null) {
            lista = repo.findByMarcaContainingIgnoreCaseAndEstadoIgnoreCase(marcaLimpia, estadoLimpio);
        } else if (marcaLimpia != null) {
            lista = repo.findByMarcaContainingIgnoreCase(marcaLimpia);
        } else if (estadoLimpio != null) {
            lista = repo.findByEstadoIgnoreCase(estadoLimpio);
        } else {
            lista = repo.findAll();
        }

        // Convierte cada entidad Vehiculo en un DTO de respuesta.
        return lista.stream()
                .map(VehiculoResponse::de)
                .toList();
    }

    // Transacción utilizada solamente para consultar.
    @Transactional(readOnly = true)
    public VehiculoResponse obtener(int id) {

        // Busca el vehículo mediante su identificador.
        // Si no existe, genera un error 404.
        // Si existe, convierte la entidad en un DTO.
        return VehiculoResponse.de(
                repo.findById(id)
                    .orElseThrow(() -> falta())
        );
    }

    // Registra un nuevo vehículo en MySQL.
    public VehiculoResponse crear(VehiculoRequest d) {

        // Crea un objeto Vehiculo vacío.
        Vehiculo v = new Vehiculo();

        // Copia y valida los datos recibidos del DTO.
        copiar(v, d);

        // Todo vehículo nuevo inicia como DISPONIBLE.
        v.setEstado("DISPONIBLE");

        // Guarda el vehículo en MySQL.
        // flush sincroniza los cambios con la base de datos.
        // Convierte el resultado en un DTO de respuesta.
        return VehiculoResponse.de(repo.saveAndFlush(v));
    }

    // Actualiza los datos de un vehículo existente.
    public VehiculoResponse actualizar(int id, VehiculoRequest d) {

        // Busca el vehículo y comprueba que pueda modificarse.
        Vehiculo v = editable(id);

        // Copia los nuevos datos al vehículo.
        copiar(v, d);

        // Guarda los cambios y devuelve el DTO actualizado.
        return VehiculoResponse.de(repo.saveAndFlush(v));
    }

    // Modifica únicamente el precio mediante PATCH.
    public VehiculoResponse precio(int id, Map<String, Object> d) {
        return parchear(id, d);
    }

    // Modifica únicamente el precio mediante PATCH normalizando antes de validar.
    public VehiculoResponse parchear(int id, Map<String, Object> d) {

        // Comprueba que el JSON tenga únicamente el campo precio y no sea nulo.
        if (d == null || d.size() != 1 || !d.containsKey("precio") || d.get("precio") == null) {
            throw new ApiError(
                    422,
                    "PATCH_INVALIDO",
                    "PATCH admite únicamente precio numérico y no nulo"
            );
        }

        Object valorPrecio = d.get("precio");
        String textoPrecio = valorPrecio instanceof String s ? s.trim() : valorPrecio.toString().trim();
        if (!(valorPrecio instanceof Number) && !(valorPrecio instanceof String)) {
            throw new ApiError(
                    422,
                    "PATCH_INVALIDO",
                    "PATCH admite únicamente precio numérico y no nulo"
            );
        }

        BigDecimal precio;
        try {
            precio = new BigDecimal(textoPrecio);
        } catch (NumberFormatException e) {
            throw new ApiError(
                    422,
                    "PRECIO_INVALIDO",
                    "Precio inválido"
            );
        }

        if (precio.signum() <= 0 ||
                precio.scale() > 2 ||
                precio.compareTo(new BigDecimal("9999999999.99")) > 0) {
            throw new ApiError(
                    422,
                    "PRECIO_INVALIDO",
                    "Precio positivo con máximo dos decimales"
            );
        }

        // Busca el vehículo y comprueba que esté disponible.
        Vehiculo v = editable(id);

        // Asigna el nuevo precio.
        v.setPrecio(precio);

        // Guarda el nuevo precio y devuelve el resultado.
        return VehiculoResponse.de(repo.saveAndFlush(v));
    }

    // Elimina un vehículo de MySQL si cumple las reglas.
    public void eliminar(int id) {

        // Busca el vehículo y verifica que esté DISPONIBLE.
        Vehiculo v = editable(id);

        // Comprueba si existen reservas asociadas al vehículo.
        if (reservas.existsByVehiculo_IdVehiculo(id)) {

            // No permite eliminar vehículos con historial de reservas.
            throw new ApiError(
                    409,
                    "HISTORIAL_RESERVAS",
                    "Se conserva el vehículo por su historial de reservas"
            );
        }

        // Solicita eliminar el vehículo de MySQL.
        repo.delete(v);

        // Sincroniza la eliminación con la base de datos.
        repo.flush();
    }

    // Método privado que verifica si un vehículo puede editarse.
    private Vehiculo editable(int id) {

        // Busca el vehículo y bloquea su registro durante
        // la transacción para controlar cambios simultáneos.
        // Si no existe, genera un error 404.
        Vehiculo v = repo.bloquear(id)
                .orElseThrow(() -> falta());

        // Comprueba que el vehículo esté DISPONIBLE.
        if (!"DISPONIBLE".equals(v.getEstado())) {

            // Impide modificar vehículos RESERVADOS o VENDIDOS.
            throw new ApiError(
                    409,
                    "VEHICULO_NO_DISPONIBLE",
                    "Un vehículo reservado o vendido no se modifica ni elimina"
            );
        }

        // Devuelve el vehículo que puede modificarse.
        return v;
    }

    // Método privado que construye un error 404.
    private ApiError falta() {

        // Devuelve un error indicando que no existe el vehículo.
        return new ApiError(
                404,
                "VEHICULO_NO_ENCONTRADO",
                "El vehículo no existe"
        );
    }

    // Copia los datos del DTO a la entidad Vehiculo normalizando antes de validar.
    private void copiar(Vehiculo v, VehiculoRequest d) {

        String marca = d.marca() != null ? d.marca().trim() : "";
        String modelo = d.modelo() != null ? d.modelo().trim() : "";
        String placa = d.placa() != null ? d.placa().trim().toUpperCase(Locale.ROOT) : "";
        String chasis = d.chasis() != null ? d.chasis().trim().toUpperCase(Locale.ROOT) : "";
        String color = d.color() != null ? d.color().trim() : null;

        if (marca.isEmpty() || modelo.isEmpty()) {
            throw new ApiError(
                    422,
                    "DATOS_INVALIDOS",
                    "Marca y modelo son obligatorios"
            );
        }

        // Valida formato de placa (ej. ABC-1234, ABC-123 o ABC1234).
        if (!placa.matches("^[A-Z]{3}-?[0-9]{3,4}$")) {
            throw new ApiError(
                    422,
                    "PLACA_INVALIDA",
                    "Formato de placa inválido (ejemplo: ABC-1234)"
            );
        }

        // Comprueba que el chasis tenga exactamente 17 caracteres alfanuméricos.
        if (!chasis.matches("^[A-Z0-9]{17}$")) {
            throw new ApiError(
                    422,
                    "CHASIS_INVALIDO",
                    "Chasis debe contener exactamente 17 caracteres alfanuméricos"
            );
        }

        // Comprueba que el año sea desde 1900 hasta el año siguiente al actual.
        if (d.anio() == null || d.anio() < 1900 ||
                d.anio() > Year.now().getValue() + 1) {

            throw new ApiError(
                    422,
                    "ANIO_INVALIDO",
                    "Año fuera de rango"
            );
        }

        // Asigna los campos ya normalizados y validados.
        v.setMarca(marca);
        v.setModelo(modelo);
        v.setPlaca(placa);
        v.setChasis(chasis);
        v.setAnio(d.anio());
        v.setColor(color);
        v.setPrecio(d.precio());
    }

} // Finaliza la clase VehiculoService.
