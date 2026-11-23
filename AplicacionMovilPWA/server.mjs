/**
 * Servidor local de la Aplicación Móvil PWA sin dependencias externas.
 * Sirve la PWA en el puerto 5174 y reenvía /oauth/token y /api/* hacia NGINX (127.0.0.1:8090).
 * La URL pública es del mismo origen para evitar problemas CORS en el navegador móvil.
 */
import { createServer } from 'node:http';
import { readFile, stat } from 'node:fs/promises';
import { resolve, extname, sep } from 'node:path';
import { fileURLToPath } from 'node:url';

const ROOT = resolve(fileURLToPath(new URL('.', import.meta.url)));
const TIPOS = {
  '.html': 'text/html; charset=utf-8',
  '.css': 'text/css; charset=utf-8',
  '.js': 'text/javascript; charset=utf-8',
  '.png': 'image/png',
  '.webmanifest': 'application/manifest+json; charset=utf-8',
  '.json': 'application/json; charset=utf-8',
  '.svg': 'image/svg+xml',
};

function leerCuerpoPeticion(peticion) {
  return new Promise((resolver, rechazar) => {
    const fragmentos = [];
    peticion.on('data', (trozo) => fragmentos.push(trozo));
    peticion.on('end', () => resolver(Buffer.concat(fragmentos)));
    peticion.on('error', rechazar);
  });
}

export function crearServidor({
  directorio = resolve(ROOT, 'public'),
  apiBase = process.env.API_BASE_URL || 'http://127.0.0.1:8090',
} = {}) {
  const carpeta = resolve(directorio);
  const baseLimpia = apiBase.replace(/\/$/, '');

  return createServer(async (peticion, respuesta) => {
    const url = new URL(peticion.url || '/', 'http://localhost');
    respuesta.setHeader('X-Content-Type-Options', 'nosniff');
    respuesta.setHeader('Referrer-Policy', 'no-referrer');
    respuesta.setHeader('Cache-Control', 'no-store');

    const esRutaProxy =
      url.pathname === '/oauth/token' || url.pathname.startsWith('/api/');

    if (esRutaProxy) {
      const metodo = (peticion.method || 'GET').toUpperCase();
      if (metodo !== 'GET' && metodo !== 'POST') {
        respuesta.writeHead(405, { 'Content-Type': 'application/json; charset=utf-8' });
        respuesta.end(JSON.stringify({ error: 'Método HTTP no permitido en el proxy PWA' }));
        return;
      }

      const abortador = new AbortController();
      const temporizador = setTimeout(() => abortador.abort(), 15000);
      try {
        const encabezados = {
          Accept: peticion.headers.accept || 'application/json',
        };
        if (peticion.headers.authorization) {
          encabezados.Authorization = peticion.headers.authorization;
        }
        if (peticion.headers['content-type']) {
          encabezados['Content-Type'] = peticion.headers['content-type'];
        }
        if (peticion.headers['idempotency-key']) {
          encabezados['Idempotency-Key'] = peticion.headers['idempotency-key'];
        }

        const opcionesFetch = {
          method: metodo,
          headers: encabezados,
          signal: abortador.signal,
          cache: 'no-store',
        };

        if (metodo === 'POST') {
          const cuerpoBuffer = await leerCuerpoPeticion(peticion);
          if (cuerpoBuffer.length > 0) {
            opcionesFetch.body = cuerpoBuffer;
          }
        }

        const destino = `${baseLimpia}${url.pathname}${url.search}`;
        const remoto = await fetch(destino, opcionesFetch);

        const cabecerasRespuesta = {
          'Content-Type':
            remoto.headers.get('content-type') || 'application/json; charset=utf-8',
          'Cache-Control': 'no-store',
          Vary: 'Authorization',
        };
        const locationHeader = remoto.headers.get('location');
        if (locationHeader) {
          cabecerasRespuesta.Location = locationHeader;
        }

        respuesta.writeHead(remoto.status, cabecerasRespuesta);
        respuesta.end(Buffer.from(await remoto.arrayBuffer()));
      } catch (error) {
        const tiempoAgotado = error.name === 'AbortError';
        respuesta.writeHead(tiempoAgotado ? 504 : 502, {
          'Content-Type': 'application/json; charset=utf-8',
        });
        respuesta.end(
          JSON.stringify({
            error: tiempoAgotado
              ? 'Tiempo de espera agotado al contactar el Gateway NGINX'
              : 'Gateway NGINX o servicio backend no disponible',
          }),
        );
      } finally {
        clearTimeout(temporizador);
      }
      return;
    }

    if (peticion.method !== 'GET' && peticion.method !== 'HEAD') {
      respuesta.writeHead(405);
      respuesta.end();
      return;
    }

    let ruta;
    try {
      ruta = decodeURIComponent(url.pathname);
    } catch {
      respuesta.writeHead(400);
      respuesta.end();
      return;
    }
    const archivo = resolve(carpeta, '.' + (ruta === '/' ? '/index.html' : ruta));
    if (!(archivo === carpeta || archivo.startsWith(carpeta + sep))) {
      respuesta.writeHead(403);
      respuesta.end();
      return;
    }
    try {
      const detalle = await stat(archivo);
      if (!detalle.isFile()) throw new Error('Archivo no encontrado');
      respuesta.setHeader(
        'Content-Type',
        TIPOS[extname(archivo)] || 'application/octet-stream',
      );
      if (peticion.method === 'HEAD') {
        respuesta.writeHead(200);
        respuesta.end();
        return;
      }
      respuesta.writeHead(200);
      respuesta.end(await readFile(archivo));
    } catch {
      respuesta.writeHead(404, { 'Content-Type': 'text/plain; charset=utf-8' });
      respuesta.end('Recurso no encontrado');
    }
  });
}

// Se habilita uso como módulo para pruebas, sin iniciar puerto al importarlo.
if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const argPuerto = process.argv.find((arg) => arg.startsWith('--port='));
  const argCarpeta = process.argv.find((arg) => arg.startsWith('--dir='));
  const puerto = Number(argPuerto?.split('=')[1] ?? process.env.PORT ?? 5174);
  const host = process.env.HOST || '127.0.0.1';
  const carpeta = resolve(ROOT, argCarpeta?.split('=')[1] || 'public');
  const apiBase = process.env.API_BASE_URL || 'http://127.0.0.1:8090';
  crearServidor({ directorio: carpeta, apiBase }).listen(puerto, host, () => {
    console.log(`PWA disponible: http://localhost:${puerto}`);
    console.log(`Proxy NGINX Gateway: /oauth/token y /api/* -> ${apiBase}`);
    console.log('Para cerrar: Ctrl + C');
  });
}
