using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ConsultaVehiculosWinForms;

/// <summary>
/// Servicio dedicado de comunicación HTTP hacia el Gateway NGINX (http://127.0.0.1:8090/).
/// Separa la lógica de transporte HTTP de la interfaz gráfica Windows Forms.
/// Cumple estrictamente el contrato arquitectónico: NO incluye creación de ventas (POST /api/ventas).
/// </summary>
public sealed class ApiService
{
    private const string ClientId = "vehiculos-client";
    private const string ClientSecret = "cliente-local-aula-2026-cambiar";

    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(15)
    };

    private static readonly JsonSerializerOptions JsonOpciones = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static string NormalizarBaseUrl(string? baseUrl)
    {
        string limpia = string.IsNullOrWhiteSpace(baseUrl)
            ? "http://127.0.0.1:8090/"
            : baseUrl.Trim();
        if (!limpia.EndsWith('/'))
        {
            limpia += "/";
        }
        return limpia;
    }

    public static string NormalizarToken(string? rawToken)
    {
        string token = (rawToken ?? "").Trim();
        if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            token = token[7..].Trim();
        }
        return token;
    }

    public async Task<ApiResultado<TokenResponse>> IniciarSesionAsync(
        string baseUrl, string username, string password)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}oauth/token";
        using var req = new HttpRequestMessage(HttpMethod.Post, url);

        string basicRaw = $"{ClientId}:{ClientSecret}";
        string basicB64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(basicRaw));
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicB64);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["username"] = username.Trim(),
            ["password"] = password.Trim()
        });

        return await EjecutarAsync<TokenResponse>(req);
    }

    public async Task<ApiResultado<IdentidadResponse>> ObtenerIdentidadAsync(
        string baseUrl, string token)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/identidad";
        using var req = CrearSolicitud(HttpMethod.Get, url, token);
        return await EjecutarAsync<IdentidadResponse>(req);
    }

    public async Task<ApiResultado<List<Vehiculo>>> ListarVehiculosAsync(
        string baseUrl, string token, string? marca = null, string? estado = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(marca))
        {
            query.Add($"marca={Uri.EscapeDataString(marca.Trim())}");
        }
        if (!string.IsNullOrWhiteSpace(estado) && !estado.Equals("TODOS", StringComparison.OrdinalIgnoreCase))
        {
            query.Add($"estado={Uri.EscapeDataString(estado.Trim().ToUpperInvariant())}");
        }

        string sufijo = query.Count > 0 ? "?" + string.Join("&", query) : "";
        string url = $"{NormalizarBaseUrl(baseUrl)}api/vehiculos{sufijo}";
        using var req = CrearSolicitud(HttpMethod.Get, url, token);
        return await EjecutarAsync<List<Vehiculo>>(req);
    }

    public async Task<ApiResultado<Vehiculo>> CrearVehiculoAsync(
        string baseUrl, string token, VehiculoRequest payload)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/vehiculos";
        using var req = CrearSolicitudJson(HttpMethod.Post, url, token, payload);
        return await EjecutarAsync<Vehiculo>(req);
    }

    public async Task<ApiResultado<Vehiculo>> ActualizarVehiculoAsync(
        string baseUrl, string token, int idVehiculo, VehiculoRequest payload)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/vehiculos/{idVehiculo}";
        using var req = CrearSolicitudJson(HttpMethod.Put, url, token, payload);
        return await EjecutarAsync<Vehiculo>(req);
    }

    public async Task<ApiResultado<bool>> EliminarVehiculoAsync(
        string baseUrl, string token, int idVehiculo)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/vehiculos/{idVehiculo}";
        using var req = CrearSolicitud(HttpMethod.Delete, url, token);
        return await EjecutarSinCuerpoAsync(req);
    }

    public async Task<ApiResultado<List<Comprador>>> ListarCompradoresAsync(
        string baseUrl, string token)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/compradores";
        using var req = CrearSolicitud(HttpMethod.Get, url, token);
        return await EjecutarAsync<List<Comprador>>(req);
    }

    public async Task<ApiResultado<Comprador>> CrearCompradorAsync(
        string baseUrl, string token, CrearCompradorRequest payload)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/compradores";
        using var req = CrearSolicitudJson(HttpMethod.Post, url, token, payload);
        return await EjecutarAsync<Comprador>(req);
    }

    public async Task<ApiResultado<List<Venta>>> ListarVentasAsync(
        string baseUrl, string token)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/ventas";
        using var req = CrearSolicitud(HttpMethod.Get, url, token);
        return await EjecutarAsync<List<Venta>>(req);
    }

    public async Task<ApiResultado<Venta>> ObtenerVentaPorIdAsync(
        string baseUrl, string token, int ventaId)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/ventas/{ventaId}";
        using var req = CrearSolicitud(HttpMethod.Get, url, token);
        return await EjecutarAsync<Venta>(req);
    }

    public async Task<ApiResultado<List<UsuarioApiDto>>> ListarUsuariosAsync(
        string baseUrl, string token)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/usuarios";
        using var req = CrearSolicitud(HttpMethod.Get, url, token);
        return await EjecutarAsync<List<UsuarioApiDto>>(req);
    }

    public async Task<ApiResultado<UsuarioApiDto>> CrearUsuarioAsync(
        string baseUrl, string token, CrearUsuarioRequest payload)
    {
        string url = $"{NormalizarBaseUrl(baseUrl)}api/usuarios";
        using var req = CrearSolicitudJson(HttpMethod.Post, url, token, payload);
        return await EjecutarAsync<UsuarioApiDto>(req);
    }

    private static HttpRequestMessage CrearSolicitud(HttpMethod metodo, string url, string rawToken)
    {
        var req = new HttpRequestMessage(metodo, url);
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        string token = NormalizarToken(rawToken);
        if (!string.IsNullOrEmpty(token))
        {
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return req;
    }

    private static HttpRequestMessage CrearSolicitudJson<TPayload>(
        HttpMethod metodo, string url, string rawToken, TPayload payload)
    {
        var req = CrearSolicitud(metodo, url, rawToken);
        string json = JsonSerializer.Serialize(payload);
        req.Content = new StringContent(json, Encoding.UTF8, "application/json");
        return req;
    }

    private static async Task<ApiResultado<T>> EjecutarAsync<T>(HttpRequestMessage solicitud)
    {
        try
        {
            using HttpResponseMessage respuesta = await Http.SendAsync(solicitud);
            int codigo = (int)respuesta.StatusCode;
            string etiqueta = FormatearCodigoHttp(respuesta.StatusCode);
            string cuerpo = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                string detalleError = ExtraerDetalleError(cuerpo, codigo);
                return new ApiResultado<T>
                {
                    CodigoHttp = codigo,
                    EstadoHttpTexto = etiqueta,
                    Exito = false,
                    Mensaje = $"HTTP {etiqueta}: {detalleError}"
                };
            }

            if (string.IsNullOrWhiteSpace(cuerpo))
            {
                return new ApiResultado<T>
                {
                    CodigoHttp = codigo,
                    EstadoHttpTexto = etiqueta,
                    Exito = true,
                    Mensaje = $"HTTP {etiqueta}: Operación completada."
                };
            }

            T? datos = JsonSerializer.Deserialize<T>(cuerpo, JsonOpciones);
            return new ApiResultado<T>
            {
                CodigoHttp = codigo,
                EstadoHttpTexto = etiqueta,
                Exito = true,
                Datos = datos,
                Mensaje = $"HTTP {etiqueta}: Operación realizada correctamente."
            };
        }
        catch (HttpRequestException ex)
        {
            return new ApiResultado<T>
            {
                CodigoHttp = 503,
                EstadoHttpTexto = "503 Service Unavailable",
                Exito = false,
                Mensaje = $"HTTP 502/503 Error de conexión con NGINX/Backend: {ex.Message}"
            };
        }
        catch (TaskCanceledException)
        {
            return new ApiResultado<T>
            {
                CodigoHttp = 504,
                EstadoHttpTexto = "504 Gateway Timeout",
                Exito = false,
                Mensaje = "HTTP 504 Timeout: El servidor no respondió dentro de 15 segundos."
            };
        }
        catch (JsonException)
        {
            return new ApiResultado<T>
            {
                CodigoHttp = 502,
                EstadoHttpTexto = "502 Bad Gateway",
                Exito = false,
                Mensaje = "HTTP 502: La respuesta recibida no contiene un formato JSON válido."
            };
        }
    }

    private static async Task<ApiResultado<bool>> EjecutarSinCuerpoAsync(HttpRequestMessage solicitud)
    {
        try
        {
            using HttpResponseMessage respuesta = await Http.SendAsync(solicitud);
            int codigo = (int)respuesta.StatusCode;
            string etiqueta = FormatearCodigoHttp(respuesta.StatusCode);
            string cuerpo = await respuesta.Content.ReadAsStringAsync();

            if (!respuesta.IsSuccessStatusCode)
            {
                string detalleError = ExtraerDetalleError(cuerpo, codigo);
                return new ApiResultado<bool>
                {
                    CodigoHttp = codigo,
                    EstadoHttpTexto = etiqueta,
                    Exito = false,
                    Datos = false,
                    Mensaje = $"HTTP {etiqueta}: {detalleError}"
                };
            }

            return new ApiResultado<bool>
            {
                CodigoHttp = codigo,
                EstadoHttpTexto = etiqueta,
                Exito = true,
                Datos = true,
                Mensaje = $"HTTP {etiqueta}: Registro eliminado correctamente."
            };
        }
        catch (HttpRequestException ex)
        {
            return new ApiResultado<bool>
            {
                CodigoHttp = 503,
                EstadoHttpTexto = "503 Service Unavailable",
                Exito = false,
                Datos = false,
                Mensaje = $"HTTP 502/503 Error de conexión con NGINX/Backend: {ex.Message}"
            };
        }
        catch (TaskCanceledException)
        {
            return new ApiResultado<bool>
            {
                CodigoHttp = 504,
                EstadoHttpTexto = "504 Gateway Timeout",
                Exito = false,
                Datos = false,
                Mensaje = "HTTP 504 Timeout: El servidor no respondió dentro de 15 segundos."
            };
        }
    }

    private static string FormatearCodigoHttp(HttpStatusCode status)
    {
        int code = (int)status;
        return code switch
        {
            200 => "200 OK",
            201 => "201 Created",
            202 => "202 Accepted",
            204 => "204 No Content",
            400 => "400 Bad Request",
            401 => "401 Unauthorized",
            403 => "403 Forbidden",
            404 => "404 Not Found",
            409 => "409 Conflict",
            422 => "422 Unprocessable Entity",
            500 => "500 Internal Server Error",
            502 => "502 Bad Gateway",
            503 => "503 Service Unavailable",
            504 => "504 Gateway Timeout",
            _ => $"{code} {status}"
        };
    }

    private static string ExtraerDetalleError(string cuerpo, int codigo)
    {
        if (!string.IsNullOrWhiteSpace(cuerpo))
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(cuerpo);
                JsonElement root = doc.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("detail", out JsonElement detail) && detail.ValueKind == JsonValueKind.String)
                        return detail.GetString()!;
                    if (root.TryGetProperty("mensaje", out JsonElement mensaje) && mensaje.ValueKind == JsonValueKind.String)
                        return mensaje.GetString()!;
                    if (root.TryGetProperty("message", out JsonElement message) && message.ValueKind == JsonValueKind.String)
                        return message.GetString()!;
                    if (root.TryGetProperty("title", out JsonElement title) && title.ValueKind == JsonValueKind.String)
                        return title.GetString()!;
                    if (root.TryGetProperty("error", out JsonElement err) && err.ValueKind == JsonValueKind.String)
                        return err.GetString()!;
                }
            }
            catch
            {
                // Ignorar error de parseo y usar mensaje por código.
            }
        }

        return codigo switch
        {
            400 => "Solicitud inválida o parámetros mal formados.",
            401 => "Token JWT ausente, inválido o vencido.",
            403 => "Acceso denegado: el rol actual no tiene permisos para esta operación.",
            404 => "El recurso solicitado no existe.",
            409 => "Conflicto de negocio o registro duplicado.",
            422 => "Error de validación en los campos enviados.",
            502 or 503 => "Servicio backend no disponible a través de NGINX.",
            _ => "La operación no pudo completarse."
        };
    }
}
