/**
 * Cliente HTTP de la Aplicación Móvil PWA.
 * Todas las solicitudes se enrutan a través del proxy local hacia NGINX (http://127.0.0.1:8090).
 * Responsabilidad arquitectónica exclusiva: Autenticación OAuth2, verificación de identidad
 * y registro exclusivo de ventas con soporte de Idempotency-Key.
 */

const BASIC_AUTH_CLIENT = 'dmVoaWN1bG9zLWNsaWVudDpjbGllbnRlLWxvY2FsLWF1bGEtMjAyNi1jYW1iaWFy';

export function normalizarToken(entrada = '') {
  return String(entrada).trim().replace(/^Bearer\s+/i, '').trim();
}

export function generarClaveIdempotencia() {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return `venta-${crypto.randomUUID()}`;
  }
  const aleatorio = Math.random().toString(36).substring(2, 12);
  return `venta-${Date.now()}-${aleatorio}`;
}

async function extraerMensajeError(respuesta, mensajesPorCodigo = {}) {
  let detalle = '';
  try {
    const cuerpo = await respuesta.json();
    detalle =
      cuerpo?.detail ||
      cuerpo?.mensaje ||
      cuerpo?.message ||
      cuerpo?.title ||
      cuerpo?.error ||
      '';
  } catch {
    // Si no es JSON, se emplea el mensaje estándar por código HTTP.
  }

  const base =
    mensajesPorCodigo[respuesta.status] ||
    `HTTP ${respuesta.status}: operación no completada.`;
  return detalle ? `HTTP ${respuesta.status}: ${detalle}` : base;
}

export async function iniciarSesion({ username, password }, ejecutarFetch = fetch) {
  const usuarioLimpio = String(username ?? '').trim();
  const claveLimpia = String(password ?? '').trim();

  if (!usuarioLimpio || !claveLimpia) {
    const err = new Error('HTTP 400: Ingrese usuario y contraseña.');
    err.codigoHttp = 400;
    throw err;
  }

  const parametros = new URLSearchParams({
    grant_type: 'password',
    username: usuarioLimpio,
    password: claveLimpia,
  });

  let respuesta;
  try {
    respuesta = await ejecutarFetch('/oauth/token', {
      method: 'POST',
      headers: new Headers({
        Authorization: `Basic ${BASIC_AUTH_CLIENT}`,
        'Content-Type': 'application/x-www-form-urlencoded',
        Accept: 'application/json',
      }),
      body: parametros.toString(),
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con NGINX (/oauth/token). Verifique el servicio.');
  }

  if (!respuesta.ok) {
    const texto = await extraerMensajeError(respuesta, {
      400: 'HTTP 400: Parámetros de autenticación inválidos.',
      401: 'HTTP 401: Usuario o contraseña incorrectos.',
      502: 'HTTP 502: NGINX no pudo conectar con el servicio de autenticación Java.',
      503: 'HTTP 503: Servicio de autenticación no disponible.',
    });
    const error = new Error(texto);
    error.codigoHttp = respuesta.status;
    throw error;
  }

  const datos = await respuesta.json();
  return {
    codigoHttp: respuesta.status,
    accessToken: datos.access_token || '',
    tokenType: datos.token_type || 'bearer',
    expiresIn: datos.expires_in || 1800,
    scope: datos.scope || 'vehiculos',
  };
}

export async function obtenerIdentidad(entradaToken, ejecutarFetch = fetch) {
  const jwt = normalizarToken(entradaToken);
  const encabezados = new Headers({ Accept: 'application/json' });
  if (jwt) encabezados.set('Authorization', `Bearer ${jwt}`);

  let respuesta;
  try {
    respuesta = await ejecutarFetch('/api/identidad', {
      method: 'GET',
      headers: encabezados,
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con /api/identidad a través de NGINX.');
  }

  if (!respuesta.ok) {
    const texto = await extraerMensajeError(respuesta, {
      401: 'HTTP 401: JWT ausente, inválido o vencido.',
      403: 'HTTP 403: Acceso denegado al verificar identidad.',
    });
    const error = new Error(texto);
    error.codigoHttp = respuesta.status;
    throw error;
  }

  const datos = await respuesta.json();
  return {
    codigoHttp: respuesta.status,
    usuario: datos.usuario || 'desconocido',
    roles: Array.isArray(datos.roles) ? datos.roles : [],
  };
}

export async function listarVehiculos(entradaToken, ejecutarFetch = fetch) {
  const jwt = normalizarToken(entradaToken);
  const encabezados = new Headers({ Accept: 'application/json' });
  if (jwt) encabezados.set('Authorization', `Bearer ${jwt}`);

  let respuesta;
  try {
    respuesta = await ejecutarFetch('/api/vehiculos', {
      method: 'GET',
      headers: encabezados,
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con el servicio. Compruebe la red y Spring Boot.');
  }

  if (!respuesta.ok) {
    const mensajes = {
      401: 'HTTP 401: JWT ausente, inválido o vencido.',
      403: 'HTTP 403: el usuario no tiene permiso para consultar.',
      404: 'HTTP 404: no se encontró /api/vehiculos.',
      502: 'HTTP 502: no se pudo establecer conexión con Spring Boot.',
      503: 'HTTP 503: el servicio no está disponible.',
      504: 'HTTP 504: Spring Boot no respondió a tiempo.',
    };
    const error = new Error(
      mensajes[respuesta.status] || `HTTP ${respuesta.status}: consulta no completada.`,
    );
    error.codigoHttp = respuesta.status;
    throw error;
  }

  let datos;
  try {
    datos = await respuesta.json();
  } catch {
    throw new Error('La API devolvió una respuesta que no es JSON válido.');
  }
  if (!Array.isArray(datos)) {
    throw new Error('La API debe devolver un arreglo JSON de vehículos.');
  }
  return { codigoHttp: respuesta.status, vehiculos: datos };
}

export async function listarCompradores(entradaToken, ejecutarFetch = fetch) {
  const jwt = normalizarToken(entradaToken);
  const encabezados = new Headers({ Accept: 'application/json' });
  if (jwt) encabezados.set('Authorization', `Bearer ${jwt}`);

  let respuesta;
  try {
    respuesta = await ejecutarFetch('/api/compradores', {
      method: 'GET',
      headers: encabezados,
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con /api/compradores a través de NGINX.');
  }

  if (!respuesta.ok) {
    const texto = await extraerMensajeError(respuesta, {
      401: 'HTTP 401: JWT ausente, inválido o vencido.',
      403: 'HTTP 403: El usuario no tiene permiso para consultar compradores.',
      502: 'HTTP 502: FastAPI no está disponible a través de NGINX.',
    });
    const error = new Error(texto);
    error.codigoHttp = respuesta.status;
    throw error;
  }

  const datos = await respuesta.json();
  if (!Array.isArray(datos)) {
    throw new Error('La API debe devolver un arreglo JSON de compradores.');
  }
  return { codigoHttp: respuesta.status, compradores: datos };
}

export async function registrarVenta(
  { token, vehiculoId, compradorId, idempotencyKey },
  ejecutarFetch = fetch,
) {
  const jwt = normalizarToken(token);
  const clave = String(idempotencyKey ?? '').trim();

  if (!clave || clave.length < 8) {
    const err = new Error(
      'HTTP 422: La cabecera Idempotency-Key es obligatoria (mínimo 8 caracteres).',
    );
    err.codigoHttp = 422;
    throw err;
  }

  const idVeh = Number(vehiculoId);
  const idComp = Number(compradorId);
  if (!Number.isInteger(idVeh) || idVeh <= 0 || !Number.isInteger(idComp) || idComp <= 0) {
    const err = new Error('HTTP 422: Seleccione un vehículo y un comprador válidos.');
    err.codigoHttp = 422;
    throw err;
  }

  const encabezados = new Headers({
    Accept: 'application/json',
    'Content-Type': 'application/json',
    'Idempotency-Key': clave,
  });
  if (jwt) encabezados.set('Authorization', `Bearer ${jwt}`);

  let respuesta;
  try {
    respuesta = await ejecutarFetch('/api/ventas', {
      method: 'POST',
      headers: encabezados,
      body: JSON.stringify({ vehiculoId: idVeh, compradorId: idComp }),
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con /api/ventas a través de NGINX.');
  }

  if (!respuesta.ok) {
    const texto = await extraerMensajeError(respuesta, {
      401: 'HTTP 401: JWT ausente, inválido o vencido.',
      403: 'HTTP 403 Forbidden: El rol actual (ROLE_CONSULTA) no tiene permiso para registrar ventas.',
      404: 'HTTP 404: El vehículo o el comprador seleccionado no existe.',
      409: 'HTTP 409 Conflict: Vehículo no disponible o Idempotency-Key reutilizada con distinto cuerpo.',
      422: 'HTTP 422: Datos de venta o Idempotency-Key inválidos.',
      502: 'HTTP 502: Error de comunicación con el servicio de ventas.',
      503: 'HTTP 503: Servicio temporalmente no disponible.',
    });
    const error = new Error(texto);
    error.codigoHttp = respuesta.status;
    throw error;
  }

  const venta = await respuesta.json();
  return { codigoHttp: respuesta.status, venta };
}

export async function consultarVentaPorId({ token, ventaId }, ejecutarFetch = fetch) {
  const jwt = normalizarToken(token);
  const id = Number(ventaId);
  if (!Number.isInteger(id) || id <= 0) {
    const err = new Error('HTTP 400: Ingrese un ID de venta entero positivo.');
    err.codigoHttp = 400;
    throw err;
  }

  const encabezados = new Headers({ Accept: 'application/json' });
  if (jwt) encabezados.set('Authorization', `Bearer ${jwt}`);

  let respuesta;
  try {
    respuesta = await ejecutarFetch(`/api/ventas/${id}`, {
      method: 'GET',
      headers: encabezados,
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con /api/ventas/{id} a través de NGINX.');
  }

  if (!respuesta.ok) {
    const texto = await extraerMensajeError(respuesta, {
      401: 'HTTP 401: JWT ausente, inválido o vencido.',
      403: 'HTTP 403: Sin permisos para consultar la venta.',
      404: `HTTP 404: No existe la venta con ID ${id}.`,
    });
    const error = new Error(texto);
    error.codigoHttp = respuesta.status;
    throw error;
  }

  const venta = await respuesta.json();
  return { codigoHttp: respuesta.status, venta };
}

export async function listarVentas(entradaToken, ejecutarFetch = fetch) {
  const jwt = normalizarToken(entradaToken);
  const encabezados = new Headers({ Accept: 'application/json' });
  if (jwt) encabezados.set('Authorization', `Bearer ${jwt}`);

  let respuesta;
  try {
    respuesta = await ejecutarFetch('/api/ventas', {
      method: 'GET',
      headers: encabezados,
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con /api/ventas a través de NGINX.');
  }

  if (!respuesta.ok) {
    const texto = await extraerMensajeError(respuesta, {
      401: 'HTTP 401: JWT ausente, inválido o vencido.',
      403: 'HTTP 403: Sin permisos para listar ventas.',
    });
    const error = new Error(texto);
    error.codigoHttp = respuesta.status;
    throw error;
  }

  const ventas = await respuesta.json();
  if (!Array.isArray(ventas)) {
    throw new Error('La API debe devolver un arreglo JSON de ventas.');
  }
  return { codigoHttp: respuesta.status, ventas };
}
