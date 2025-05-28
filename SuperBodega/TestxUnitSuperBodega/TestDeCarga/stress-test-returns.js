import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Trend, Rate, Counter, Gauge } from 'k6/metrics';

// Métricas personalizadas
const syncRequestTime = new Trend('sync_request_time');
const asyncRequestTime = new Trend('async_request_time');
const syncProcessTime = new Trend('sync_process_time');
const asyncProcessTime = new Trend('async_process_time');
const syncSuccess = new Rate('sync_success');
const asyncSuccess = new Rate('async_success');
const activeUsersGauge = new Gauge('active_users');
const returnRequestedCounter = new Counter('return_requested');
const returnCompletedCounter = new Counter('return_completed');
const errorCounter = new Counter('errors');

// Estados de ventas
const RECIBIDA = 1;
const DESPACHADA = 2;
const ENTREGADA = 3;
const DEVOLUCION_SOLICITADA = 4;
const DEVOLUCION_COMPLETADA = 5;

// Número de ventas de prueba
const NUM_VENTAS = 10;

// Clave compartida para estados de ventas
const ventasKey = 'devolution_states';

// Variable compartida para saber cuándo todas las devoluciones están completas
const allCompletedKey = 'all_returns_completed';

export const options = {
    scenarios: {
        // Prueba con duración de 25 segundos
        returns_test: {
            executor: 'ramping-vus',
            startVUs: 1,
            stages: [
                { duration: '5s', target: 3 },   // Rampa hasta 3 usuarios
                { duration: '15s', target: 3 },  // Mantén 3 usuarios
                { duration: '3s', target: 0 },   // Finaliza
            ],
            gracefulRampDown: '2s',
        },
    },
    thresholds: {
        'sync_request_time': ['p(95)<3000'],
        'async_request_time': ['p(95)<500'],
        'sync_process_time': ['p(95)<3000'],
        'async_process_time': ['p(95)<500'],
        'sync_success': ['rate>0.8'],
        'async_success': ['rate>0.8'],
        'errors': ['count<10'],
    },
};

// Inicializar estados al comienzo de la prueba
function initEstadosVentas() {
    if (!__ENV[ventasKey]) {
        const estadosIniciales = {};
        for (let i = 1; i <= NUM_VENTAS; i++) {
            // Asignar estados iniciales aleatorios: 50% Recibidas, 50% Entregadas
            estadosIniciales[i] = Math.random() < 0.5 ? RECIBIDA : ENTREGADA;
        }
        __ENV[ventasKey] = JSON.stringify(estadosIniciales);
        __ENV[allCompletedKey] = 'false';
    }

    return JSON.parse(__ENV[ventasKey]);
}

// Actualizar estado con bloqueo
function actualizarEstadoVenta(ventaId, nuevoEstado) {
    const estados = JSON.parse(__ENV[ventasKey] || '{}');

    // Solo permitir transiciones válidas en el flujo de devolución
    let transicionValida = false;

    const estadoActual = estados[ventaId];
    if ((estadoActual === RECIBIDA || estadoActual === ENTREGADA) && nuevoEstado === DEVOLUCION_SOLICITADA) {
        transicionValida = true;
    } else if (estadoActual === DEVOLUCION_SOLICITADA && nuevoEstado === DEVOLUCION_COMPLETADA) {
        transicionValida = true;
    }

    if (transicionValida) {
        estados[ventaId] = nuevoEstado;
        __ENV[ventasKey] = JSON.stringify(estados);

        // Verificar si todas las ventas están en estado DEVOLUCION_COMPLETADA
        const todasCompletas = Object.values(estados).every(estado => estado === DEVOLUCION_COMPLETADA);
        if (todasCompletas) {
            __ENV[allCompletedKey] = 'true';
            console.log("¡TODAS LAS DEVOLUCIONES COMPLETADAS! Terminando pruebas...");
        }

        return true;
    }

    return false;
}

// Verificar si todas las ventas están completas
function todasVentasCompletas() {
    return __ENV[allCompletedKey] === 'true';
}

// Solicitar devolución
function solicitarDevolucion(ventaId, usarModoSincrono) {
    console.log(`${usarModoSincrono ? "SYNC" : "ASYNC"}: Solicitando devolución para venta ${ventaId}`);

    const payload = JSON.stringify({
        IdEstadoDeLaVenta: DEVOLUCION_SOLICITADA,
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
        syncRequestTime.add(duration);
    } else {
        asyncRequestTime.add(duration);
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
        console.log(`${usarModoSincrono ? "SYNC" : "ASYNC"}: Devolución solicitada para Venta ${ventaId} - duración: ${duration}ms`);
        actualizarEstadoVenta(ventaId, DEVOLUCION_SOLICITADA);
        returnRequestedCounter.add(1);
        return true;
    } else {
        errorCounter.add(1);
        console.log(`Error al solicitar devolución: ${response.status}, ${response.body}`);
        return false;
    }
}

// Procesar devolución
function procesarDevolucion(ventaId, usarModoSincrono) {
    console.log(`${usarModoSincrono ? "SYNC" : "ASYNC"}: Procesando devolución para venta ${ventaId}`);

    const payload = JSON.stringify({
        usarNotificacionSincronica: usarModoSincrono
    });

    const params = {
        headers: { 'Content-Type': 'application/json' },
        timeout: usarModoSincrono ? '10s' : '5s',
    };

    const start = new Date();
    const response = http.post(
        `http://localhost:8080/api/Venta/devolucion/${ventaId}`,
        payload,
        params
    );
    const duration = new Date() - start;

    if (usarModoSincrono) {
        syncProcessTime.add(duration);
    } else {
        asyncProcessTime.add(duration);
    }

    const success = check(response, {
        'estado 200': (r) => r.status === 200
    });

    if (usarModoSincrono) {
        syncSuccess.add(success);
    } else {
        asyncSuccess.add(success);
    }

    if (success) {
        console.log(`${usarModoSincrono ? "SYNC" : "ASYNC"}: Devolución procesada para Venta ${ventaId} - duración: ${duration}ms`);
        actualizarEstadoVenta(ventaId, DEVOLUCION_COMPLETADA);
        returnCompletedCounter.add(1);
        console.log(`Venta ${ventaId} completada exitosamente (Devolución Completada)`);
        return true;
    } else {
        errorCounter.add(1);
        console.log(`Error al procesar devolución: ${response.status}, ${response.body}`);
        return false;
    }
}

export default function() {
    activeUsersGauge.add(1);

    // Inicializar estados al principio
    const estadosVentas = initEstadosVentas();

    if (todasVentasCompletas()) {
        console.log("Todas las devoluciones están completas. No hay más acciones disponibles.");
        sleep(0.5);
        activeUsersGauge.add(0);
        return;
    }

    // Determinar modo (50% sincrónico, 50% asincrónico)
    const usarModoSincrono = Math.random() < 0.5;

    group(usarModoSincrono ? 'Devolución Sincrónica' : 'Devolución Asincrónica', () => {
        // Paso 1: Buscar ventas que puedan solicitar devolución
        const ventasParaSolicitar = Object.entries(estadosVentas)
            .filter(([_, estado]) => estado === RECIBIDA || estado === ENTREGADA)
            .map(([id, _]) => parseInt(id));

        // Paso 2: Buscar ventas que ya tengan devolución solicitada y puedan ser procesadas
        const ventasParaProcesar = Object.entries(estadosVentas)
            .filter(([_, estado]) => estado === DEVOLUCION_SOLICITADA)
            .map(([id, _]) => parseInt(id));

        // No hay ventas disponibles para ninguna acción
        if (ventasParaSolicitar.length === 0 && ventasParaProcesar.length === 0) {
            console.log("No hay ventas disponibles para devoluciones");
            if (Object.keys(estadosVentas).length > 0) {
                __ENV[allCompletedKey] = 'true';
            }
            sleep(0.5);
            activeUsersGauge.add(0);
            return;
        }

        // Priorizar procesamiento de devoluciones ya solicitadas
        if (ventasParaProcesar.length > 0) {
            const ventaId = ventasParaProcesar[Math.floor(Math.random() * ventasParaProcesar.length)];
            procesarDevolucion(ventaId, usarModoSincrono);
        }
        // Si no hay devoluciones para procesar, solicitar nuevas
        else if (ventasParaSolicitar.length > 0) {
            const ventaId = ventasParaSolicitar[Math.floor(Math.random() * ventasParaSolicitar.length)];
            solicitarDevolucion(ventaId, usarModoSincrono);
        }

        sleep(usarModoSincrono ? 2 : 0.5);
    });

    activeUsersGauge.add(0);
}