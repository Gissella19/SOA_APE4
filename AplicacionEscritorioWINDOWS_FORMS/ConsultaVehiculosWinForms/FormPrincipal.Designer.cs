namespace ConsultaVehiculosWinForms;

partial class FormPrincipal
{
    private System.ComponentModel.IContainer components = null!;

    // Cabecera y sesión
    private Panel pnlHeader = null!;
    private Label lblTitulo = null!;
    private Label lblSubtitulo = null!;
    private Label lblIdentidadActiva = null!;

    private Panel pnlSesion = null!;
    private Label lblBaseUrl = null!;
    private TextBox txtBaseUrl = null!;
    private Label lblLoginUser = null!;
    private TextBox txtLoginUser = null!;
    private Label lblLoginPass = null!;
    private TextBox txtLoginPass = null!;
    private Button btnLogin = null!;
    private Button btnPresetAdmin = null!;
    private Button btnPresetOperador = null!;
    private Button btnPresetConsulta = null!;
    private Label lblToken = null!;
    private TextBox txtToken = null!;
    private Button btnToggleToken = null!;
    private Button btnVerificarIdentidad = null!;

    // Pestañas principales
    private TabControl tabPrincipal = null!;
    private TabPage tabVehiculos = null!;
    private TabPage tabCompradores = null!;
    private TabPage tabVentas = null!;
    private TabPage tabUsuarios = null!;

    // Tab 1: Vehículos
    private GroupBox grpVehiculoForm = null!;
    private TextBox txtVehiculoId = null!;
    private TextBox txtMarca = null!;
    private TextBox txtModelo = null!;
    private TextBox txtPlaca = null!;
    private TextBox txtChasis = null!;
    private NumericUpDown numAnio = null!;
    private TextBox txtColor = null!;
    private NumericUpDown numPrecio = null!;
    private ComboBox cmbEstadoVehiculo = null!;
    private Button btnGenerarPlacaChasis = null!;
    private Button btnCrearVehiculo = null!;
    private Button btnActualizarVehiculo = null!;
    private Button btnEliminarVehiculo = null!;
    private Button btnLimpiarVehiculo = null!;

    private Panel pnlVehiculosFiltro = null!;
    private TextBox txtFiltroMarca = null!;
    private ComboBox cmbFiltroEstado = null!;
    private Button btnConsultar = null!;
    private DataGridView dgvVehiculos = null!;

    // Tab 2: Compradores
    private GroupBox grpCompradorForm = null!;
    private TextBox txtCompDocumento = null!;
    private TextBox txtCompNombre = null!;
    private TextBox txtCompCorreo = null!;
    private Button btnGenerarCompradorDemo = null!;
    private Button btnCrearComprador = null!;
    private Button btnListarCompradores = null!;
    private DataGridView dgvCompradores = null!;

    // Tab 3: Ventas (Solo Consulta Administrativa)
    private Panel pnlVentasBanner = null!;
    private Label lblVentasBanner = null!;
    private Panel pnlVentasAcciones = null!;
    private TextBox txtBuscarVentaId = null!;
    private Button btnBuscarVentaId = null!;
    private Button btnListarVentas = null!;
    private DataGridView dgvVentas = null!;

    // Tab 4: Usuarios API (Defensa en Vivo)
    private GroupBox grpUsuarioForm = null!;
    private TextBox txtNuevoUsername = null!;
    private TextBox txtNuevoPassword = null!;
    private ComboBox cmbNuevoRol = null!;
    private Button btnCrearUsuario = null!;
    private Button btnListarUsuarios = null!;
    private DataGridView dgvUsuarios = null!;

    // Barra inferior de estado
    private Panel pnlFooter = null!;
    private Label lblEstado = null!;
    private Label lblTotal = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        pnlHeader = new Panel();
        lblTitulo = new Label();
        lblSubtitulo = new Label();
        lblIdentidadActiva = new Label();

        pnlSesion = new Panel();
        lblBaseUrl = new Label();
        txtBaseUrl = new TextBox();
        lblLoginUser = new Label();
        txtLoginUser = new TextBox();
        lblLoginPass = new Label();
        txtLoginPass = new TextBox();
        btnLogin = new Button();
        btnPresetAdmin = new Button();
        btnPresetOperador = new Button();
        btnPresetConsulta = new Button();
        lblToken = new Label();
        txtToken = new TextBox();
        btnToggleToken = new Button();
        btnVerificarIdentidad = new Button();

        tabPrincipal = new TabControl();
        tabVehiculos = new TabPage();
        tabCompradores = new TabPage();
        tabVentas = new TabPage();
        tabUsuarios = new TabPage();

        grpVehiculoForm = new GroupBox();
        txtVehiculoId = new TextBox();
        txtMarca = new TextBox();
        txtModelo = new TextBox();
        txtPlaca = new TextBox();
        txtChasis = new TextBox();
        numAnio = new NumericUpDown();
        txtColor = new TextBox();
        numPrecio = new NumericUpDown();
        cmbEstadoVehiculo = new ComboBox();
        btnGenerarPlacaChasis = new Button();
        btnCrearVehiculo = new Button();
        btnActualizarVehiculo = new Button();
        btnEliminarVehiculo = new Button();
        btnLimpiarVehiculo = new Button();

        pnlVehiculosFiltro = new Panel();
        txtFiltroMarca = new TextBox();
        cmbFiltroEstado = new ComboBox();
        btnConsultar = new Button();
        dgvVehiculos = new DataGridView();

        grpCompradorForm = new GroupBox();
        txtCompDocumento = new TextBox();
        txtCompNombre = new TextBox();
        txtCompCorreo = new TextBox();
        btnGenerarCompradorDemo = new Button();
        btnCrearComprador = new Button();
        btnListarCompradores = new Button();
        dgvCompradores = new DataGridView();

        pnlVentasBanner = new Panel();
        lblVentasBanner = new Label();
        pnlVentasAcciones = new Panel();
        txtBuscarVentaId = new TextBox();
        btnBuscarVentaId = new Button();
        btnListarVentas = new Button();
        dgvVentas = new DataGridView();

        grpUsuarioForm = new GroupBox();
        txtNuevoUsername = new TextBox();
        txtNuevoPassword = new TextBox();
        cmbNuevoRol = new ComboBox();
        btnCrearUsuario = new Button();
        btnListarUsuarios = new Button();
        dgvUsuarios = new DataGridView();

        pnlFooter = new Panel();
        lblEstado = new Label();
        lblTotal = new Label();

        SuspendLayout();

        // 
        // pnlHeader
        // 
        pnlHeader.Dock = DockStyle.Top;
        pnlHeader.Height = 76;
        pnlHeader.BackColor = Color.FromArgb(15, 23, 42);
        pnlHeader.Padding = new Padding(18, 10, 18, 10);

        lblTitulo.AutoSize = true;
        lblTitulo.Font = new Font("Segoe UI", 15F, FontStyle.Bold);
        lblTitulo.ForeColor = Color.White;
        lblTitulo.Location = new Point(18, 10);
        lblTitulo.Text = "Sistema de Gestión Administrativa — Windows Forms (.NET 8)";
        lblTitulo.Click += lblTitulo_Click;

        lblSubtitulo.AutoSize = true;
        lblSubtitulo.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        lblSubtitulo.ForeColor = Color.FromArgb(148, 163, 184);
        lblSubtitulo.Location = new Point(20, 44);
        lblSubtitulo.Text = "Gateway NGINX (8090) · Gestión de Vehículos, Creación Exclusiva de Compradores, Consulta de Ventas y Usuarios API";

        lblIdentidadActiva.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        lblIdentidadActiva.AutoSize = false;
        lblIdentidadActiva.TextAlign = ContentAlignment.MiddleCenter;
        lblIdentidadActiva.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblIdentidadActiva.BackColor = Color.FromArgb(30, 41, 59);
        lblIdentidadActiva.ForeColor = Color.FromArgb(56, 189, 248);
        lblIdentidadActiva.Location = new Point(790, 16);
        lblIdentidadActiva.Size = new Size(470, 44);
        lblIdentidadActiva.Text = "Usuario activo: (sin autenticar) | Rol: — | HTTP —";

        pnlHeader.Controls.Add(lblIdentidadActiva);
        pnlHeader.Controls.Add(lblTitulo);
        pnlHeader.Controls.Add(lblSubtitulo);

        // 
        // pnlSesion
        // 
        pnlSesion.Dock = DockStyle.Top;
        pnlSesion.Height = 104;
        pnlSesion.BackColor = Color.FromArgb(241, 245, 249);
        pnlSesion.Padding = new Padding(16, 8, 16, 8);

        lblBaseUrl.AutoSize = true;
        lblBaseUrl.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblBaseUrl.Location = new Point(16, 14);
        lblBaseUrl.Text = "Gateway NGINX:";

        txtBaseUrl.Location = new Point(125, 10);
        txtBaseUrl.Size = new Size(185, 27);
        txtBaseUrl.Text = "http://127.0.0.1:8090/";

        lblLoginUser.AutoSize = true;
        lblLoginUser.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblLoginUser.Location = new Point(320, 14);
        lblLoginUser.Text = "Usuario:";

        txtLoginUser.Location = new Point(380, 10);
        txtLoginUser.Size = new Size(145, 27);
        txtLoginUser.Text = "usuario_operador";

        lblLoginPass.AutoSize = true;
        lblLoginPass.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblLoginPass.Location = new Point(535, 14);
        lblLoginPass.Text = "Clave:";

        txtLoginPass.Location = new Point(582, 10);
        txtLoginPass.Size = new Size(165, 27);
        txtLoginPass.UseSystemPasswordChar = true;
        txtLoginPass.Text = "cambiar-contrasena-aula-2026";

        btnLogin.BackColor = Color.FromArgb(14, 116, 144);
        btnLogin.ForeColor = Color.White;
        btnLogin.FlatStyle = FlatStyle.Flat;
        btnLogin.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnLogin.Location = new Point(756, 8);
        btnLogin.Size = new Size(190, 31);
        btnLogin.Text = "Iniciar Sesión (/oauth/token)";
        btnLogin.Click += btnLogin_Click;

        btnPresetAdmin.Location = new Point(956, 8);
        btnPresetAdmin.Size = new Size(98, 31);
        btnPresetAdmin.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnPresetAdmin.Text = "Admin";
        btnPresetAdmin.Click += (_, _) => CargarPresetUsuario("usuario_admin", "cambiar-contrasena-aula-2026");

        btnPresetOperador.Location = new Point(1060, 8);
        btnPresetOperador.Size = new Size(98, 31);
        btnPresetOperador.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnPresetOperador.Text = "Operador";
        btnPresetOperador.Click += (_, _) => CargarPresetUsuario("usuario_operador", "cambiar-contrasena-aula-2026");

        btnPresetConsulta.Location = new Point(1164, 8);
        btnPresetConsulta.Size = new Size(98, 31);
        btnPresetConsulta.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnPresetConsulta.Text = "Consulta";
        btnPresetConsulta.Click += (_, _) => CargarPresetUsuario("usuario_consulta", "cambiar-contrasena-aula-2026");

        lblToken.AutoSize = true;
        lblToken.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblToken.Location = new Point(16, 56);
        lblToken.Text = "Token JWT (En memoria / Editable):";

        txtToken.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        txtToken.Location = new Point(242, 52);
        txtToken.Size = new Size(760, 27);
        txtToken.UseSystemPasswordChar = true;
        txtToken.PlaceholderText = "El token obtenido de POST /oauth/token se almacena aquí y se envía en Authorization: Bearer <JWT>";

        btnToggleToken.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnToggleToken.Location = new Point(1010, 50);
        btnToggleToken.Size = new Size(105, 31);
        btnToggleToken.Font = new Font("Segoe UI", 8.5F);
        btnToggleToken.Text = "Mostrar JWT";
        btnToggleToken.Click += btnToggleToken_Click;

        btnVerificarIdentidad.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        btnVerificarIdentidad.BackColor = Color.FromArgb(30, 64, 175);
        btnVerificarIdentidad.ForeColor = Color.White;
        btnVerificarIdentidad.FlatStyle = FlatStyle.Flat;
        btnVerificarIdentidad.Location = new Point(1122, 50);
        btnVerificarIdentidad.Size = new Size(140, 31);
        btnVerificarIdentidad.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnVerificarIdentidad.Text = "GET /api/identidad";
        btnVerificarIdentidad.Click += btnVerificarIdentidad_Click;

        pnlSesion.Controls.AddRange(new Control[]
        {
            lblBaseUrl, txtBaseUrl, lblLoginUser, txtLoginUser, lblLoginPass, txtLoginPass,
            btnLogin, btnPresetAdmin, btnPresetOperador, btnPresetConsulta,
            lblToken, txtToken, btnToggleToken, btnVerificarIdentidad
        });

        // 
        // pnlFooter
        // 
        pnlFooter.Dock = DockStyle.Bottom;
        pnlFooter.Height = 44;
        pnlFooter.BackColor = Color.FromArgb(248, 250, 252);
        pnlFooter.Padding = new Padding(16, 6, 16, 6);

        lblEstado.AutoSize = false;
        lblEstado.Dock = DockStyle.Fill;
        lblEstado.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblEstado.ForeColor = Color.FromArgb(30, 41, 59);
        lblEstado.TextAlign = ContentAlignment.MiddleLeft;
        lblEstado.Text = "Listo. Inicie sesión o pegue un token JWT para operar a través de NGINX (http://127.0.0.1:8090/).";

        lblTotal.AutoSize = false;
        lblTotal.Dock = DockStyle.Right;
        lblTotal.Width = 260;
        lblTotal.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblTotal.ForeColor = Color.FromArgb(15, 23, 42);
        lblTotal.TextAlign = ContentAlignment.MiddleRight;
        lblTotal.Text = "Total de registros: 0";

        pnlFooter.Controls.Add(lblEstado);
        pnlFooter.Controls.Add(lblTotal);

        // 
        // tabPrincipal
        // 
        tabPrincipal.Dock = DockStyle.Fill;
        tabPrincipal.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        tabPrincipal.Controls.Add(tabVehiculos);
        tabPrincipal.Controls.Add(tabCompradores);
        tabPrincipal.Controls.Add(tabVentas);
        tabPrincipal.Controls.Add(tabUsuarios);

        ConfigurarTabVehiculos();
        ConfigurarTabCompradores();
        ConfigurarTabVentas();
        ConfigurarTabUsuarios();

        // 
        // FormPrincipal
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(1280, 760);
        MinimumSize = new Size(1120, 680);
        Controls.Add(tabPrincipal);
        Controls.Add(pnlSesion);
        Controls.Add(pnlHeader);
        Controls.Add(pnlFooter);
        Font = new Font("Segoe UI", 9F);
        Name = "FormPrincipal";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "APE 04 — Cliente Escritorio Windows Forms (.NET 8) — Gestión de Vehículos, Compradores, Ventas y Usuarios";

        ResumeLayout(false);
        PerformLayout();
    }

    private void ConfigurarTabVehiculos()
    {
        tabVehiculos.Text = "1. Vehículos (Crear, Editar, Eliminar y Listar)";
        tabVehiculos.BackColor = Color.White;
        tabVehiculos.Padding = new Padding(12);

        grpVehiculoForm.Dock = DockStyle.Top;
        grpVehiculoForm.Height = 148;
        grpVehiculoForm.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpVehiculoForm.Text = "Formulario de Vehículo (POST / PUT / DELETE /api/vehiculos)";

        var lblId = CrearEtiqueta("ID:", 16, 28);
        txtVehiculoId.Location = new Point(45, 24);
        txtVehiculoId.Size = new Size(60, 27);
        txtVehiculoId.ReadOnly = true;
        txtVehiculoId.BackColor = Color.FromArgb(241, 245, 249);

        var lblMarca = CrearEtiqueta("Marca:", 118, 28);
        txtMarca.Location = new Point(170, 24);
        txtMarca.Size = new Size(135, 27);
        txtMarca.Text = "Toyota";

        var lblModelo = CrearEtiqueta("Modelo:", 318, 28);
        txtModelo.Location = new Point(378, 24);
        txtModelo.Size = new Size(145, 27);
        txtModelo.Text = "Corolla Cross";

        var lblPlaca = CrearEtiqueta("Placa:", 535, 28);
        txtPlaca.Location = new Point(580, 24);
        txtPlaca.Size = new Size(115, 27);
        txtPlaca.Text = "ABC-1234";

        var lblChasis = CrearEtiqueta("Chasis (17):", 705, 28);
        txtChasis.Location = new Point(790, 24);
        txtChasis.Size = new Size(185, 27);
        txtChasis.MaxLength = 17;
        txtChasis.Text = "1HGCM82633A123456";

        btnGenerarPlacaChasis.Location = new Point(985, 22);
        btnGenerarPlacaChasis.Size = new Size(245, 31);
        btnGenerarPlacaChasis.BackColor = Color.FromArgb(241, 245, 249);
        btnGenerarPlacaChasis.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        btnGenerarPlacaChasis.Text = "Generar Placa/Chasis Válidos";
        btnGenerarPlacaChasis.Click += btnGenerarPlacaChasis_Click;

        var lblAnio = CrearEtiqueta("Año:", 16, 68);
        numAnio.Location = new Point(55, 64);
        numAnio.Size = new Size(85, 27);
        numAnio.Minimum = 1900;
        numAnio.Maximum = 2100;
        numAnio.Value = 2025;

        var lblColor = CrearEtiqueta("Color:", 152, 68);
        txtColor.Location = new Point(200, 64);
        txtColor.Size = new Size(125, 27);
        txtColor.Text = "Blanco Perla";

        var lblPrecio = CrearEtiqueta("Precio USD:", 338, 68);
        numPrecio.Location = new Point(422, 64);
        numPrecio.Size = new Size(135, 27);
        numPrecio.DecimalPlaces = 2;
        numPrecio.Minimum = 1;
        numPrecio.Maximum = 999999999;
        numPrecio.Value = 26990.00M;

        var lblEst = CrearEtiqueta("Estado:", 570, 68);
        cmbEstadoVehiculo.Location = new Point(628, 64);
        cmbEstadoVehiculo.Size = new Size(140, 27);
        cmbEstadoVehiculo.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbEstadoVehiculo.Items.AddRange(new object[] { "DISPONIBLE", "RESERVADO", "VENDIDO" });
        cmbEstadoVehiculo.SelectedIndex = 0;

        btnCrearVehiculo.Location = new Point(16, 104);
        btnCrearVehiculo.Size = new Size(245, 34);
        btnCrearVehiculo.BackColor = Color.FromArgb(22, 101, 52);
        btnCrearVehiculo.ForeColor = Color.White;
        btnCrearVehiculo.FlatStyle = FlatStyle.Flat;
        btnCrearVehiculo.Text = "Crear Vehículo (POST /api/vehiculos)";
        btnCrearVehiculo.Click += btnCrearVehiculo_Click;

        btnActualizarVehiculo.Location = new Point(272, 104);
        btnActualizarVehiculo.Size = new Size(275, 34);
        btnActualizarVehiculo.BackColor = Color.FromArgb(3, 105, 161);
        btnActualizarVehiculo.ForeColor = Color.White;
        btnActualizarVehiculo.FlatStyle = FlatStyle.Flat;
        btnActualizarVehiculo.Text = "Actualizar Vehículo (PUT /api/vehiculos/{id})";
        btnActualizarVehiculo.Click += btnActualizarVehiculo_Click;

        btnEliminarVehiculo.Location = new Point(558, 104);
        btnEliminarVehiculo.Size = new Size(280, 34);
        btnEliminarVehiculo.BackColor = Color.FromArgb(185, 28, 28);
        btnEliminarVehiculo.ForeColor = Color.White;
        btnEliminarVehiculo.FlatStyle = FlatStyle.Flat;
        btnEliminarVehiculo.Text = "Eliminar Vehículo (DELETE /api/vehiculos/{id})";
        btnEliminarVehiculo.Click += btnEliminarVehiculo_Click;

        btnLimpiarVehiculo.Location = new Point(850, 104);
        btnLimpiarVehiculo.Size = new Size(165, 34);
        btnLimpiarVehiculo.Text = "Limpiar Selección";
        btnLimpiarVehiculo.Click += (_, _) => LimpiarFormularioVehiculo();

        grpVehiculoForm.Controls.AddRange(new Control[]
        {
            lblId, txtVehiculoId, lblMarca, txtMarca, lblModelo, txtModelo,
            lblPlaca, txtPlaca, lblChasis, txtChasis, btnGenerarPlacaChasis,
            lblAnio, numAnio, lblColor, txtColor, lblPrecio, numPrecio,
            lblEst, cmbEstadoVehiculo,
            btnCrearVehiculo, btnActualizarVehiculo, btnEliminarVehiculo, btnLimpiarVehiculo
        });

        pnlVehiculosFiltro.Dock = DockStyle.Top;
        pnlVehiculosFiltro.Height = 48;
        pnlVehiculosFiltro.Padding = new Padding(4, 8, 4, 8);

        var lblFiltroMarca = CrearEtiqueta("Filtrar Marca:", 8, 14);
        txtFiltroMarca.Location = new Point(102, 10);
        txtFiltroMarca.Size = new Size(170, 27);
        txtFiltroMarca.PlaceholderText = "Ej: Toyota";

        var lblFiltroEstado = CrearEtiqueta("Filtrar Estado:", 288, 14);
        cmbFiltroEstado.Location = new Point(388, 10);
        cmbFiltroEstado.Size = new Size(155, 27);
        cmbFiltroEstado.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFiltroEstado.Items.AddRange(new object[] { "TODOS", "DISPONIBLE", "VENDIDO", "RESERVADO" });
        cmbFiltroEstado.SelectedIndex = 0;

        btnConsultar.Location = new Point(560, 8);
        btnConsultar.Size = new Size(280, 32);
        btnConsultar.BackColor = Color.FromArgb(15, 23, 42);
        btnConsultar.ForeColor = Color.White;
        btnConsultar.FlatStyle = FlatStyle.Flat;
        btnConsultar.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnConsultar.Text = "Listar Vehículos (GET /api/vehiculos)";
        btnConsultar.Click += btnConsultar_Click;

        pnlVehiculosFiltro.Controls.AddRange(new Control[]
        {
            lblFiltroMarca, txtFiltroMarca, lblFiltroEstado, cmbFiltroEstado, btnConsultar
        });

        ConfigurarGrilla(dgvVehiculos);
        dgvVehiculos.SelectionChanged += dgvVehiculos_SelectionChanged;

        tabVehiculos.Controls.Add(dgvVehiculos);
        tabVehiculos.Controls.Add(pnlVehiculosFiltro);
        tabVehiculos.Controls.Add(grpVehiculoForm);
    }

    private void ConfigurarTabCompradores()
    {
        tabCompradores.Text = "2. Compradores (Crear y Listar — Exclusivo WinForms)";
        tabCompradores.BackColor = Color.White;
        tabCompradores.Padding = new Padding(12);

        grpCompradorForm.Dock = DockStyle.Top;
        grpCompradorForm.Height = 112;
        grpCompradorForm.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpCompradorForm.Text = "Registro de Compradores en FastAPI a través de NGINX (Exclusivo de Windows Forms)";

        var lblDoc = CrearEtiqueta("Documento / Cédula:", 16, 30);
        txtCompDocumento.Location = new Point(158, 26);
        txtCompDocumento.Size = new Size(165, 27);
        txtCompDocumento.Text = "1804567890";

        var lblNom = CrearEtiqueta("Nombre Completo:", 338, 30);
        txtCompNombre.Location = new Point(468, 26);
        txtCompNombre.Size = new Size(245, 27);
        txtCompNombre.Text = "Carlos Andrés Salazar";

        var lblCor = CrearEtiqueta("Correo Electrónico:", 728, 30);
        txtCompCorreo.Location = new Point(860, 26);
        txtCompCorreo.Size = new Size(245, 27);
        txtCompCorreo.Text = "carlos.salazar@uta.edu.ec";

        btnGenerarCompradorDemo.Location = new Point(16, 66);
        btnGenerarCompradorDemo.Size = new Size(235, 34);
        btnGenerarCompradorDemo.Text = "Generar Datos Comprador Válido";
        btnGenerarCompradorDemo.Click += btnGenerarCompradorDemo_Click;

        btnCrearComprador.Location = new Point(265, 66);
        btnCrearComprador.Size = new Size(285, 34);
        btnCrearComprador.BackColor = Color.FromArgb(22, 101, 52);
        btnCrearComprador.ForeColor = Color.White;
        btnCrearComprador.FlatStyle = FlatStyle.Flat;
        btnCrearComprador.Text = "Crear Comprador (POST /api/compradores)";
        btnCrearComprador.Click += btnCrearComprador_Click;

        btnListarCompradores.Location = new Point(564, 66);
        btnListarCompradores.Size = new Size(285, 34);
        btnListarCompradores.BackColor = Color.FromArgb(15, 23, 42);
        btnListarCompradores.ForeColor = Color.White;
        btnListarCompradores.FlatStyle = FlatStyle.Flat;
        btnListarCompradores.Text = "Listar Compradores (GET /api/compradores)";
        btnListarCompradores.Click += btnListarCompradores_Click;

        grpCompradorForm.Controls.AddRange(new Control[]
        {
            lblDoc, txtCompDocumento, lblNom, txtCompNombre, lblCor, txtCompCorreo,
            btnGenerarCompradorDemo, btnCrearComprador, btnListarCompradores
        });

        ConfigurarGrilla(dgvCompradores);

        tabCompradores.Controls.Add(dgvCompradores);
        tabCompradores.Controls.Add(grpCompradorForm);
    }

    private void ConfigurarTabVentas()
    {
        tabVentas.Text = "3. Ventas (Solo Consulta Administrativa)";
        tabVentas.BackColor = Color.White;
        tabVentas.Padding = new Padding(12);

        pnlVentasBanner.Dock = DockStyle.Top;
        pnlVentasBanner.Height = 46;
        pnlVentasBanner.BackColor = Color.FromArgb(254, 243, 199);
        pnlVentasBanner.Padding = new Padding(12, 10, 12, 10);

        lblVentasBanner.AutoSize = true;
        lblVentasBanner.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        lblVentasBanner.ForeColor = Color.FromArgb(146, 64, 14);
        lblVentasBanner.Location = new Point(12, 12);
        lblVentasBanner.Text = "ROL ARQUITECTÓNICO: Consulta Administrativa de Ventas (El registro de ventas es exclusivo de la PWA Móvil)";

        pnlVentasBanner.Controls.Add(lblVentasBanner);

        pnlVentasAcciones.Dock = DockStyle.Top;
        pnlVentasAcciones.Height = 52;

        var lblVentaId = CrearEtiqueta("ID de Venta:", 12, 16);
        txtBuscarVentaId.Location = new Point(98, 12);
        txtBuscarVentaId.Size = new Size(110, 27);
        txtBuscarVentaId.PlaceholderText = "Ej: 1";

        btnBuscarVentaId.Location = new Point(220, 10);
        btnBuscarVentaId.Size = new Size(290, 34);
        btnBuscarVentaId.BackColor = Color.FromArgb(3, 105, 161);
        btnBuscarVentaId.ForeColor = Color.White;
        btnBuscarVentaId.FlatStyle = FlatStyle.Flat;
        btnBuscarVentaId.Text = "Buscar Venta por ID (GET /api/ventas/{id})";
        btnBuscarVentaId.Click += btnBuscarVentaId_Click;

        btnListarVentas.Location = new Point(522, 10);
        btnListarVentas.Size = new Size(275, 34);
        btnListarVentas.BackColor = Color.FromArgb(15, 23, 42);
        btnListarVentas.ForeColor = Color.White;
        btnListarVentas.FlatStyle = FlatStyle.Flat;
        btnListarVentas.Text = "Listar Ventas (GET /api/ventas)";
        btnListarVentas.Click += btnListarVentas_Click;

        pnlVentasAcciones.Controls.AddRange(new Control[]
        {
            lblVentaId, txtBuscarVentaId, btnBuscarVentaId, btnListarVentas
        });

        ConfigurarGrilla(dgvVentas);

        tabVentas.Controls.Add(dgvVentas);
        tabVentas.Controls.Add(pnlVentasAcciones);
        tabVentas.Controls.Add(pnlVentasBanner);
    }

    private void ConfigurarTabUsuarios()
    {
        tabUsuarios.Text = "4. Usuarios API (Gestión para Defensa en Vivo)";
        tabUsuarios.BackColor = Color.White;
        tabUsuarios.Padding = new Padding(12);

        grpUsuarioForm.Dock = DockStyle.Top;
        grpUsuarioForm.Height = 108;
        grpUsuarioForm.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        grpUsuarioForm.Text = "Creación y Consulta de Usuarios API (POST /api/usuarios y GET /api/usuarios — Solo ADMIN)";

        var lblUser = CrearEtiqueta("Username:", 16, 30);
        txtNuevoUsername.Location = new Point(95, 26);
        txtNuevoUsername.Size = new Size(185, 27);
        txtNuevoUsername.PlaceholderText = "nuevo_operador";

        var lblPass = CrearEtiqueta("Password:", 295, 30);
        txtNuevoPassword.Location = new Point(372, 26);
        txtNuevoPassword.Size = new Size(215, 27);
        txtNuevoPassword.Text = "cambiar-contrasena-aula-2026";

        var lblRol = CrearEtiqueta("Rol:", 605, 30);
        cmbNuevoRol.Location = new Point(642, 26);
        cmbNuevoRol.Size = new Size(155, 27);
        cmbNuevoRol.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbNuevoRol.Items.AddRange(new object[] { "CONSULTA", "OPERADOR", "ADMIN" });
        cmbNuevoRol.SelectedIndex = 1;

        btnCrearUsuario.Location = new Point(16, 64);
        btnCrearUsuario.Size = new Size(275, 34);
        btnCrearUsuario.BackColor = Color.FromArgb(22, 101, 52);
        btnCrearUsuario.ForeColor = Color.White;
        btnCrearUsuario.FlatStyle = FlatStyle.Flat;
        btnCrearUsuario.Text = "Crear Usuario (POST /api/usuarios)";
        btnCrearUsuario.Click += btnCrearUsuario_Click;

        btnListarUsuarios.Location = new Point(304, 64);
        btnListarUsuarios.Size = new Size(275, 34);
        btnListarUsuarios.BackColor = Color.FromArgb(15, 23, 42);
        btnListarUsuarios.ForeColor = Color.White;
        btnListarUsuarios.FlatStyle = FlatStyle.Flat;
        btnListarUsuarios.Text = "Listar Usuarios (GET /api/usuarios)";
        btnListarUsuarios.Click += btnListarUsuarios_Click;

        grpUsuarioForm.Controls.AddRange(new Control[]
        {
            lblUser, txtNuevoUsername, lblPass, txtNuevoPassword, lblRol, cmbNuevoRol,
            btnCrearUsuario, btnListarUsuarios
        });

        ConfigurarGrilla(dgvUsuarios);

        tabUsuarios.Controls.Add(dgvUsuarios);
        tabUsuarios.Controls.Add(grpUsuarioForm);
    }

    private static Label CrearEtiqueta(string texto, int x, int y)
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            Location = new Point(x, y),
            Text = texto
        };
    }

    private static void ConfigurarGrilla(DataGridView grilla)
    {
        grilla.Dock = DockStyle.Fill;
        grilla.AllowUserToAddRows = false;
        grilla.AllowUserToDeleteRows = false;
        grilla.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(245, 247, 250)
        };
        grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grilla.BackgroundColor = Color.White;
        grilla.ColumnHeadersHeight = 36;
        grilla.MultiSelect = false;
        grilla.ReadOnly = true;
        grilla.RowHeadersVisible = false;
        grilla.RowTemplate.Height = 28;
        grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
    }
}
