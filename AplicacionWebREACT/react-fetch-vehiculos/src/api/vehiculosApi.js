/**
 * Cliente HTTP de la Plataforma Web React (Solo Lectura).
 *
 * Responsabilidad: autenticarse con OAuth2 (/oauth/token), verificar la identidad
 * del usuario activo (/api/identidad), consultar el catálogo de vehículos (/api/vehiculos)
 * y consultar las ventas (/api/ventas) a través del Gateway NGINX (127.0.0.1:8090).
 *
 * Cumple estrictamente el contrato arquitectónico: NO incluye funciones para crear
 * vehículos, compradores ni ventas.
 */

const BASIC_AUTH_CLIENT = 'dmVoaWN1bG9zLWNsaWVudDpjbGllbnRlLWxvY2FsLWF1bGEtMjAyNi1jYW1iaWFy';

export function normalizarToken(token = '') {
  return String(token).trim().replace(/^Bearer\s+/i, '').trim();
}

async function construirErrorHttp(respuesta, mensajesPorCodigo = {}) {
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
    // Respuesta sin cuerpo JSON; se usa mensaje por código.
  }

  const base =
    mensajesPorCodigo[respuesta.status] ??
    `La API devolvió HTTP ${respuesta.status}.`;
  const error = new Error(detalle ? `HTTP ${respuesta.status}: ${detalle}` : base);
  error.status = respuesta.status;
  return error;
}

export async function iniciarSesion({ username, password }) {
  const usuario = String(username ?? '').trim();
  const clave = String(password ?? '').trim();

  if (!usuario || !clave) {
    const error = new Error('HTTP 400: Ingrese usuario y contraseña.');
    error.status = 400;
    throw error;
  }

  const parametros = new URLSearchParams({
    grant_type: 'password',
    username: usuario,
    password: clave,
  });

  let respuesta;
  try {
    respuesta = await fetch('/oauth/token', {
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
    throw new Error('No se pudo conectar con NGINX (/oauth/token). Verifique el servidor.');
  }

  if (!respuesta.ok) {
    throw await construirErrorHttp(respuesta, {
      400: 'HTTP 400: Parámetros de autenticación inválidos.',
      401: 'HTTP 401: Usuario o contraseña incorrectos.',
      502: 'HTTP 502: NGINX no pudo contactar al servidor de autenticación Java.',
      503: 'HTTP 503: Servicio de autenticación no disponible.',
    });
  }

  const datos = await respuesta.json();
  return {
    accessToken: datos.access_token ?? '',
    tokenType: datos.token_type ?? 'bearer',
    expiresIn: datos.expires_in ?? 1800,
    codigoHttp: respuesta.status,
  };
}

export async function obtenerIdentidad(token) {
  const headers = new Headers({ Accept: 'application/json' });
  const limpio = normalizarToken(token);
  if (limpio) {
    headers.set('Authorization', `Bearer ${limpio}`);
  }

  let respuesta;
  try {
    respuesta = await fetch('/api/identidad', {
      method: 'GET',
      headers,
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con /api/identidad a través de NGINX.');
  }

  if (!respuesta.ok) {
    throw await construirErrorHttp(respuesta, {
      401: 'HTTP 401: Token JWT ausente, inválido o vencido.',
      403: 'HTTP 403: El usuario no tiene permisos para verificar identidad.',
    });
  }

  const datos = await respuesta.json();
  return {
    usuario: datos.usuario ?? 'desconocido',
    roles: Array.isArray(datos.roles) ? datos.roles : [],
    codigoHttp: respuesta.status,
  };
}

export async function listarVehiculos(token) {
  const headers = new Headers({ Accept: 'application/json' });
  const limpio = normalizarToken(token);
  if (limpio) {
    headers.set('Authorization', `Bearer ${limpio}`);
  }

  let respuesta;
  try {
    respuesta = await fetch('/api/vehiculos', {
      method: 'GET',
      headers,
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con NGINX (/api/vehiculos). Verifique el servidor y la red.');
  }

  if (!respuesta.ok) {
    throw await construirErrorHttp(respuesta, {
      401: 'HTTP 401: Falta el JWT o el token ya no es válido.',
      403: 'HTTP 403: El usuario no tiene permiso para consultar.',
      404: 'HTTP 404: No existe la ruta /api/vehiculos.',
      500: 'HTTP 500: Error del servidor o de la conexión con la API Java.',
      502: 'HTTP 502: El servidor NGINX no pudo contactar a Java.',
      503: 'HTTP 503: El servicio de vehículos no está disponible.',
    });
  }

  if (respuesta.status === 204) {
    return { vehiculos: [], codigoHttp: 204 };
  }

  let datos;
  try {
    datos = await respuesta.json();
  } catch {
    throw new Error('La API respondió, pero el JSON no pudo interpretarse.');
  }

  if (!Array.isArray(datos)) {
    throw new Error('Se esperaba una lista JSON de vehículos (un arreglo).');
  }

  return { vehiculos: datos, codigoHttp: respuesta.status };
}

export async function listarVentas(token) {
  const headers = new Headers({ Accept: 'application/json' });
  const limpio = normalizarToken(token);
  if (limpio) {
    headers.set('Authorization', `Bearer ${limpio}`);
  }

  let respuesta;
  try {
    respuesta = await fetch('/api/ventas', {
      method: 'GET',
      headers,
      cache: 'no-store',
    });
  } catch {
    throw new Error('No se pudo conectar con NGINX (/api/ventas). Verifique FastAPI y NGINX.');
  }

  if (!respuesta.ok) {
    throw await construirErrorHttp(respuesta, {
      401: 'HTTP 401: Falta el JWT o el token ya no es válido.',
      403: 'HTTP 403: El usuario no tiene permiso para consultar ventas.',
      404: 'HTTP 404: No existe la ruta /api/ventas.',
      502: 'HTTP 502: NGINX no pudo contactar con el servicio FastAPI de ventas.',
      503: 'HTTP 503: El servicio de ventas no está disponible.',
    });
  }

  if (respuesta.status === 204) {
    return { ventas: [], codigoHttp: 204 };
  }

  let datos;
  try {
    datos = await respuesta.json();
  } catch {
    throw new Error('La API de ventas respondió, pero el JSON no pudo interpretarse.');
  }

  if (!Array.isArray(datos)) {
    throw new Error('Se esperaba una lista JSON de ventas (un arreglo).');
  }

  return { ventas: datos, codigoHttp: respuesta.status };
}
