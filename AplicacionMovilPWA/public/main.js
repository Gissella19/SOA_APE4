import {
  iniciarSesion,
  obtenerIdentidad,
  listarVehiculos,
  listarCompradores,
  registrarVenta,
  consultarVentaPorId,
  listarVentas,
  generarClaveIdempotencia,
} from './api.js';

// Controles de sesión e identidad
const formLogin = document.querySelector('#formLogin');
const loginUsername = document.querySelector('#loginUsername');
const loginPassword = document.querySelector('#loginPassword');
const btnLogin = document.querySelector('#btnLogin');
const botonesPreset = document.querySelectorAll('.boton-chip');

const tarjetaIdentidad = document.querySelector('#tarjetaIdentidad');
const usuarioActivo = document.querySelector('#usuarioActivo');
const rolActivo = document.querySelector('#rolActivo');

const formulario = document.querySelector('#formulario');
const campoToken = document.querySelector('#jwt');
const botonConsultar = document.querySelector('#consultar');
const btnVerificarIdentidad = document.querySelector('#btnVerificarIdentidad');
const botonBorrar = document.querySelector('#borrar');
const botonMostrar = document.querySelector('#alternarToken');
const mensaje = document.querySelector('#mensaje');

// Controles de registro exclusivo de ventas
const formVenta = document.querySelector('#formVenta');
const selectVehiculo = document.querySelector('#selectVehiculo');
const selectComprador = document.querySelector('#selectComprador');
const campoIdempotency = document.querySelector('#idempotencyKey');
const btnNuevaClave = document.querySelector('#btnNuevaClave');
const btnRegistrarVenta = document.querySelector('#btnRegistrarVenta');
const btnRefrescarCatalogos = document.querySelector('#btnRefrescarCatalogos');
const resultadoVenta = document.querySelector('#resultadoVenta');

// Controles de seguimiento de ventas
const formBuscarVenta = document.querySelector('#formBuscarVenta');
const inputVentaId = document.querySelector('#inputVentaId');
const btnBuscarVenta = document.querySelector('#btnBuscarVenta');
const btnListarVentas = document.querySelector('#btnListarVentas');
const contadorVentas = document.querySelector('#contadorVentas');
const detalleVentaConsultada = document.querySelector('#detalleVentaConsultada');
const listaVentas = document.querySelector('#listaVentas');

// Controles de listado de vehículos
const contenedor = document.querySelector('#vehiculos');
const contador = document.querySelector('#contador');
const conexion = document.querySelector('#conexion');

const moneda = new Intl.NumberFormat('es-EC', { style: 'currency', currency: 'USD' });

campoIdempotency.value = generarClaveIdempotencia();

function mostrarMensaje(texto, tipo = 'neutro') {
  mensaje.textContent = texto;
  mensaje.className = `mensaje ${tipo}`;
}

function crearElemento(etiqueta, clase, contenido) {
  const elemento = document.createElement(etiqueta);
  if (clase) elemento.className = clase;
  if (contenido !== undefined) elemento.textContent = String(contenido);
  return elemento;
}

function texto(valor) {
  return valor === null || valor === undefined || valor === '' ? '—' : String(valor);
}

function precio(valor) {
  if (valor === null || valor === undefined || valor === '') return '—';
  const numero = Number(valor);
  return Number.isFinite(numero) ? moneda.format(numero) : texto(valor);
}

function agregarDato(grilla, etiqueta, valor, extra = '') {
  const grupo = crearElemento('div', extra);
  grupo.append(
    crearElemento('span', 'dato-etiqueta', etiqueta),
    crearElemento('span', 'dato-valor', valor),
  );
  grilla.append(grupo);
}

function actualizarTarjetaIdentidad(identidad) {
  if (!identidad) {
    tarjetaIdentidad.className = 'identidad-card sin-sesion';
    usuarioActivo.textContent = 'Sin sesión autenticada';
    rolActivo.textContent = 'SIN ROL';
    return;
  }
  const rolTexto = identidad.roles.length > 0 ? identidad.roles.join(', ') : 'AUTENTICADO';
  tarjetaIdentidad.className = 'identidad-card sesion-activa';
  usuarioActivo.textContent = `${identidad.usuario} (HTTP ${identidad.codigoHttp})`;
  rolActivo.textContent = rolTexto;
}

function poblarSelectores(vehiculos, compradores) {
  const vehiculoPrevio = selectVehiculo.value;
  selectVehiculo.replaceChildren();

  if (vehiculos.length === 0) {
    selectVehiculo.append(crearElemento('option', '', '-- Sin vehículos registrados --'));
  } else {
    for (const v of vehiculos) {
      const id = v.idVehiculo ?? v.id;
      const opt = crearElemento(
        'option',
        '',
        `#${id} · ${texto(v.marca)} ${texto(v.modelo)} (${texto(v.placa)}) · ${precio(v.precio)} · [${texto(v.estado)}]`,
      );
      opt.value = String(id);
      selectVehiculo.append(opt);
    }
    const disponible = vehiculos.find((v) => String(v.estado).toUpperCase() === 'DISPONIBLE');
    if (vehiculoPrevio && [...selectVehiculo.options].some((o) => o.value === vehiculoPrevio)) {
      selectVehiculo.value = vehiculoPrevio;
    } else if (disponible) {
      selectVehiculo.value = String(disponible.idVehiculo ?? disponible.id);
    }
  }

  const compradorPrevio = selectComprador.value;
  selectComprador.replaceChildren();
  if (compradores.length === 0) {
    selectComprador.append(
      crearElemento('option', '', '-- Sin compradores (Créelos en Windows Forms) --'),
    );
  } else {
    for (const c of compradores) {
      const opt = crearElemento(
        'option',
        '',
        `#${c.id} · ${texto(c.nombre)} (${texto(c.documento)}) · ${texto(c.correo)}`,
      );
      opt.value = String(c.id);
      selectComprador.append(opt);
    }
    if (compradorPrevio && [...selectComprador.options].some((o) => o.value === compradorPrevio)) {
      selectComprador.value = compradorPrevio;
    }
  }
}

function pintarListadoVehiculos(vehiculos) {
  contenedor.replaceChildren();
  contador.textContent = `${vehiculos.length} vehículo${vehiculos.length === 1 ? '' : 's'}`;

  if (vehiculos.length === 0) {
    contenedor.append(crearElemento('p', 'vacio', 'HTTP 200: no hay vehículos registrados.'));
    return;
  }

  for (const vehiculo of vehiculos) {
    const tarjeta = crearElemento('article', 'vehiculo');
    const cabecera = crearElemento('div', 'vehiculo-cabecera');
    const encabezado = crearElemento('div');
    encabezado.append(
      crearElemento('h3', 'vehiculo-titulo', `${texto(vehiculo.marca)} ${texto(vehiculo.modelo)}`),
      crearElemento('span', 'vehiculo-id', `ID: ${texto(vehiculo.idVehiculo ?? vehiculo.id)} · Chasis: ${texto(vehiculo.chasis)}`),
    );

    const estado = texto(vehiculo.estado);
    const claseEstado =
      String(estado).toUpperCase() === 'DISPONIBLE'
        ? 'disponible'
        : String(estado).toUpperCase() === 'VENDIDO'
          ? 'vendido'
          : '';
    cabecera.append(encabezado, crearElemento('span', `estado ${claseEstado}`, estado));

    const grilla = crearElemento('div', 'datos');
    agregarDato(grilla, 'Placa', texto(vehiculo.placa));
    agregarDato(grilla, 'Año', texto(vehiculo.anio));
    agregarDato(grilla, 'Color', texto(vehiculo.color));
    agregarDato(grilla, 'Precio', precio(vehiculo.precio), 'dato-precio');
    tarjeta.append(cabecera, grilla);
    contenedor.append(tarjeta);
  }
}

function renderizarTarjetaVenta(contenedorDestino, codigoHttp, v, tituloExtra = '') {
  contenedorDestino.replaceChildren();
  contenedorDestino.className = 'resultado-venta exito';

  const cabecera = crearElemento('div', 'vehiculo-cabecera');
  cabecera.append(
    crearElemento(
      'strong',
      '',
      `${tituloExtra || 'Venta Procesada'} · Venta #${texto(v.id)} (HTTP ${codigoHttp})`,
    ),
    crearElemento(
      'span',
      `estado ${v.notificacion === 'ENVIADA' ? 'disponible' : 'vendido'}`,
      `RabbitMQ: ${texto(v.notificacion)}`,
    ),
  );

  const grilla = crearElemento('div', 'datos');
  agregarDato(grilla, 'Operación UUID', texto(v.operacionId));
  agregarDato(grilla, 'Estado Venta', texto(v.estado));
  agregarDato(grilla, 'Vehículo ID', `#${texto(v.vehiculoId)}`);
  agregarDato(
    grilla,
    'Comprador',
    `${texto(v.compradorNombre)} (#${texto(v.compradorId)})`,
  );
  agregarDato(grilla, 'Precio Acordado', `${precio(v.precioAcordado)} ${texto(v.moneda)}`);
  agregarDato(grilla, 'Registrado por', texto(v.usuario));

  contenedorDestino.append(cabecera, grilla);
}

function pintarListadoVentas(ventas) {
  listaVentas.replaceChildren();
  contadorVentas.textContent = `${ventas.length} venta${ventas.length === 1 ? '' : 's'}`;

  if (ventas.length === 0) {
    listaVentas.append(crearElemento('p', 'vacio', 'Aún no hay ventas registradas.'));
    return;
  }

  for (const v of ventas) {
    const tarjeta = crearElemento('article', 'vehiculo');
    const cabecera = crearElemento('div', 'vehiculo-cabecera');
    cabecera.append(
      crearElemento('h3', 'vehiculo-titulo', `Venta #${texto(v.id)} · Vehículo #${texto(v.vehiculoId)}`),
      crearElemento(
        'span',
        `estado ${v.notificacion === 'ENVIADA' ? 'disponible' : 'vendido'}`,
        `${texto(v.estado)} · RabbitMQ: ${texto(v.notificacion)}`,
      ),
    );
    const grilla = crearElemento('div', 'datos');
    agregarDato(grilla, 'Comprador', `${texto(v.compradorNombre)} (${texto(v.compradorDocumento)})`);
    agregarDato(grilla, 'Precio Acordado', precio(v.precioAcordado), 'dato-precio');
    agregarDato(grilla, 'Operación UUID', texto(v.operacionId));
    agregarDato(grilla, 'Usuario / Fecha', `${texto(v.usuario)} · ${texto(v.fechaUTC)}`);
    tarjeta.append(cabecera, grilla);
    listaVentas.append(tarjeta);
  }
}

async function sincronizarTodo() {
  const token = campoToken.value;
  const identidad = await obtenerIdentidad(token);
  actualizarTarjetaIdentidad(identidad);

  const [resVehiculos, resCompradores, resVentas] = await Promise.all([
    listarVehiculos(token),
    listarCompradores(token),
    listarVentas(token),
  ]);

  pintarListadoVehiculos(resVehiculos.vehiculos);
  poblarSelectores(resVehiculos.vehiculos, resCompradores.compradores);
  pintarListadoVentas(resVentas.ventas);

  return {
    identidad,
    vehiculos: resVehiculos.vehiculos,
    compradores: resCompradores.compradores,
    ventas: resVentas.ventas,
  };
}

// Evento Login OAuth2
formLogin.addEventListener('submit', async (evento) => {
  evento.preventDefault();
  btnLogin.disabled = true;
  mostrarMensaje('Autenticando en POST /oauth/token vía NGINX…');

  try {
    const tokenRes = await iniciarSesion({
      username: loginUsername.value,
      password: loginPassword.value,
    });
    campoToken.value = tokenRes.accessToken;
    const datos = await sincronizarTodo();
    mostrarMensaje(
      `HTTP ${tokenRes.codigoHttp}: Sesión iniciada como '${datos.identidad.usuario}' [${datos.identidad.roles.join(', ')}]. ${datos.vehiculos.length} vehículos y ${datos.compradores.length} compradores cargados.`,
      'exito',
    );
  } catch (error) {
    actualizarTarjetaIdentidad(null);
    mostrarMensaje(error.message, 'error');
  } finally {
    btnLogin.disabled = false;
  }
});

for (const chip of botonesPreset) {
  chip.addEventListener('click', () => {
    loginUsername.value = chip.dataset.user || 'usuario_operador';
    loginPassword.value = 'cambiar-contrasena-aula-2026';
    formLogin.requestSubmit();
  });
}

btnVerificarIdentidad.addEventListener('click', async () => {
  try {
    const id = await obtenerIdentidad(campoToken.value);
    actualizarTarjetaIdentidad(id);
    mostrarMensaje(
      `HTTP ${id.codigoHttp}: Usuario activo '${id.usuario}' con rol [${id.roles.join(', ')}].`,
      'exito',
    );
  } catch (error) {
    actualizarTarjetaIdentidad(null);
    mostrarMensaje(error.message, 'error');
  }
});

// Sincronizar manual con el token del campo
formulario.addEventListener('submit', async (evento) => {
  evento.preventDefault();
  botonConsultar.disabled = true;
  mostrarMensaje('Sincronizando identidad, vehículos, compradores y ventas…');

  try {
    const datos = await sincronizarTodo();
    mostrarMensaje(
      `HTTP 200: Catálogos sincronizados (${datos.vehiculos.length} vehículos, ${datos.compradores.length} compradores, ${datos.ventas.length} ventas).`,
      'exito',
    );
  } catch (error) {
    mostrarMensaje(error.message, 'error');
  } finally {
    botonConsultar.disabled = false;
  }
});

btnRefrescarCatalogos.addEventListener('click', () => {
  formulario.requestSubmit();
});

btnNuevaClave.addEventListener('click', () => {
  campoIdempotency.value = generarClaveIdempotencia();
  mostrarMensaje(`Nueva clave Idempotency-Key generada: ${campoIdempotency.value}`);
});

// Registro exclusivo de venta (POST /api/ventas)
formVenta.addEventListener('submit', async (evento) => {
  evento.preventDefault();
  btnRegistrarVenta.disabled = true;
  resultadoVenta.className = 'resultado-venta neutro';
  resultadoVenta.textContent = 'Procesando POST /api/ventas con Idempotency-Key vía NGINX…';

  try {
    const res = await registrarVenta({
      token: campoToken.value,
      vehiculoId: selectVehiculo.value,
      compradorId: selectComprador.value,
      idempotencyKey: campoIdempotency.value,
    });

    const etiquetaCodigo =
      res.codigoHttp === 201
        ? 'Venta Creada (201 Created)'
        : res.codigoHttp === 200
          ? 'Reintento Idempotente Recuperado (200 OK - Sin Duplicar)'
          : `Venta en Revisión (${res.codigoHttp})`;

    renderizarTarjetaVenta(resultadoVenta, res.codigoHttp, res.venta, etiquetaCodigo);
    inputVentaId.value = String(res.venta.id);
    mostrarMensaje(
      `HTTP ${res.codigoHttp}: Venta #${res.venta.id} (${etiquetaCodigo}) · RabbitMQ: ${res.venta.notificacion}.`,
      'exito',
    );

    await sincronizarTodo();
  } catch (error) {
    resultadoVenta.className = 'resultado-venta error';
    resultadoVenta.textContent = error.message;
    mostrarMensaje(error.message, 'error');
  } finally {
    btnRegistrarVenta.disabled = false;
  }
});

// Consulta individual de venta (GET /api/ventas/{id})
formBuscarVenta.addEventListener('submit', async (evento) => {
  evento.preventDefault();
  btnBuscarVenta.disabled = true;
  try {
    const res = await consultarVentaPorId({
      token: campoToken.value,
      ventaId: inputVentaId.value,
    });
    renderizarTarjetaVenta(
      detalleVentaConsultada,
      res.codigoHttp,
      res.venta,
      'Consulta Individual GET /api/ventas/{id}',
    );
    mostrarMensaje(`HTTP ${res.codigoHttp}: Venta #${res.venta.id} consultada correctamente.`, 'exito');
  } catch (error) {
    detalleVentaConsultada.className = 'resultado-venta error';
    detalleVentaConsultada.textContent = error.message;
    mostrarMensaje(error.message, 'error');
  } finally {
    btnBuscarVenta.disabled = false;
  }
});

btnListarVentas.addEventListener('click', async () => {
  try {
    const res = await listarVentas(campoToken.value);
    pintarListadoVentas(res.ventas);
    mostrarMensaje(`HTTP ${res.codigoHttp}: ${res.ventas.length} venta(s) listadas.`, 'exito');
  } catch (error) {
    mostrarMensaje(error.message, 'error');
  }
});

botonMostrar.addEventListener('click', () => {
  const revelar = campoToken.type === 'password';
  campoToken.type = revelar ? 'text' : 'password';
  botonMostrar.textContent = revelar ? 'Ocultar' : 'Mostrar';
  botonMostrar.setAttribute('aria-pressed', String(revelar));
  botonMostrar.setAttribute('aria-label', revelar ? 'Ocultar token' : 'Mostrar token');
});

botonBorrar.addEventListener('click', () => {
  campoToken.value = '';
  campoToken.type = 'password';
  botonMostrar.textContent = 'Mostrar';
  botonMostrar.setAttribute('aria-pressed', 'false');
  actualizarTarjetaIdentidad(null);
  mostrarMensaje('Sesión cerrada y token eliminado de la memoria.');
  campoToken.focus();
});

function actualizarConexion() {
  conexion.textContent = navigator.onLine ? 'Navegador conectado' : 'Sin conexión de red';
}
window.addEventListener('online', actualizarConexion);
window.addEventListener('offline', actualizarConexion);
actualizarConexion();

if ('serviceWorker' in navigator && window.isSecureContext) {
  window.addEventListener('load', () => {
    navigator.serviceWorker.register('/sw.js').catch(() => {});
  });
}
