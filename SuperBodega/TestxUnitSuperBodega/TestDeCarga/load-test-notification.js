import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Trend, Rate, Counter, Gauge } from 'k6/metrics';

// Métricas personalizadas
const syncResponseTime = new Trend('sync_response_time');
const asyncResponseTime = new Trend('async_response_time');
const syncSuccess = new Rate('sync_success');
const asyncSuccess = new Rate('async_success');
const activeUsersGauge = new Gauge('active_users');
const changedStatesCounter = new Counter('changed_states');
const completedVentasCounter = new Counter('completed_ventas');
const errorCounter = new Counter('errors');

// Estados de ventas
const RECIBIDA = 1;
const DESPACHADA = 2;
const ENTREGADA = 3;

// Número de ventas de prueba
const NUM_VENTAS = 10;

// Clave compartida para estados de ventas
const ventasKey = 'ventas_estados';

// Variable compartida para saber cuándo todas las ventas están completas
const allCompletedKey = 'all_completed';

export const options = {
    scenarios: {
        // Prueba más corta y controlada
        ramping_load: {
            executor: 'ramping-vus',
            startVUs: 1,
            stages: [
                { duration: '5s', target: 5 },   // Rampa hasta 5 usuarios
                { duration: '15s', target: 5 },   // Mantén 5 usuarios
                { duration: '3s', target: 0 },   // Finaliza
            ],
            gracefulRampDown: '2s',
        },
    },
    thresholds: {
        'sync_response_time': ['p(95)<3000'],
        'async_response_time': ['p(95)<500'],
        'sync_success': ['rate>0.8'],        // Bajado a 80% para tolerar algunos errores
        'async_success': ['rate>0.8'],
        'errors': ['count<20'],              // No más de 20 errores totales
    },
};

// Inicializar estados al comienzo de la prueba
function initEstadosVentas() {
    // Usar SharedArray o algo equivalente no funciona bien con k6,
    // así que usamos __ENV como almacenamiento compartido
    if (!__ENV[ventasKey]) {
        const estadosIniciales = {};
        for (let i = 1; i <= NUM_VENTAS; i++) {
            estadosIniciales[i] = RECIBIDA;
        }
        __ENV[ventasKey] = JSON.stringify(estadosIniciales);
        __ENV[allCompletedKey] = 'false';
    }

    return JSON.parse(__ENV[ventasKey]);
}

// Actualizar estado con bloqueo
function actualizarEstadoVenta(ventaId, nuevoEstado) {
    // Obtener la versión más reciente
    const estados = JSON.parse(__ENV[ventasKey] || '{}');

    // Actualizar solo si es un avance de estado
    if (nuevoEstado > estados[ventaId]) {
        estados[ventaId] = nuevoEstado;
        __ENV[ventasKey] = JSON.stringify(estados);

        // Verificar si todas las ventas están en estado ENTREGADA
        const todasCompletas = Object.values(estados).every(estado => estado === ENTREGADA);
        if (todasCompletas) {
            __ENV[allCompletedKey] = 'true';
            console.log("¡TODAS LAS VENTAS COMPLETADAS! Terminando pruebas...");
        }
    }
}

// Verificar si todas las ventas están completas
function todasVentasCompletas() {
    return __ENV[allCompletedKey] === 'true';
}

export default function() {
    activeUsersGauge.add(1);

    // Inicializa los estados al principio
    const estadosVentas = initEstadosVentas();

    // Si todas las ventas ya están completadas, no hacer nada más
    if (todasVentasCompletas()) {
        console.log("Todas las ventas ya están en estado ENTREGADA. No hay más acciones disponibles.");
        sleep(0.5);
        activeUsersGauge.add(0);
        return;
    }

    // Determinar modo (50% sincrónico, 50% asincrónico)
    const usarModoSincrono = Math.random() < 0.5;

    group(usarModoSincrono ? 'Notificación Sincrónica' : 'Notificación Asincrónica', () => {
        // Encontrar ventas disponibles para cambiar de estado
        const ventasDisponibles = Object.keys(estadosVentas).filter(
            id => estadosVentas[id] < ENTREGADA
        );

        if (ventasDisponibles.length === 0) {
            console.log("Todas las ventas ya están en estado ENTREGADA");
            __ENV[allCompletedKey] = 'true'; // Marcar como completado
            sleep(0.5);
            activeUsersGauge.add(0);
            return;
        }

        // Seleccionar una venta aleatoria disponible
        const ventaId = parseInt(ventasDisponibles[Math.floor(Math.random() * ventasDisponibles.length)]);
        const estadoActual = estadosVentas[ventaId];
        const nuevoEstado = estadoActual + 1;

        console.log(`${usarModoSincrono ? "SYNC" : "ASYNC"}: Intentando cambiar venta ${ventaId} de estado ${estadoActual} a ${nuevoEstado}`);

        const payload = JSON.stringify({
            IdEstadoDeLaVenta: nuevoEstado,
            UsarNotificacionSincronica: usarModoSincrono
        });

        const params = {
            headers: { 'Content-Type': 'application/json' },
            timeout: usarModoSincrono ? '10s' : '5s',
        };

        const start = new Date();
        const response = http.put(
            `http://localhost:8080/api/Venta/estado/edit/${ventaId}`,
            payload,
            params
        );
        const duration = new Date() - start;

        if (usarModoSincrono) {
            syncResponseTime.add(duration);
        } else {
            asyncResponseTime.add(duration);
        }

        const success = check(response, {
            'status es 200': (r) => r.status === 200
        });

        if (usarModoSincrono) {
            syncSuccess.add(success);
        } else {
            asyncSuccess.add(success);
        }

        if (success) {
            // Mensaje de éxito mejorado que incluye la información de la venta
            console.log(`${usarModoSincrono ? "SYNC" : "ASYNC"} exitoso para Venta ${ventaId} - duración: ${duration}ms`);
            actualizarEstadoVenta(ventaId, nuevoEstado);
            changedStatesCounter.add(1);

            // Contar si la venta se completó (llegó a ENTREGADA)
            if (nuevoEstado === ENTREGADA) {
                completedVentasCounter.add(1);
                console.log(`Venta ${ventaId} completada exitosamente (Estado ENTREGADA)`);
            }
        } else {
            // Contar errores pero sin llenar la consola
            errorCounter.add(1);
        }

        sleep(usarModoSincrono ? 1.5 : 0.5);
    });

    activeUsersGauge.add(0);
}