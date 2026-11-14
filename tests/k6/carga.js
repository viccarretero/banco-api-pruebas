// =====================================================================
// Prueba de carga de Banco API con k6
// Simula hasta 20 usuarios virtuales haciendo operaciones bancarias.
// =====================================================================
import http from 'k6/http';
import { check, group, sleep } from 'k6';

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000';
const JSON_HEADERS = { headers: { 'Content-Type': 'application/json' } };

export const options = {
  stages: [
    { duration: '30s', target: 20 }, // subida: de 0 a 20 usuarios
    { duration: '1m', target: 20 },  // carga sostenida: 20 usuarios
    { duration: '15s', target: 0 },  // bajada
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // el 95 % de las peticiones en menos de 500 ms
    http_req_failed: ['rate<0.01'],   // menos del 1 % de errores
    checks: ['rate>0.99'],            // más del 99 % de validaciones correctas
  },
};

export default function () {
  group('Consultar cuentas', () => {
    const lista = http.get(`${BASE_URL}/api/cuentas`);
    check(lista, {
      'listar: status 200': (r) => r.status === 200,
      'listar: devuelve un arreglo': (r) => Array.isArray(r.json()),
    });

    const cuenta = http.get(`${BASE_URL}/api/cuentas/1`);
    check(cuenta, {
      'obtener: status 200': (r) => r.status === 200,
      'obtener: titular correcto': (r) => r.json('titular') === 'Ana Torres',
    });
  });

  group('Operaciones', () => {
    const nueva = http.post(
      `${BASE_URL}/api/cuentas`,
      JSON.stringify({ titular: `Usuario ${__VU}-${__ITER}`, saldoInicial: 100 }),
      JSON_HEADERS,
    );
    check(nueva, { 'crear: status 201': (r) => r.status === 201 });
    const id = nueva.json('id');

    const deposito = http.post(`${BASE_URL}/api/cuentas/${id}/depositos`, JSON.stringify({ monto: 50 }), JSON_HEADERS);
    check(deposito, {
      'depositar: status 200': (r) => r.status === 200,
      'depositar: saldo 150': (r) => r.json('saldo') === 150,
    });

    const retiro = http.post(`${BASE_URL}/api/cuentas/${id}/retiros`, JSON.stringify({ monto: 30 }), JSON_HEADERS);
    check(retiro, {
      'retirar: status 200': (r) => r.status === 200,
      'retirar: saldo 120': (r) => r.json('saldo') === 120,
    });
  });

  sleep(1);
}
