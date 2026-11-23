import { useState } from 'react';
import {
  iniciarSesion,
  obtenerIdentidad,
  listarVehiculos,
  listarVentas,
} from './api/vehiculosApi.js';
import './App.css';

const dinero = new Intl.NumberFormat('es-EC', {
  style: 'currency',
  currency: 'USD',
});

function mostrarPrecio(precio) {
  if (precio === null || precio === undefined || precio === '') return '—';
  const numero = Number(precio);
  return Number.isFinite(numero) ? dinero.format(numero) : String(precio);
}

export default function App() {
  // Estado de autenticación e identidad
  const [username, setUsername] = useState('usuario_consulta');
  const [password, setPassword] = useState('cambiar-contrasena-aula-2026');
  const [token, setToken] = useState('');
  const [mostrarToken, setMostrarToken] = useState(false);
  const [identidad, setIdentidad] = useState(null);

  // Estado de datos de catálogo y ventas (Solo Lectura)
  const [vehiculos, setVehiculos] = useState([]);
  const [ventas, setVentas] = useState([]);
  const [marca, setMarca] = useState('');
  const [estadoFiltro, setEstadoFiltro] = useState('TODOS');
  const [filtroVenta, setFiltroVenta] = useState('');
  const [seleccionado, setSeleccionado] = useState(null);
  const [ventaSeleccionada, setVentaSeleccionada] = useState(null);

  // Estado de interfaz y transporte HTTP
  const [cargando, setCargando] = useState(false);
  const [mensaje, setMensaje] = useState('Listo para iniciar sesión o consultar.');
  const [error, setError] = useState('');
  const [codigoHttp, setCodigoHttp] = useState(null);

  // Filtrado en cliente sobre los vehículos recibidos
  const vehiculosVisibles = vehiculos.filter((v) => {
    const coincideMarca = String(v.marca ?? '')
      .toLowerCase()
      .includes(marca.trim().toLowerCase());
    const coincideEstado =
      estadoFiltro === 'TODOS' ||
      String(v.estado ?? '').toUpperCase() === estadoFiltro;
    return coincideMarca && coincideEstado;
  });

  // Filtrado de ventas por ID, comprador, documento o estado
  const ventasVisibles = ventas.filter((venta) => {
    const termino = filtroVenta.trim().toLowerCase();
    if (!termino) return true;
    return (
      String(venta.id ?? '').toLowerCase().includes(termino) ||
      String(venta.operacionId ?? '').toLowerCase().includes(termino) ||
      String(venta.vehiculoId ?? '').toLowerCase().includes(termino) ||
      String(venta.compradorNombre ?? '').toLowerCase().includes(termino) ||
      String(venta.compradorDocumento ?? '').toLowerCase().includes(termino) ||
      String(venta.estado ?? '').toLowerCase().includes(termino)
    );
  });

  // Indicadores KPI reales calculados desde las respuestas HTTP
  const totalVehiculos = vehiculos.length;
  const totalDisponibles = vehiculos.filter(
    (v) => String(v.estado ?? '').toUpperCase() === 'DISPONIBLE',
  ).length;
  const totalVendidos = vehiculos.filter(
    (v) => String(v.estado ?? '').toUpperCase() === 'VENDIDO',
  ).length;
  const ventasConfirmadas = ventas.filter(
    (v) => String(v.estado ?? '').toUpperCase() === 'COMPLETADA',
  );
  const montoTotalVentas = ventasConfirmadas.reduce((acc, v) => {
    const num = Number(v.precioAcordado);
    return Number.isFinite(num) ? acc + num : acc;
  }, 0);

  async function cargarDatosConToken(jwtActivo) {
    const [idRes, vehRes, venRes] = await Promise.all([
      obtenerIdentidad(jwtActivo),
      listarVehiculos(jwtActivo),
      listarVentas(jwtActivo),
    ]);
    setIdentidad(idRes);
    setVehiculos(vehRes.vehiculos);
    setVentas(venRes.ventas);
    setCodigoHttp(vehRes.codigoHttp);
    if (vehRes.vehiculos.length > 0) {
      setSeleccionado(vehRes.vehiculos[0]);
    }
    if (venRes.ventas.length > 0) {
      setVentaSeleccionada(venRes.ventas[0]);
    }
    return { idRes, vehRes, venRes };
  }

  async function manejarLogin(evento, userOverride, passOverride) {
    if (evento) evento.preventDefault();
    const usr = userOverride ?? username;
    const pwd = passOverride ?? password;

    setCargando(true);
    setError('');
    setMensaje(`Autenticando '${usr}' en POST /oauth/token vía NGINX...`);
    setCodigoHttp(null);

    try {
      const loginRes = await iniciarSesion({ username: usr, password: pwd });
      setToken(loginRes.accessToken);
      const { idRes, vehRes, venRes } = await cargarDatosConToken(loginRes.accessToken);
      setMensaje(
        `Sesión activa como '${idRes.usuario}' [${idRes.roles.join(', ')}] · ${vehRes.vehiculos.length} vehículos y ${venRes.ventas.length} ventas consultadas.`,
      );
    } catch (e) {
      setIdentidad(null);
      setCodigoHttp(e.status ?? null);
      setError(e.message);
      setMensaje('No se pudo completar el inicio de sesión.');
    } finally {
      setCargando(false);
    }
  }

  async function consultarCatalogoYVentas(evento) {
    if (evento) evento.preventDefault();
    setCargando(true);
    setError('');
    setMensaje('Consultando identidad, catálogo de vehículos y listado de ventas vía NGINX...');
    setCodigoHttp(null);
    setVehiculos([]);
    setVentas([]);
    setSeleccionado(null);
    setVentaSeleccionada(null);

    try {
      const { idRes, vehRes, venRes } = await cargarDatosConToken(token);
      setMensaje(
        `Consulta correcta (${idRes.usuario}): ${vehRes.vehiculos.length} vehículos y ${venRes.ventas.length} ventas.`,
      );
    } catch (e) {
      setIdentidad(null);
      setCodigoHttp(e.status ?? null);
      setError(e.message);
      setMensaje('La consulta no pudo completarse.');
    } finally {
      setCargando(false);
    }
  }

  function seleccionarPerfilRapido(nuevoUsuario) {
    const claveAula = 'cambiar-contrasena-aula-2026';
    setUsername(nuevoUsuario);
    setPassword(claveAula);
    manejarLogin(null, nuevoUsuario, claveAula);
  }

  return (
    <main className="contenedor">
      <header className="cabecera">
        <div className="cabecera-superior">
          <div>
            <div className="marca-producto">APE 04 · REACT + VITE + FETCH · GATEWAY NGINX (127.0.0.1:8090)</div>
            <h1>Plataforma Web React — Consulta de Catálogo y Seguimiento de Ventas (Solo Lectura)</h1>
            <p className="subtitulo">
              Rol arquitectónico: Consulta de catálogo de vehículos (<code>GET /api/vehiculos</code>) y seguimiento de ventas (<code>GET /api/ventas</code>). Sin operaciones de escritura.
            </p>
          </div>
          <div className={identidad ? 'badge-identidad activa' : 'badge-identidad inactiva'}>
            <span className="badge-etiqueta">IDENTIDAD ACTIVA (GET /api/identidad)</span>
            <strong>
              {identidad
                ? `Usuario activo: ${identidad.usuario}`
                : 'Usuario activo: Sin autenticar'}
            </strong>
            <span className="badge-rol">
              {identidad
                ? `Rol: ${identidad.roles.join(', ') || 'AUTENTICADO'}`
                : 'Rol: —'}
            </span>
          </div>
        </div>
      </header>

      {/* PANEL 1: SESIÓN Y TOKEN JWT */}
      <section className="panel" aria-labelledby="titulo-acceso">
        <div className="titulo-listado">
          <div>
            <h2 id="titulo-acceso">1. Autenticación OAuth2 e Inspector de Token JWT</h2>
            <p className="ayuda">
              Inicie sesión contra <code>POST /oauth/token</code> a través de NGINX (<code>127.0.0.1:8090</code>) o utilice un token JWT en memoria.
            </p>
          </div>
          <span className="badge-lectura">MODO SOLO LECTURA</span>
        </div>

        <form className="form-login" onSubmit={(e) => manejarLogin(e)}>
          <div className="grilla-login">
            <div>
              <label htmlFor="loginUser">Usuario</label>
              <input
                id="loginUser"
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                disabled={cargando}
              />
            </div>
            <div>
              <label htmlFor="loginPass">Contraseña</label>
              <input
                id="loginPass"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                disabled={cargando}
              />
            </div>
            <div className="columna-boton">
              <button type="submit" disabled={cargando}>
                {cargando ? 'Autenticando...' : 'Iniciar Sesión (/oauth/token)'}
              </button>
            </div>
          </div>

          <div className="barra-presets">
            <span className="presets-label">Perfiles rápidos de defensa:</span>
            <button
              type="button"
              className="chip"
              disabled={cargando}
              onClick={() => seleccionarPerfilRapido('usuario_consulta')}
            >
              usuario_consulta (ROLE_CONSULTA)
            </button>
            <button
              type="button"
              className="chip"
              disabled={cargando}
              onClick={() => seleccionarPerfilRapido('usuario_operador')}
            >
              usuario_operador (ROLE_OPERADOR)
            </button>
            <button
              type="button"
              className="chip"
              disabled={cargando}
              onClick={() => seleccionarPerfilRapido('usuario_admin')}
            >
              usuario_admin (ROLE_ADMIN)
            </button>
          </div>
        </form>

        <form className="form-token" onSubmit={consultarCatalogoYVentas}>
          <label htmlFor="jwt">Token JWT en memoria (Authorization: Bearer)</label>
          <div className="linea-token">
            <input
              id="jwt"
              type={mostrarToken ? 'text' : 'password'}
              autoComplete="off"
              spellCheck="false"
              placeholder="Token JWT obtenido de /oauth/token o pegado manualmente"
              value={token}
              onChange={(e) => setToken(e.target.value)}
            />
            <button
              className="secundario"
              type="button"
              onClick={() => setMostrarToken((actual) => !actual)}
              aria-label={mostrarToken ? 'Ocultar token' : 'Mostrar token'}
            >
              {mostrarToken ? 'Ocultar' : 'Mostrar'}
            </button>
          </div>

          <div className="acciones">
            <button type="submit" disabled={cargando}>
              {cargando ? 'Consultando...' : 'Consultar Catálogo y Ventas (GET)'}
            </button>
            <button
              className="secundario"
              type="button"
              disabled={cargando}
              onClick={() => {
                setToken('');
                setIdentidad(null);
                setVehiculos([]);
                setVentas([]);
                setSeleccionado(null);
                setVentaSeleccionada(null);
                setMostrarToken(false);
                setError('');
                setCodigoHttp(null);
                setMensaje('Sesión limpiada de la memoria.');
              }}
            >
              Limpiar sesión
            </button>
          </div>
        </form>

        <div
          className={error ? 'estado estado-error' : 'estado estado-ok'}
          role="status"
          aria-live="polite"
        >
          <span>{error || mensaje}</span>
          {codigoHttp !== null && <strong>HTTP {codigoHttp}</strong>}
        </div>
        <p className="nota">
          Arquitectura Stateless: El JWT permanece en memoria de React y no se persiste en localStorage.
        </p>
      </section>

      {/* PANEL KPI: INDICADORES EN TIEMPO REAL */}
      <section className="kpis-grilla" aria-label="Indicadores clave de catálogo y ventas">
        <article className="kpi-card">
          <span className="kpi-titulo">TOTAL VEHÍCULOS</span>
          <strong className="kpi-valor">{totalVehiculos}</strong>
          <span className="kpi-sub">Registros en catálogo Java</span>
        </article>
        <article className="kpi-card kpi-verde">
          <span className="kpi-titulo">VEHÍCULOS DISPONIBLES</span>
          <strong className="kpi-valor">{totalDisponibles}</strong>
          <span className="kpi-sub">Listos para venta en PWA</span>
        </article>
        <article className="kpi-card kpi-ambar">
          <span className="kpi-titulo">VEHÍCULOS VENDIDOS</span>
          <strong className="kpi-valor">{totalVendidos}</strong>
          <span className="kpi-sub">Reserva confirmada</span>
        </article>
        <article className="kpi-card kpi-azul">
          <span className="kpi-titulo">VENTAS CONFIRMADAS</span>
          <strong className="kpi-valor">{ventasConfirmadas.length}</strong>
          <span className="kpi-sub">Total acumulado: {mostrarPrecio(montoTotalVentas)}</span>
        </article>
      </section>

      {/* PANEL 2: CATÁLOGO DE VEHÍCULOS (P14, P15) */}
      <section className="panel" aria-labelledby="titulo-listado">
        <div className="titulo-listado">
          <div>
            <h2 id="titulo-listado">2. Catálogo de Vehículos (GET /api/vehiculos)</h2>
            <p className="ayuda">
              Filtre por marca o estado y seleccione cualquier fila para inspeccionar la ficha técnica completa del vehículo.
            </p>
          </div>
          <span className="contador">{vehiculosVisibles.length} visibles</span>
        </div>

        <div className="filtros-barra">
          <div>
            <label htmlFor="filtroMarca">Filtrar por Marca</label>
            <input
              id="filtroMarca"
              type="search"
              placeholder="Ejemplo: Toyota"
              value={marca}
              onChange={(e) => setMarca(e.target.value)}
              disabled={cargando}
            />
          </div>
          <div>
            <label htmlFor="filtroEstado">Filtrar por Estado</label>
            <select
              id="filtroEstado"
              value={estadoFiltro}
              onChange={(e) => setEstadoFiltro(e.target.value)}
              disabled={cargando}
            >
              <option value="TODOS">TODOS LOS ESTADOS</option>
              <option value="DISPONIBLE">DISPONIBLE</option>
              <option value="VENDIDO">VENDIDO</option>
              <option value="RESERVADO">RESERVADO</option>
            </select>
          </div>
        </div>

        <div className="tabla-scroll">
          <table>
            <thead>
              <tr>
                <th scope="col">ID</th>
                <th scope="col">Marca</th>
                <th scope="col">Modelo</th>
                <th scope="col">Placa</th>
                <th scope="col">Año</th>
                <th scope="col">Color</th>
                <th scope="col">Precio</th>
                <th scope="col">Estado</th>
                <th scope="col">Acción</th>
              </tr>
            </thead>
            <tbody>
              {vehiculosVisibles.map((v, indice) => {
                const id = v.idVehiculo ?? v.id ?? indice;
                const esActivo = (seleccionado?.idVehiculo ?? seleccionado?.id) === id;
                const est = String(v.estado ?? '').toUpperCase();
                return (
                  <tr
                    key={id}
                    className={esActivo ? 'fila-seleccionada' : ''}
                    onClick={() => setSeleccionado(v)}
                  >
                    <td>
                      <strong>#{id}</strong>
                    </td>
                    <td>{v.marca ?? '—'}</td>
                    <td>{v.modelo ?? '—'}</td>
                    <td><code>{v.placa ?? '—'}</code></td>
                    <td>{v.anio ?? '—'}</td>
                    <td>{v.color ?? '—'}</td>
                    <td><strong>{mostrarPrecio(v.precio)}</strong></td>
                    <td>
                      <span className={`pill pill-${est.toLowerCase()}`}>
                        {v.estado ?? '—'}
                      </span>
                    </td>
                    <td>
                      <button
                        className="enlace-id"
                        type="button"
                        onClick={() => setSeleccionado(v)}
                      >
                        Ver detalle
                      </button>
                    </td>
                  </tr>
                );
              })}
              {vehiculosVisibles.length === 0 && (
                <tr>
                  <td className="sin-datos" colSpan="9">
                    {cargando
                      ? 'Cargando catálogo de vehículos...'
                      : vehiculos.length === 0
                        ? 'Aún no hay vehículos cargados. Inicie sesión o presione Consultar Catálogo y Ventas.'
                        : 'No hay vehículos que coincidan con el filtro de marca y estado.'}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </section>

      {/* PANEL 3: DETALLE DEL VEHÍCULO SELECCIONADO (P15) */}
      <section className="panel" aria-labelledby="titulo-detalle">
        <h2 id="titulo-detalle">3. Detalle del Vehículo Seleccionado</h2>
        {seleccionado ? (
          <dl className="detalle-grilla">
            <div><dt>ID Vehículo</dt><dd>#{seleccionado.idVehiculo ?? seleccionado.id ?? '—'}</dd></div>
            <div><dt>Marca</dt><dd>{seleccionado.marca ?? '—'}</dd></div>
            <div><dt>Modelo</dt><dd>{seleccionado.modelo ?? '—'}</dd></div>
            <div><dt>Placa</dt><dd><code>{seleccionado.placa ?? '—'}</code></dd></div>
            <div><dt>Número de Chasis (VIN)</dt><dd><code>{seleccionado.chasis ?? '—'}</code></dd></div>
            <div><dt>Año de Fabricación</dt><dd>{seleccionado.anio ?? '—'}</dd></div>
            <div><dt>Color</dt><dd>{seleccionado.color ?? '—'}</dd></div>
            <div><dt>Precio Oficial (USD)</dt><dd className="precio-destacado">{mostrarPrecio(seleccionado.precio)}</dd></div>
            <div><dt>Estado Actual</dt><dd><span className={`pill pill-${String(seleccionado.estado ?? '').toLowerCase()}`}>{seleccionado.estado ?? '—'}</span></dd></div>
          </dl>
        ) : (
          <p className="ayuda">Seleccione cualquier vehículo de la tabla superior para visualizar su ficha completa.</p>
        )}
      </section>

      {/* PANEL 4: SEGUIMIENTO DE VENTAS (P16 - SOLO LECTURA) */}
      <section className="panel" aria-labelledby="titulo-ventas">
        <div className="titulo-listado">
          <div>
            <h2 id="titulo-ventas">4. Seguimiento de Ventas Registradas (GET /api/ventas)</h2>
            <p className="ayuda">
              Consulta en tiempo real de las ventas procesadas por FastAPI y notificadas a RabbitMQ. (El registro de nuevas ventas es exclusivo de la PWA Móvil).
            </p>
          </div>
          <span className="contador">{ventasVisibles.length} ventas</span>
        </div>

        <label htmlFor="filtroVenta">Buscar venta por ID, comprador, documento, vehículo o UUID de operación</label>
        <input
          id="filtroVenta"
          type="search"
          placeholder="Ejemplo: Carlos, 180, COMPLETADA o ID de venta"
          value={filtroVenta}
          onChange={(e) => setFiltroVenta(e.target.value)}
          disabled={cargando}
        />

        <div className="tabla-scroll">
          <table>
            <thead>
              <tr>
                <th scope="col">ID Venta</th>
                <th scope="col">Operación ID</th>
                <th scope="col">Vehículo ID</th>
                <th scope="col">Comprador</th>
                <th scope="col">Documento</th>
                <th scope="col">Precio Acordado</th>
                <th scope="col">Estado</th>
                <th scope="col">Notificación RabbitMQ</th>
                <th scope="col">Fecha UTC</th>
              </tr>
            </thead>
            <tbody>
              {ventasVisibles.map((venta) => {
                const esActiva = ventaSeleccionada?.id === venta.id;
                return (
                  <tr
                    key={venta.id}
                    className={esActiva ? 'fila-seleccionada' : ''}
                    onClick={() => setVentaSeleccionada(venta)}
                  >
                    <td><strong>#{venta.id}</strong></td>
                    <td><code>{venta.operacionId ?? '—'}</code></td>
                    <td>#{venta.vehiculoId ?? '—'}</td>
                    <td>{venta.compradorNombre ?? `Comprador #${venta.compradorId ?? '—'}`}</td>
                    <td><code>{venta.compradorDocumento ?? '—'}</code></td>
                    <td><strong>{mostrarPrecio(venta.precioAcordado)} {venta.moneda ?? 'USD'}</strong></td>
                    <td>
                      <span className={`pill pill-${String(venta.estado ?? '').toLowerCase()}`}>
                        {venta.estado ?? '—'}
                      </span>
                    </td>
                    <td>
                      <span className={`pill pill-${String(venta.notificacion ?? '').toLowerCase()}`}>
                        {venta.notificacion ?? '—'}
                      </span>
                    </td>
                    <td>{venta.fechaUTC ?? '—'}</td>
                  </tr>
                );
              })}
              {ventasVisibles.length === 0 && (
                <tr>
                  <td className="sin-datos" colSpan="9">
                    {cargando
                      ? 'Consultando ventas en FastAPI...'
                      : ventas.length === 0
                        ? 'No hay ventas cargadas todavía. Inicie sesión o presione Consultar Catálogo y Ventas.'
                        : 'Ninguna venta coincide con el criterio de búsqueda.'}
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {ventaSeleccionada && (
          <div className="subpanel-detalle-venta">
            <h3>Detalle de Operación de Venta #{ventaSeleccionada.id}</h3>
            <dl className="detalle-grilla">
              <div><dt>ID Venta</dt><dd>#{ventaSeleccionada.id}</dd></div>
              <div><dt>UUID Operación</dt><dd><code>{ventaSeleccionada.operacionId}</code></dd></div>
              <div><dt>ID Vehículo Vendido</dt><dd>#{ventaSeleccionada.vehiculoId}</dd></div>
              <div><dt>Comprador</dt><dd>{ventaSeleccionada.compradorNombre ?? '—'} (ID #{ventaSeleccionada.compradorId})</dd></div>
              <div><dt>Documento Comprador</dt><dd><code>{ventaSeleccionada.compradorDocumento ?? '—'}</code></dd></div>
              <div><dt>Precio Oficial Acordado</dt><dd className="precio-destacado">{mostrarPrecio(ventaSeleccionada.precioAcordado)} {ventaSeleccionada.moneda ?? 'USD'}</dd></div>
              <div><dt>Estado de Transacción</dt><dd>{ventaSeleccionada.estado}</dd></div>
              <div><dt>Estado Cola RabbitMQ</dt><dd>{ventaSeleccionada.notificacion}</dd></div>
              <div><dt>Registrado Por / Fecha UTC</dt><dd>{ventaSeleccionada.usuario} · {ventaSeleccionada.fechaUTC}</dd></div>
            </dl>
          </div>
        )}
      </section>
    </main>
  );
}
