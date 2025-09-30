import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate, Trend } from 'k6/metrics';

// Métricas personalizadas
const errorRate = new Rate('errors');
const responseTimeTrend = new Trend('response_time');

// Configuración de opciones
export let options = {
  vus: 200, // Usuarios virtuales
  duration: '5m', // Duración de la prueba

  // Umbrales de aceptación según especificaciones del Día 27
  thresholds: {
    'errors': ['rate<0.02'], // Error rate < 2%
    'http_req_duration': ['p(95)<500'], // P95 < 500ms
    'response_time': ['p(95)<500'], // P95 personalizado < 500ms
  },

  // Etapas de carga progresiva
  stages: [
    { duration: '1m', target: 50 },   // Ramp up a 50 usuarios
    { duration: '3m', target: 200 },  // Carga sostenida de 200 usuarios
    { duration: '1m', target: 0 },    // Ramp down a 0 usuarios
  ],
};

// Configuración de entorno
const BASE_URL = __ENV.BASE_URL || 'https://staging.monitorimpresoras.com';
const API_TOKEN = __ENV.API_TOKEN || 'test_token';

// Función de configuración antes de la prueba
export function setup() {
  console.log(`🚀 Iniciando pruebas de carga contra: ${BASE_URL}`);
  console.log(`👥 Usuarios: ${options.vus}, Duración: ${options.duration}`);

  // Verificación inicial de health check
  const healthResponse = http.get(`${BASE_URL}/api/health`);
  check(healthResponse, {
    'Health check inicial exitoso': (r) => r.status === 200,
  });

  return { timestamp: new Date().toISOString() };
}

// Función principal de la prueba
export default function (data) {
  // 1. Test de carga de página principal (simulado)
  const dashboardResponse = http.get(`${BASE_URL}/dashboard`, {
    headers: {
      'Authorization': `Bearer ${API_TOKEN}`,
      'Content-Type': 'application/json',
    },
  });

  const dashboardResult = check(dashboardResponse, {
    'Dashboard carga correctamente': (r) => r.status === 200,
    'Dashboard responde en < 1000ms': (r) => r.timings.duration < 1000,
  });

  if (!dashboardResult) {
    errorRate.add(1);
  } else {
    responseTimeTrend.add(dashboardResponse.timings.duration);
  }

  sleep(0.1); // Pequeña pausa entre requests

  // 2. Test de API crítica - Obtener lista de impresoras
  const printersResponse = http.get(`${BASE_URL}/api/printers`, {
    headers: {
      'Authorization': `Bearer ${API_TOKEN}`,
      'Content-Type': 'application/json',
    },
  });

  const printersResult = check(printersResponse, {
    'API /printers responde 200': (r) => r.status === 200,
    'API /printers responde en < 500ms': (r) => r.timings.duration < 500,
    'API /printers retorna datos válidos': (r) => {
      try {
        const data = JSON.parse(r.body);
        return data && Array.isArray(data);
      } catch (e) {
        return false;
      }
    },
  });

  if (!printersResult) {
    errorRate.add(1);
  } else {
    responseTimeTrend.add(printersResponse.timings.duration);
  }

  sleep(0.1);

  // 3. Test de endpoint de métricas (si existe)
  const metricsResponse = http.get(`${BASE_URL}/api/telemetry/dashboard`, {
    headers: {
      'Authorization': `Bearer ${API_TOKEN}`,
      'Content-Type': 'application/json',
    },
  });

  const metricsResult = check(metricsResponse, {
    'API /telemetry/dashboard responde 200': (r) => r.status === 200,
    'API /telemetry/dashboard responde en < 800ms': (r) => r.timings.duration < 800,
  });

  if (!metricsResult) {
    errorRate.add(1);
  } else {
    responseTimeTrend.add(metricsResponse.timings.duration);
  }

  sleep(0.1);

  // 4. Test de health check (operación ligera)
  const healthResponse = http.get(`${BASE_URL}/api/health`);

  check(healthResponse, {
    'Health check responde correctamente': (r) => r.status === 200,
  });

  sleep(0.05); // Pausa más corta para operación ligera
}

// Función de finalización después de la prueba
export function teardown(data) {
  console.log(`✅ Pruebas de carga completadas a las: ${new Date().toISOString()}`);
  console.log(`📊 Error rate: ${errorRate.value * 100}%`);
  console.log(`⏱️ Tiempo de respuesta promedio: ${responseTimeTrend.avg}ms`);
  console.log(`📈 Tiempo de respuesta P95: ${responseTimeTrend.p(95)}ms`);
}

// Función auxiliar para generar datos de prueba
export function generateTestData() {
  return {
    timestamp: new Date().toISOString(),
    userAgent: 'k6-load-test',
    sessionId: `session_${Math.random().toString(36).substr(2, 9)}`,
  };
}
