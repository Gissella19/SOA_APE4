using System.Text.Json.Serialization;

namespace ConsultaVehiculosWinForms;

public sealed class Vehiculo
{
    [JsonPropertyName("idVehiculo")]
    public int IdVehiculo { get; set; }

    [JsonPropertyName("marca")]
    public string Marca { get; set; } = "";

    [JsonPropertyName("modelo")]
    public string Modelo { get; set; } = "";

    [JsonPropertyName("placa")]
    public string Placa { get; set; } = "";

    [JsonPropertyName("chasis")]
    public string Chasis { get; set; } = "";

    [JsonPropertyName("anio")]
    public int Anio { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = "";

    [JsonPropertyName("precio")]
    public decimal Precio { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = "";
}

public sealed class VehiculoRequest
{
    [JsonPropertyName("marca")]
    public string Marca { get; set; } = "";

    [JsonPropertyName("modelo")]
    public string Modelo { get; set; } = "";

    [JsonPropertyName("placa")]
    public string Placa { get; set; } = "";

    [JsonPropertyName("chasis")]
    public string Chasis { get; set; } = "";

    [JsonPropertyName("anio")]
    public int Anio { get; set; }

    [JsonPropertyName("color")]
    public string Color { get; set; } = "";

    [JsonPropertyName("precio")]
    public decimal Precio { get; set; }
}

public sealed class Comprador
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("documento")]
    public string Documento { get; set; } = "";

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";

    [JsonPropertyName("correo")]
    public string Correo { get; set; } = "";

    [JsonPropertyName("usuario")]
    public string Usuario { get; set; } = "";
}

public sealed class CrearCompradorRequest
{
    [JsonPropertyName("documento")]
    public string Documento { get; set; } = "";

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";

    [JsonPropertyName("correo")]
    public string Correo { get; set; } = "";
}

public sealed class Venta
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("operacionId")]
    public string OperacionId { get; set; } = "";

    [JsonPropertyName("vehiculoId")]
    public int VehiculoId { get; set; }

    [JsonPropertyName("compradorId")]
    public int CompradorId { get; set; }

    [JsonPropertyName("compradorNombre")]
    public string? CompradorNombre { get; set; }

    [JsonPropertyName("compradorDocumento")]
    public string? CompradorDocumento { get; set; }

    [JsonPropertyName("precioAcordado")]
    public string? PrecioAcordado { get; set; }

    [JsonPropertyName("moneda")]
    public string? Moneda { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = "";

    [JsonPropertyName("notificacion")]
    public string Notificacion { get; set; } = "";

    [JsonPropertyName("usuario")]
    public string Usuario { get; set; } = "";

    [JsonPropertyName("fechaUTC")]
    public string FechaUTC { get; set; } = "";
}

public sealed class UsuarioApiDto
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("rol")]
    public string Rol { get; set; } = "";
}

public sealed class CrearUsuarioRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = "";

    [JsonPropertyName("password")]
    public string Password { get; set; } = "";

    [JsonPropertyName("rol")]
    public string Rol { get; set; } = "";
}

public sealed class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "";

    [JsonPropertyName("expires_in")]
    public long ExpiresIn { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "";
}

public sealed class IdentidadResponse
{
    [JsonPropertyName("usuario")]
    public string Usuario { get; set; } = "";

    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; } = new();
}

public sealed class ApiResultado<T>
{
    public int CodigoHttp { get; init; }
    public string EstadoHttpTexto { get; init; } = "";
    public bool Exito { get; init; }
    public T? Datos { get; init; }
    public string Mensaje { get; init; } = "";
}
