import test from 'node:test';
import assert from 'node:assert/strict';
import {
  listarVehiculos,
  normalizarToken,
  iniciarSesion,
  obtenerIdentidad,
  registrarVenta,
  consultarVentaPorId,
} from '../public/api.js';

test('normaliza un token pegado con prefijo Bearer', () => {
  assert.equal(normalizarToken('  bearer abc.def.ghi  '), 'abc.def.ghi');
});

test('POST /oauth/token envía Basic Auth y grant_type=password', async () => {
  let solicitud;
  const simularFetch = async (ruta, opciones) => {
    solicitud = { ruta, opciones };
    return new Response(
      JSON.stringify({
        access_token: 'jwt.generado.ok',
        token_type: 'bearer',
        expires_in: 1800,
      }),
      { status: 200, headers: { 'Content-Type': 'application/json' } },
    );
  };
  const res = await iniciarSesion(
    { username: 'usuario_operador', password: 'cambiar-contrasena-aula-2026' },
    simularFetch,
  );
  assert.equal(solicitud.ruta, '/oauth/token');
  assert.equal(solicitud.opciones.method, 'POST');
  assert.match(solicitud.opciones.headers.get('Authorization'), /^Basic /);
  assert.equal(res.accessToken, 'jwt.generado.ok');
});

test('GET /api/identidad devuelve usuario y roles activos', async () => {
  const simularFetch = async (ruta, opciones) => {
    assert.equal(ruta, '/api/identidad');
    assert.equal(opciones.headers.get('Authorization'), 'Bearer token.activo');
    return new Response(
      JSON.stringify({ usuario: 'usuario_operador', roles: ['ROLE_OPERADOR'] }),
      { status: 200, headers: { 'Content-Type': 'application/json' } },
    );
  };
  const res = await obtenerIdentidad('token.activo', simularFetch);
  assert.equal(res.usuario, 'usuario_operador');
  assert.deepEqual(res.roles, ['ROLE_OPERADOR']);
});

test('GET /api/vehiculos transmite Authorization y recibe una lista JSON', async () => {
  let solicitud;
  const simularFetch = async (ruta, opciones) => {
    solicitud = { ruta, opciones };
    return new Response(JSON.stringify([{ idVehiculo: 1, marca: 'Ejemplo' }]), {
      status: 200,
      headers: { 'Content-Type': 'application/json' },
    });
  };
  const resultado = await listarVehiculos('mi.token', simularFetch);
  assert.equal(solicitud.ruta, '/api/vehiculos');
  assert.equal(solicitud.opciones.headers.get('Authorization'), 'Bearer mi.token');
  assert.equal(solicitud.opciones.cache, 'no-store');
  assert.equal(resultado.vehiculos.length, 1);
});

test('POST /api/ventas envía Idempotency-Key y cuerpo JSON con vehiculoId y compradorId', async () => {
  let solicitud;
  const simularFetch = async (ruta, opciones) => {
    solicitud = { ruta, opciones };
    return new Response(
      JSON.stringify({
        id: 10,
        operacionId: 'uuid-op-10',
        vehiculoId: 2,
        compradorId: 3,
        estado: 'COMPLETADA',
        notificacion: 'ENVIADA',
      }),
      { status: 201, headers: { 'Content-Type': 'application/json' } },
    );
  };
  const res = await registrarVenta(
    {
      token: 'jwt.operador',
      vehiculoId: 2,
      compradorId: 3,
      idempotencyKey: 'venta-uuid-12345678',
    },
    simularFetch,
  );
  assert.equal(solicitud.ruta, '/api/ventas');
  assert.equal(solicitud.opciones.method, 'POST');
  assert.equal(solicitud.opciones.headers.get('Idempotency-Key'), 'venta-uuid-12345678');
  assert.equal(res.codigoHttp, 201);
  assert.equal(res.venta.id, 10);
});

test('GET /api/ventas/{id} consulta estado individual de la venta', async () => {
  const simularFetch = async (ruta) => {
    assert.equal(ruta, '/api/ventas/10');
    return new Response(
      JSON.stringify({ id: 10, estado: 'COMPLETADA', notificacion: 'ENVIADA' }),
      { status: 200, headers: { 'Content-Type': 'application/json' } },
    );
  };
  const res = await consultarVentaPorId({ token: 'jwt', ventaId: 10 }, simularFetch);
  assert.equal(res.codigoHttp, 200);
  assert.equal(res.venta.id, 10);
});

test('sin token no agrega Authorization (el backend decide 401)', async () => {
  const simularFetch = async (_ruta, opciones) => {
    assert.equal(opciones.headers.has('Authorization'), false);
    return new Response('[]', { status: 200 });
  };
  await listarVehiculos('', simularFetch);
});

test('identifica HTTP 401 sin mostrar una lista inventada', async () => {
  await assert.rejects(
    listarVehiculos('vencido', async () => new Response('No autorizado', { status: 401 })),
    (error) => error.codigoHttp === 401 && /401/.test(error.message),
  );
});

test('rechaza un objeto en lugar del arreglo JSON contratado', async () => {
  await assert.rejects(
    listarVehiculos('jwt', async () => new Response('{"data":[]}', { status: 200 })),
    /arreglo JSON/,
  );
});

test('informa error de conectividad', async () => {
  await assert.rejects(
    listarVehiculos('jwt', async () => {
      throw new Error('sin red');
    }),
    /conectar/,
  );
});
