import http from 'k6/http';
import { check, sleep } from 'k6';
import { Counter } from 'k6/metrics';

const rateLimitDisparado = new Counter('rate_limit_disparado');

// CONFIGURACIÓN

const BASE_URL = __ENV.API_BASE_URL || 'http://localhost:5123/api';
const NUM_USUARIOS = parseInt(__ENV.USUARIOS_PRUEBA || '10', 10);
const CLAVE_PRUEBA = 'StressTest.2026!';
// Sobrescrituras manuales (por si el descubrimiento automático del catálogo falla)
const ID_EVENTO_LOCALIDAD = __ENV.EVENTO_LOCALIDAD_ID;
const ID_MEDIO_PAGO = __ENV.MEDIO_PAGO_ID;
const ID_SEDE = __ENV.SEDE_ID;

const RESPUESTAS_COMPRA_OK = http.expectedStatuses(200, 201, 429);

export const options = {
  stages: [
    { duration: '10s', target: 10 }, // Sube a 10 usuarios concurrentes
    { duration: '20s', target: 10 }, // Mantiene la carga
    { duration: '10s', target: 0 },  // Baja la carga
  ],
  thresholds: {
    http_req_failed: ['rate<0.01'],
    'http_req_duration{name:GET /Eventos}': ['p(95)<500'],
    'http_req_duration{name:GET /Eventos/{id}}': ['p(95)<500'],
    'http_req_duration{name:GET /Eventos/localidad-sede}': ['p(95)<500'],
    'http_req_duration{name:GET /Eventos/mis-eventos}': ['p(95)<500'],
    'http_req_duration{name:GET /EventosLocalidades}': ['p(95)<500'],
    'http_req_duration{name:GET /MediosPago}': ['p(95)<500'],
    'http_req_duration{name:GET /Sedes}': ['p(95)<500'],
    'http_req_duration{name:POST /Auth/login}': ['p(95)<1500'],
    'http_req_duration{name:POST /Boletos/comprar}': ['p(95)<3500'],
    'http_req_duration{name:POST /Eventos/crear}': ['p(95)<1500'],
  },
  discardResponseBodies: true,
};


export function setup() {
  const usuarios = [];
  const sufijo = Date.now();

  for (let i = 1; i <= NUM_USUARIOS; i++) {
    const correo = `k6.stress.${sufijo}.${i}@prueba.com`;
    const res = http.post(
      `${BASE_URL}/Usuarios/crearUsuario`,
      JSON.stringify({
        nombre: 'K6',
        apellido: `Stress${i}`,
        telefono: '8888-0000',
        correo: correo,
        contrasena: CLAVE_PRUEBA,
      }),
      { headers: { 'Content-Type': 'application/json' }, responseType: 'text' }
    );

    if (res.status === 201) {
      usuarios.push({ correo, idUsuario: res.json('idUsuario') });
    } else if (res.status === 400) {
      usuarios.push({ correo, idUsuario: null });
    } else {
      console.warn(`No se pudo crear el usuario ${correo} (status ${res.status}): ${res.body}`);
      usuarios.push({ correo, idUsuario: null });
    }
  }

  const ids = {
    eventoId: null,
    eventoLocalidadId: ID_EVENTO_LOCALIDAD || null,
    medioPagoId: ID_MEDIO_PAGO || null,
    sedeId: ID_SEDE || null,
  };

  const resEventos = http.get(`${BASE_URL}/Eventos`, { responseType: 'text' });
  if (resEventos.status === 200) {
    const eventos = resEventos.json();
    if (eventos && eventos.length > 0) {
      ids.eventoId = eventos[1].idEvento;
    }
  }

  if (!ids.eventoLocalidadId) {
    const resLocalidades = http.get(`${BASE_URL}/EventosLocalidades`, { responseType: 'text' });
    if (resLocalidades.status === 200) {
      const localidades = resLocalidades.json();
      if (localidades && localidades.length > 0) {
        ids.eventoLocalidadId = localidades[2].idEventoLocalidad;
      }
    }
  }

  if (!ids.medioPagoId) {
    const resMedios = http.get(`${BASE_URL}/MediosPago`, { responseType: 'text' });
    if (resMedios.status === 200) {
      const medios = resMedios.json();
      ids.medioPagoId = medios && medios.length > 0 ? medios[0].idMedioPago : null;
    }
  }

  if (!ids.sedeId) {
    const resSedes = http.get(`${BASE_URL}/Sedes`, { responseType: 'text' });
    if (resSedes.status === 200) {
      const sedes = resSedes.json();
      ids.sedeId = sedes && sedes.length > 0 ? sedes[0].idSedeEvento : null;
    }
  }

  console.log(
    `Catálogo → evento:${ids.eventoId} eventoLocalidad:${ids.eventoLocalidadId} ` +
    `medioPago:${ids.medioPagoId} sede:${ids.sedeId}`
  );

  if (!ids.eventoLocalidadId || !ids.medioPagoId || !ids.sedeId) {
    console.warn('Descubrimiento parcial del catálogo. Usa EVENTO_LOCALIDAD_ID / MEDIO_PAGO_ID / SEDE_ID si hace falta.');
  }

  return { usuarios, ...ids };
}

const sesiones = {};

export default function (data) {

  // 1. LOGIN: cada VU se autentica con su propio usuario (una sola vez)
  if (!sesiones[__VU]) {
    const usuario = data.usuarios[(__VU - 1) % data.usuarios.length];
    const resLogin = http.post(
      `${BASE_URL}/Auth/login`,
      JSON.stringify({ correo: usuario.correo, contrasena: CLAVE_PRUEBA }),
      { headers: { 'Content-Type': 'application/json' }, responseType: 'text', tags: { name: 'POST /Auth/login' } }
    );

    check(resLogin, {
      'login exitoso (status 200)': (r) => r.status === 200,
    });

    if (resLogin.status === 200) {
      sesiones[__VU] = { token: resLogin.json('token'), idUsuario: resLogin.json('idUsuario') };
    } else {
      console.error(`login → ${resLogin.status}: ${resLogin.body}`);
      sesiones[__VU] = { token: '', idUsuario: null };
    }
  }

  const { token, idUsuario } = sesiones[__VU];
  const getParams = { headers: { Authorization: `Bearer ${token}` } };

  // 2. LECTURAS DEL CATÁLOGO 

  const resEventos = http.get(`${BASE_URL}/Eventos`, { ...getParams, tags: { name: 'GET /Eventos' } });
  check(resEventos, { 'GET /Eventos (status 200)': (r) => r.status === 200 });

  if (data.eventoId) {
    const resDetalle = http.get(`${BASE_URL}/Eventos/${data.eventoId}`, { ...getParams, tags: { name: 'GET /Eventos/{id}' } });
    check(resDetalle, { 'GET /Eventos/{id} (status 200)': (r) => r.status === 200 });
  }

  const resLocalidadSede = http.get(`${BASE_URL}/Eventos/localidad-sede`, { ...getParams, tags: { name: 'GET /Eventos/localidad-sede' } });
  check(resLocalidadSede, { 'GET /Eventos/localidad-sede (status 200)': (r) => r.status === 200 });

  http.get(`${BASE_URL}/EventosLocalidades`, { ...getParams, tags: { name: 'GET /EventosLocalidades' } });
  http.get(`${BASE_URL}/MediosPago`, { ...getParams, tags: { name: 'GET /MediosPago' } });
  http.get(`${BASE_URL}/Sedes`, { ...getParams, tags: { name: 'GET /Sedes' } });

  if (idUsuario) {
    const resMisEventos = http.get(`${BASE_URL}/Eventos/mis-eventos?usuarioId=${idUsuario}`, { ...getParams, tags: { name: 'GET /Eventos/mis-eventos' } });
    check(resMisEventos, { 'GET /mis-eventos (status 200)': (r) => r.status === 200 });
  }

  // 3. COMPRA DE BOLETOS (1 de cada 5 iteraciones)

  if (token && data.eventoLocalidadId && data.medioPagoId && __ITER % 5 === 0) {
    const resComprar = http.post(
      `${BASE_URL}/Boletos/comprar`,
      JSON.stringify({
        idEventoLocalidad: data.eventoLocalidadId,
        idUsuario: idUsuario,
        idMedioPago: data.medioPagoId,
        cantidad: 1,
      }),
      {
        headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
        responseType: 'text',
        responseCallback: RESPUESTAS_COMPRA_OK,
        tags: { name: 'POST /Boletos/comprar' },
      }
    );

    if (resComprar.status === 429) {
      rateLimitDisparado.add(1);
    }

    const compraOk = check(resComprar, {
      'compra aceptada (200/201) o limitada por rate limiter (429)': (r) =>
        r.status === 200 || r.status === 201 || r.status === 429,
    });

    if (!compraOk) {
      console.error(`comprar → ${resComprar.status}: ${resComprar.body}`);
    }
  }


  // 4. CREACIÓN DE EVENTOS (1 de cada 15 iteraciones, [FromForm])
  if (__ITER % 15 === 0 && data.sedeId) {
    const resCrear = http.post(
      `${BASE_URL}/Eventos/crear`,
      {
        nombreEvento: `Evento K6 stress ${__VU}-${__ITER}-${Date.now()}`,
        fechaEvento: '2026-12-01T20:00:00',
        horaEvento: '20:00:00',
        idSede: String(data.sedeId),
      },
      {
        responseType: 'text', 
        tags: { name: 'POST /Eventos/crear' },
      }
    );

    const crearOk = check(resCrear, {
      'POST /Eventos/crear (status 200)': (r) => r.status === 200,
    });

    if (!crearOk) {
      console.error(`crear → ${resCrear.status}: ${resCrear.body}`);
    }
  }


  sleep(1);
}