import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Trend, Rate, Counter, Gauge } from 'k6/metrics';

// Métricas personalizadas
const syncResponseTime = new Trend('sync_purchase_time');
const asyncResponseTime = new Trend('async_purchase_time');
const syncSuccess = new Rate('sync_success');
const asyncSuccess = new Rate('async_success');
const activeUsersGauge = new Gauge('active_users');
const purchasesCounter = new Counter('purchases_completed');
const errorCounter = new Counter('errors');

// Clientes disponibles para pruebas
const clientIds = [1, 2, 3, 4, 5];

export const options = {
    scenarios: {
        purchase_test: {
            executor: 'ramping-vus',
            startVUs: 1,
            stages: [
                { duration: '5s', target: 5 },   // Rampa hasta 5 usuarios
                { duration: '15s', target: 5 },  // Mantén 5 usuarios
                { duration: '3s', target: 0 },   // Finaliza
            ],
            gracefulRampDown: '2s',
        },
    },
    thresholds: {
        'sync_purchase_time': ['p(95)<3000'],
        'async_purchase_time': ['p(95)<500'],
        'sync_success': ['rate>0.8'],
        'async_success': ['rate>0.8'],
        'errors': ['count<10'],
    },
};

// Función para generar un número de factura único
function generarNumeroFactura() {
    // Generar una letra aleatoria (A-Z)
    const letras = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const letra = letras.charAt(Math.floor(Math.random() * letras.length));

    // Generar 3 dígitos aleatorios (000-999)
    const digitos = Math.floor(Math.random() * 1000).toString().padStart(3, '0');

    // Combinar en el formato requerido
    return `${letra}-${digitos}`;
}

// Función para generar detalles aleatorios de compra
function generarDetallesCompra() {
    // Productos de ejemplo para las pruebas con sus proveedores
    const productosDisponibles = [
        { id: 1, nombre: "Laptop", precio: 999.99, proveedorId: 1, nombreDelProveedor: "TechSupplier" },    // Proveedor con ID 1
        { id: 2, nombre: "Smartphone", precio: 499.99, proveedorId: 2, nombreDelProveedor: "MobileWorld" }, // Proveedor con ID 2
        { id: 3, nombre: "Audífonos", precio: 59.99, proveedorId: 3, nombreDelProveedor: "AudioTech" }    // Proveedor con ID 3
    ];

    // Generar entre 1 y 3 productos para la compra (usando solo IDs existentes)
    const cantidadProductos = Math.floor(Math.random() * 2) + 1;
    const detalles = [];
    let total = 0;

    for (let i = 0; i < cantidadProductos; i++) {
        const producto = productosDisponibles[Math.floor(Math.random() * productosDisponibles.length)];
        const cantidad = Math.floor(Math.random() * 2) + 1;
        const subtotal = producto.precio * cantidad;
        total += subtotal;

        detalles.push({
            idProducto: producto.id,
            nombreDelProducto: producto.nombre,
            idProveedor: producto.proveedorId,
            nombreDelProveedor: producto.nombreDelProveedor,
            cantidad: cantidad,
            precioDeVenta: producto.precio,
            montoTotal: subtotal
        });
    }

    return { detalles, total };
}

// Realizar una compra
function realizarCompra(usarModoSincrono) {
    // Seleccionar un cliente aleatorio
    const clienteId = clientIds[Math.floor(Math.random() * clientIds.length)];

    // Generar datos de la compra
    const { detalles, total } = generarDetallesCompra();
    const numeroFactura = generarNumeroFactura();
    const montoPago = total + 50; // Monto fijo adicional para cambio
    const montoCambio = montoPago - total;

    console.log(`${usarModoSincrono ? "SYNC" : "ASYNC"}: Realizando compra para cliente ${clienteId}, factura ${numeroFactura}`);

    // Construir payload para la creación de venta
    const payload = JSON.stringify({
        NumeroDeFactura: numeroFactura,
        IdCliente: clienteId,
        MontoDePago: montoPago,
        MontoDeCambio: montoCambio,
        MontoTotal: total,
        detalles: detalles, 
        UsarNotificacionSincronica: usarModoSincrono
    });

    const params = {
        headers: { 'Content-Type': 'application/json' },
        timeout: usarModoSincrono ? '10s' : '5s',
    };

    const start = new Date();
    const response = http.post(
        `http://localhost:8080/api/Venta/Create`,
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
        'status es 201 Created': (r) => r.status === 201,
        'contiene ID de venta': (r) => {
            try {
                return r.json().hasOwnProperty('id');
            } catch (e) {
                console.log(`Error al verificar respuesta: ${r.body}`);
                return false;
            }
        }
    });

    if (usarModoSincrono) {
        syncSuccess.add(success);
    } else {
        asyncSuccess.add(success);
    }

    if (success) {
        console.log(`${usarModoSincrono ? "SYNC" : "ASYNC"}: Compra exitosa para cliente ${clienteId} - duración: ${duration}ms`);
        purchasesCounter.add(1);
        return true;
    } else {
        errorCounter.add(1);
        console.log(`Error al realizar compra: ${response.status}, ${response.body}`);
        return false;
    }
}

export default function() {
    activeUsersGauge.add(1);

    // Determinar modo (20% sincrónico, 80% asincrónico para representar uso real)
    const usarModoSincrono = Math.random() < 0.2;

    group(usarModoSincrono ? 'Compra Sincrónica' : 'Compra Asincrónica', () => {
        realizarCompra(usarModoSincrono);

        // Tiempo entre acciones del usuario
        sleep(usarModoSincrono ? 2 : 0.5);
    });

    activeUsersGauge.add(0);
}