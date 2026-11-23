namespace ConsultaVehiculosWinForms;

public partial class FormPrincipal : Form
{
    private readonly ApiService _api = new();
    private static readonly Random _random = new();

    public FormPrincipal()
    {
        InitializeComponent();
        GenerarPlacaChasisAleatorios();
    }

    private string BaseUrl => txtBaseUrl.Text.Trim();
    private string TokenActual => ApiService.NormalizarToken(txtToken.Text);

    private async void CargarPresetUsuario(string username, string password)
    {
        txtLoginUser.Text = username;
        txtLoginPass.Text = password;
        await AutenticarUsuarioAsync();
    }

    private async void btnLogin_Click(object? sender, EventArgs e)
    {
        await AutenticarUsuarioAsync();
    }

    private async Task AutenticarUsuarioAsync()
    {
        string user = txtLoginUser.Text.Trim();
        string pass = txtLoginPass.Text.Trim();

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            MostrarEstado("HTTP 400 Bad Request: Ingrese usuario y contraseña para solicitar un token JWT.", esError: true);
            return;
        }

        btnLogin.Enabled = false;
        MostrarEstado($"Autenticando '{user}' en POST /oauth/token vía NGINX...");

        try
        {
            ApiResultado<TokenResponse> res = await _api.IniciarSesionAsync(BaseUrl, user, pass);
            if (!res.Exito || res.Datos is null || string.IsNullOrWhiteSpace(res.Datos.AccessToken))
            {
                lblIdentidadActiva.Text = $"Usuario activo: (autenticación fallida) | Rol: — | HTTP {res.CodigoHttp}";
                lblIdentidadActiva.ForeColor = Color.FromArgb(248, 113, 113);
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            txtToken.Text = res.Datos.AccessToken;
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Token JWT emitido para '{user}'. Verificando identidad activa...");
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
        }
        finally
        {
            btnLogin.Enabled = true;
        }
    }

    private void btnToggleToken_Click(object? sender, EventArgs e)
    {
        bool oculto = txtToken.UseSystemPasswordChar;
        txtToken.UseSystemPasswordChar = !oculto;
        btnToggleToken.Text = oculto ? "Ocultar JWT" : "Mostrar JWT";
    }

    private async void btnVerificarIdentidad_Click(object? sender, EventArgs e)
    {
        await ActualizarIdentidadActivaAsync(mostrarMensajeExito: true);
    }

    private async Task ActualizarIdentidadActivaAsync(bool mostrarMensajeExito)
    {
        if (string.IsNullOrWhiteSpace(TokenActual))
        {
            lblIdentidadActiva.Text = "Usuario activo: (sin token JWT) | Rol: — | HTTP 401";
            lblIdentidadActiva.ForeColor = Color.FromArgb(248, 113, 113);
            MostrarEstado("HTTP 401 Unauthorized: No hay un token JWT cargado en memoria.", esError: true);
            return;
        }

        ApiResultado<IdentidadResponse> res = await _api.ObtenerIdentidadAsync(BaseUrl, TokenActual);
        if (!res.Exito || res.Datos is null)
        {
            lblIdentidadActiva.Text = $"Usuario activo: (token inválido) | Rol: — | HTTP {res.CodigoHttp}";
            lblIdentidadActiva.ForeColor = Color.FromArgb(248, 113, 113);
            MostrarEstado(res.Mensaje, esError: true);
            return;
        }

        string roles = res.Datos.Roles.Count > 0 ? string.Join(", ", res.Datos.Roles) : "SIN_ROL";
        lblIdentidadActiva.Text = $"Usuario activo: {res.Datos.Usuario} | Rol: {roles} | HTTP {res.CodigoHttp}";
        lblIdentidadActiva.ForeColor = Color.FromArgb(74, 222, 128);

        if (mostrarMensajeExito)
        {
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Identidad verificada -> Usuario '{res.Datos.Usuario}' con rol [{roles}].");
        }
    }

    // =========================================================================
    // TAB 1: VEHÍCULOS (GET, POST, PUT, DELETE /api/vehiculos)
    // =========================================================================

    private void btnGenerarPlacaChasis_Click(object? sender, EventArgs e)
    {
        GenerarPlacaChasisAleatorios();
        MostrarEstado("Placa y Chasis válidos de 17 caracteres generados para prueba rápida.");
    }

    private void GenerarPlacaChasisAleatorios()
    {
        const string letras = "ABCDEFGHJKLMNPRSTUVWXYZ";
        const string alfanum = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";

        string prefijo = new(Enumerable.Range(0, 3).Select(_ => letras[_random.Next(letras.Length)]).ToArray());
        int numero = _random.Next(1000, 9999);
        txtPlaca.Text = $"{prefijo}-{numero}";

        string chasis = new(Enumerable.Range(0, 17).Select(_ => alfanum[_random.Next(alfanum.Length)]).ToArray());
        txtChasis.Text = chasis;
    }

    private async void btnConsultar_Click(object? sender, EventArgs e)
    {
        await ListarVehiculosAsync();
    }

    private async Task ListarVehiculosAsync()
    {
        if (!ValidarTokenCargado()) return;

        btnConsultar.Enabled = false;
        MostrarEstado("Consultando GET /api/vehiculos a través de NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            string? marca = txtFiltroMarca.Text.Trim();
            string? estado = cmbFiltroEstado.SelectedItem?.ToString();

            ApiResultado<List<Vehiculo>> res = await _api.ListarVehiculosAsync(BaseUrl, TokenActual, marca, estado);
            if (!res.Exito)
            {
                dgvVehiculos.DataSource = null;
                lblTotal.Text = "Total de vehículos: 0";
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            List<Vehiculo> lista = res.Datos ?? new List<Vehiculo>();
            dgvVehiculos.DataSource = null;
            dgvVehiculos.DataSource = lista;
            lblTotal.Text = $"Total de vehículos: {lista.Count}";
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Se listaron {lista.Count} vehículo(s) desde NGINX.");
        }
        finally
        {
            btnConsultar.Enabled = true;
        }
    }

    private async void btnCrearVehiculo_Click(object? sender, EventArgs e)
    {
        if (!ValidarTokenCargado()) return;
        if (!ConstruirVehiculoRequest(out VehiculoRequest payload)) return;

        btnCrearVehiculo.Enabled = false;
        MostrarEstado("Enviando POST /api/vehiculos a través de NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            ApiResultado<Vehiculo> res = await _api.CrearVehiculoAsync(BaseUrl, TokenActual, payload);
            if (!res.Exito || res.Datos is null)
            {
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            txtVehiculoId.Text = res.Datos.IdVehiculo.ToString();
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Vehículo creado con ID {res.Datos.IdVehiculo} y placa {res.Datos.Placa}.");
            await ListarVehiculosAsync();
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Vehículo creado con ID {res.Datos.IdVehiculo} y placa {res.Datos.Placa}.");
        }
        finally
        {
            btnCrearVehiculo.Enabled = true;
        }
    }

    private async void btnActualizarVehiculo_Click(object? sender, EventArgs e)
    {
        if (!ValidarTokenCargado()) return;
        if (!int.TryParse(txtVehiculoId.Text.Trim(), out int idVehiculo) || idVehiculo <= 0)
        {
            MostrarEstado("Seleccione primero un vehículo de la tabla para actualizar (PUT /api/vehiculos/{id}).", esError: true);
            return;
        }
        if (!ConstruirVehiculoRequest(out VehiculoRequest payload)) return;

        btnActualizarVehiculo.Enabled = false;
        MostrarEstado($"Enviando PUT /api/vehiculos/{idVehiculo} a través de NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            ApiResultado<Vehiculo> res = await _api.ActualizarVehiculoAsync(BaseUrl, TokenActual, idVehiculo, payload);
            if (!res.Exito || res.Datos is null)
            {
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            await ListarVehiculosAsync();
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Vehículo ID {idVehiculo} actualizado correctamente.");
        }
        finally
        {
            btnActualizarVehiculo.Enabled = true;
        }
    }

    private async void btnEliminarVehiculo_Click(object? sender, EventArgs e)
    {
        if (!ValidarTokenCargado()) return;
        if (!int.TryParse(txtVehiculoId.Text.Trim(), out int idVehiculo) || idVehiculo <= 0)
        {
            MostrarEstado("Seleccione primero un vehículo de la tabla para eliminar (DELETE /api/vehiculos/{id}).", esError: true);
            return;
        }

        btnEliminarVehiculo.Enabled = false;
        MostrarEstado($"Enviando DELETE /api/vehiculos/{idVehiculo} a través de NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            ApiResultado<bool> res = await _api.EliminarVehiculoAsync(BaseUrl, TokenActual, idVehiculo);
            if (!res.Exito)
            {
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            LimpiarFormularioVehiculo();
            await ListarVehiculosAsync();
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Vehículo ID {idVehiculo} eliminado correctamente.");
        }
        finally
        {
            btnEliminarVehiculo.Enabled = true;
        }
    }

    private bool ConstruirVehiculoRequest(out VehiculoRequest request)
    {
        // Normalización previa a la validación (.Trim() y .ToUpperInvariant() en placa y chasis)
        string marca = txtMarca.Text.Trim();
        string modelo = txtModelo.Text.Trim();
        string placa = txtPlaca.Text.Trim().ToUpperInvariant();
        string chasis = txtChasis.Text.Trim().ToUpperInvariant();
        string color = txtColor.Text.Trim();
        int anio = (int)numAnio.Value;
        decimal precio = numPrecio.Value;

        txtMarca.Text = marca;
        txtModelo.Text = modelo;
        txtPlaca.Text = placa;
        txtChasis.Text = chasis;
        txtColor.Text = color;

        request = new VehiculoRequest
        {
            Marca = marca,
            Modelo = modelo,
            Placa = placa,
            Chasis = chasis,
            Anio = anio,
            Color = color,
            Precio = precio
        };

        if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo) ||
            string.IsNullOrWhiteSpace(placa) || string.IsNullOrWhiteSpace(chasis))
        {
            MostrarEstado("HTTP 422 Unprocessable Entity: Marca, Modelo, Placa y Chasis (17 caracteres) son obligatorios.", esError: true);
            return false;
        }

        return true;
    }

    private void dgvVehiculos_SelectionChanged(object? sender, EventArgs e)
    {
        if (dgvVehiculos.CurrentRow?.DataBoundItem is not Vehiculo v)
        {
            return;
        }

        txtVehiculoId.Text = v.IdVehiculo.ToString();
        txtMarca.Text = v.Marca;
        txtModelo.Text = v.Modelo;
        txtPlaca.Text = v.Placa;
        txtChasis.Text = v.Chasis;
        if (v.Anio >= numAnio.Minimum && v.Anio <= numAnio.Maximum)
        {
            numAnio.Value = v.Anio;
        }
        txtColor.Text = v.Color;
        if (v.Precio >= numPrecio.Minimum && v.Precio <= numPrecio.Maximum)
        {
            numPrecio.Value = v.Precio;
        }
        int idx = cmbEstadoVehiculo.Items.IndexOf(v.Estado.ToUpperInvariant());
        if (idx >= 0)
        {
            cmbEstadoVehiculo.SelectedIndex = idx;
        }
    }

    private void LimpiarFormularioVehiculo()
    {
        txtVehiculoId.Text = "";
        txtMarca.Text = "Toyota";
        txtModelo.Text = "Hilux 4x4";
        GenerarPlacaChasisAleatorios();
        numAnio.Value = 2025;
        txtColor.Text = "Plata Metálico";
        numPrecio.Value = 38500.00M;
        cmbEstadoVehiculo.SelectedIndex = 0;
    }

    // =========================================================================
    // TAB 2: COMPRADORES (POST /api/compradores y GET /api/compradores)
    // =========================================================================

    private void btnGenerarCompradorDemo_Click(object? sender, EventArgs e)
    {
        int sufijo = _random.Next(100000, 999999);
        txtCompDocumento.Text = $"180{sufijo}9";
        txtCompNombre.Text = $"Comprador UTA {sufijo}";
        txtCompCorreo.Text = $"comprador.{sufijo}@uta.edu.ec";
        MostrarEstado("Datos de comprador generados para registro rápido.");
    }

    private async void btnCrearComprador_Click(object? sender, EventArgs e)
    {
        if (!ValidarTokenCargado()) return;

        string doc = txtCompDocumento.Text.Trim();
        string nom = txtCompNombre.Text.Trim();
        string cor = txtCompCorreo.Text.Trim();

        txtCompDocumento.Text = doc;
        txtCompNombre.Text = nom;
        txtCompCorreo.Text = cor;

        if (string.IsNullOrWhiteSpace(doc) || string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(cor))
        {
            MostrarEstado("HTTP 422 Unprocessable Entity: Documento, Nombre y Correo son obligatorios.", esError: true);
            return;
        }

        btnCrearComprador.Enabled = false;
        MostrarEstado("Enviando POST /api/compradores a FastAPI vía NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            var payload = new CrearCompradorRequest
            {
                Documento = doc,
                Nombre = nom,
                Correo = cor
            };

            ApiResultado<Comprador> res = await _api.CrearCompradorAsync(BaseUrl, TokenActual, payload);
            if (!res.Exito || res.Datos is null)
            {
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            await ListarCompradoresInternoAsync();
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Comprador creado con ID {res.Datos.Id} ({res.Datos.Nombre} - {res.Datos.Documento}).");
        }
        finally
        {
            btnCrearComprador.Enabled = true;
        }
    }

    private async void btnListarCompradores_Click(object? sender, EventArgs e)
    {
        await ListarCompradoresInternoAsync();
    }

    private async Task ListarCompradoresInternoAsync()
    {
        if (!ValidarTokenCargado()) return;

        btnListarCompradores.Enabled = false;
        MostrarEstado("Consultando GET /api/compradores a través de NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            ApiResultado<List<Comprador>> res = await _api.ListarCompradoresAsync(BaseUrl, TokenActual);
            if (!res.Exito)
            {
                dgvCompradores.DataSource = null;
                lblTotal.Text = "Total de compradores: 0";
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            List<Comprador> lista = res.Datos ?? new List<Comprador>();
            dgvCompradores.DataSource = null;
            dgvCompradores.DataSource = lista;
            lblTotal.Text = $"Total de compradores: {lista.Count}";
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Se listaron {lista.Count} comprador(es) registrados.");
        }
        finally
        {
            btnListarCompradores.Enabled = true;
        }
    }

    // =========================================================================
    // TAB 3: VENTAS (SOLO CONSULTA: GET /api/ventas y GET /api/ventas/{id})
    // =========================================================================

    private async void btnListarVentas_Click(object? sender, EventArgs e)
    {
        if (!ValidarTokenCargado()) return;

        btnListarVentas.Enabled = false;
        MostrarEstado("Consultando GET /api/ventas a través de NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            ApiResultado<List<Venta>> res = await _api.ListarVentasAsync(BaseUrl, TokenActual);
            if (!res.Exito)
            {
                dgvVentas.DataSource = null;
                lblTotal.Text = "Total de ventas: 0";
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            List<Venta> lista = res.Datos ?? new List<Venta>();
            dgvVentas.DataSource = null;
            dgvVentas.DataSource = lista;
            lblTotal.Text = $"Total de ventas: {lista.Count}";
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Se consultaron {lista.Count} venta(s) administrativas.");
        }
        finally
        {
            btnListarVentas.Enabled = true;
        }
    }

    private async void btnBuscarVentaId_Click(object? sender, EventArgs e)
    {
        if (!ValidarTokenCargado()) return;
        if (!int.TryParse(txtBuscarVentaId.Text.Trim(), out int ventaId) || ventaId <= 0)
        {
            MostrarEstado("HTTP 400 Bad Request: Ingrese un ID de venta entero positivo.", esError: true);
            return;
        }

        btnBuscarVentaId.Enabled = false;
        MostrarEstado($"Consultando GET /api/ventas/{ventaId} a través de NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            ApiResultado<Venta> res = await _api.ObtenerVentaPorIdAsync(BaseUrl, TokenActual, ventaId);
            if (!res.Exito || res.Datos is null)
            {
                dgvVentas.DataSource = null;
                lblTotal.Text = "Total de ventas: 0";
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            var lista = new List<Venta> { res.Datos };
            dgvVentas.DataSource = null;
            dgvVentas.DataSource = lista;
            lblTotal.Text = "Total de ventas: 1";
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Venta #{res.Datos.Id} encontrada (Estado: {res.Datos.Estado}, Notificación: {res.Datos.Notificacion}).");
        }
        finally
        {
            btnBuscarVentaId.Enabled = true;
        }
    }

    // =========================================================================
    // TAB 4: USUARIOS API (POST /api/usuarios y GET /api/usuarios)
    // =========================================================================

    private async void btnCrearUsuario_Click(object? sender, EventArgs e)
    {
        if (!ValidarTokenCargado()) return;

        string username = txtNuevoUsername.Text.Trim();
        string password = txtNuevoPassword.Text.Trim();
        string rol = cmbNuevoRol.SelectedItem?.ToString() ?? "OPERADOR";

        txtNuevoUsername.Text = username;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            MostrarEstado("HTTP 422 Unprocessable Entity: Username y Password son obligatorios.", esError: true);
            return;
        }

        btnCrearUsuario.Enabled = false;
        MostrarEstado("Enviando POST /api/usuarios a través de NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            var payload = new CrearUsuarioRequest
            {
                Username = username,
                Password = password,
                Rol = rol
            };

            ApiResultado<UsuarioApiDto> res = await _api.CrearUsuarioAsync(BaseUrl, TokenActual, payload);
            if (!res.Exito || res.Datos is null)
            {
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            await ListarUsuariosInternoAsync();
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Usuario '{res.Datos.Username}' creado con ID {res.Datos.Id} y rol {res.Datos.Rol}.");
        }
        finally
        {
            btnCrearUsuario.Enabled = true;
        }
    }

    private async void btnListarUsuarios_Click(object? sender, EventArgs e)
    {
        await ListarUsuariosInternoAsync();
    }

    private async Task ListarUsuariosInternoAsync()
    {
        if (!ValidarTokenCargado()) return;

        btnListarUsuarios.Enabled = false;
        MostrarEstado("Consultando GET /api/usuarios a través de NGINX...");

        try
        {
            await ActualizarIdentidadActivaAsync(mostrarMensajeExito: false);
            ApiResultado<List<UsuarioApiDto>> res = await _api.ListarUsuariosAsync(BaseUrl, TokenActual);
            if (!res.Exito)
            {
                dgvUsuarios.DataSource = null;
                lblTotal.Text = "Total de usuarios: 0";
                MostrarEstado(res.Mensaje, esError: true);
                return;
            }

            List<UsuarioApiDto> lista = res.Datos ?? new List<UsuarioApiDto>();
            dgvUsuarios.DataSource = null;
            dgvUsuarios.DataSource = lista;
            lblTotal.Text = $"Total de usuarios: {lista.Count}";
            MostrarEstado($"HTTP {res.EstadoHttpTexto}: Se listaron {lista.Count} usuario(s) registrados en la API.");
        }
        finally
        {
            btnListarUsuarios.Enabled = true;
        }
    }

    private bool ValidarTokenCargado()
    {
        if (string.IsNullOrWhiteSpace(TokenActual))
        {
            MostrarEstado("HTTP 401 Unauthorized: Inicie sesión primero o pegue un token JWT válido.", esError: true);
            txtToken.Focus();
            return false;
        }
        return true;
    }

    private void MostrarEstado(string mensaje, bool esError = false)
    {
        lblEstado.ForeColor = esError ? Color.Firebrick : Color.FromArgb(22, 101, 52);
        lblEstado.Text = mensaje;
    }

    private void lblTitulo_Click(object? sender, EventArgs e)
    {
    }
}
