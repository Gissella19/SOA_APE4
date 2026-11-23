import test from 'node:test';
import assert from 'node:assert/strict';
import { createServer } from 'node:http';
import { once } from 'node:events';
import { crearServidor } from '../server.mjs';

test('proxy reenvía GET /api/vehiculos y POST /api/ventas con Authorization e Idempotency-Key', async () => {
  let ultimoMetodo = '';
  let ultimaRuta = '';
  let jwt = '';
  let idempotencyKey = '';
  let cuerpoRecibido = '';

  const backend = createServer((req, res) => {
    ultimoMetodo = req.method;
    ultimaRuta = req.url;
    jwt = req.headers.authorization || '';
    idempotencyKey = req.headers['idempotency-key'] || '';
    const trozos = [];
    req.on('data', (c) => trozos.push(c));
    req.on('end', () => {
      cuerpoRecibido = Buffer.concat(trozos).toString('utf8');
      if (req.url === '/api/ventas' && req.method === 'POST') {
        res.writeHead(201, { 'Content-Type': 'application/json' });
        res.end(JSON.stringify({ id: 1, estado: 'COMPLETADA', notificacion: 'ENVIADA' }));
        return;
      }
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end('[]');
    });
  });

  backend.listen(0, '127.0.0.1');
  await once(backend, 'listening');
  const port = backend.address().port;
  const frontend = crearServidor({ apiBase: `http://127.0.0.1:${port}` });
  frontend.listen(0, '127.0.0.1');
  await once(frontend, 'listening');

  try {
    const baseFrontend = `http://127.0.0.1:${frontend.address().port}`;

    const respuestaGet = await fetch(`${baseFrontend}/api/vehiculos`, {
      headers: { Authorization: 'Bearer prueba.jwt' },
    });
    assert.equal(respuestaGet.status, 200);
    assert.equal(ultimoMetodo, 'GET');
    assert.equal(ultimaRuta, '/api/vehiculos');
    assert.equal(jwt, 'Bearer prueba.jwt');
    assert.equal(respuestaGet.headers.get('cache-control'), 'no-store');
    assert.deepEqual(await respuestaGet.json(), []);

    const respuestaPost = await fetch(`${baseFrontend}/api/ventas`, {
      method: 'POST',
      headers: {
        Authorization: 'Bearer operador.jwt',
        'Content-Type': 'application/json',
        'Idempotency-Key': 'clave-prueba-12345',
      },
      body: JSON.stringify({ vehiculoId: 1, compradorId: 2 }),
    });
    assert.equal(respuestaPost.status, 201);
    assert.equal(ultimoMetodo, 'POST');
    assert.equal(ultimaRuta, '/api/ventas');
    assert.equal(jwt, 'Bearer operador.jwt');
    assert.equal(idempotencyKey, 'clave-prueba-12345');
    assert.deepEqual(JSON.parse(cuerpoRecibido), { vehiculoId: 1, compradorId: 2 });
  } finally {
    await new Promise((r) => frontend.close(r));
    await new Promise((r) => backend.close(r));
  }
});
