document.addEventListener('DOMContentLoaded', function() {
    // Añadir logs a la función cargarNotificaciones para depuración
    const originalFn = cargarNotificaciones;
    cargarNotificaciones = function(clienteId) {
        console.log("Solicitando cargar notificaciones para cliente:", clienteId);
        return originalFn(clienteId);
    };
    // Cargar clientes si estamos en la vista de notificaciones
    const clienteSelect = document.getElementById('clienteSelect');
    if (clienteSelect) {
        // Cargar lista de clientes
        fetch('/api/Cliente')
            .then(response => response.json())
            .then(clientes => {
                clienteSelect.innerHTML = '<option value="">Seleccione un cliente</option>';

                clientes.forEach(cliente => {
                    const option = document.createElement('option');
                    option.value = cliente.id;
                    option.textContent = `${cliente.nombre || ''} ${cliente.apellido || ''}`.trim();
                    clienteSelect.appendChild(option);
                });

                // Intentar seleccionar el último cliente seleccionado (si existe en localStorage)
                const lastClienteId = localStorage.getItem('lastSelectedClienteId');
                if (lastClienteId) {
                    clienteSelect.value = lastClienteId;
                }

                // Cargar notificaciones para el cliente seleccionado (si hay alguno)
                if (clienteSelect.value) {
                    cargarNotificaciones(clienteSelect.value);
                }
            })
            .catch(error => {
                console.error('Error al cargar clientes:', error);
                mostrarMensajeError('No se pudieron cargar los clientes');
            });

        // Configurar evento de cambio de cliente
        clienteSelect.addEventListener('change', function() {
            const clienteId = this.value;
            if (clienteId) {
                // Guardar el ID del cliente seleccionado para futuros usos
                localStorage.setItem('lastSelectedClienteId', clienteId);
                cargarNotificaciones(clienteId);
            } else {
                // Si no hay cliente seleccionado, limpiar las notificaciones
                const container = document.getElementById('notificaciones-container');
                if (container) {
                    container.innerHTML = `
                        <div class="text-center py-4">
                            <i class="bi bi-person-x fs-1 text-muted"></i>
                            <p class="mt-2">Seleccione un cliente para ver sus notificaciones</p>
                        </div>
                    `;
                }
            }
        });
    } else {
        const clienteId = obtenerClienteId();
        actualizarContadorNotificaciones(clienteId);
    }

    // Configurar botón de actualizar
    const refreshBtn = document.getElementById('refreshNotificaciones');
    if (refreshBtn) {
        refreshBtn.addEventListener('click', () => {
            const clienteId = obtenerClienteId();
            if (clienteId) {
                cargarNotificaciones(clienteId);
            } else {
                mostrarMensajeError('Seleccione un cliente primero');
            }
        });
    }
});

// Función para obtener el ID del cliente actual
function obtenerClienteId() {
    // Log para seguimiento
    console.log("Obteniendo ID de cliente...");
    console.log("localStorage items:",
        "lastClienteId=", localStorage.getItem('lastClienteId'),
        "lastSelectedClienteId=", localStorage.getItem('lastSelectedClienteId'));

    // Verificar primero el selector visible
    const clienteSelect = document.getElementById('clienteSelect');
    if (clienteSelect && clienteSelect.value) {
        console.log("Usando ID desde selector:", clienteSelect.value);

        // IMPORTANTE: Guardar en ambas variables para consistencia
        localStorage.setItem('lastClienteId', clienteSelect.value);
        localStorage.setItem('lastSelectedClienteId', clienteSelect.value);

        return clienteSelect.value;
    }

    // Verificar almacenamiento local
    const lastClienteId = localStorage.getItem('lastClienteId');
    if (lastClienteId) {
        console.log("Usando ID desde lastClienteId:", lastClienteId);
        return lastClienteId;
    }

    const lastSelectedClienteId = localStorage.getItem('lastSelectedClienteId');
    if (lastSelectedClienteId) {
        console.log("Usando ID desde lastSelectedClienteId:", lastSelectedClienteId);
        return lastSelectedClienteId;
    }

    // Elemento oculto como último recurso
    const clienteIdElement = document.getElementById('clienteIdActual');
    if (clienteIdElement && clienteIdElement.value) {
        console.log("Usando ID desde elemento oculto:", clienteIdElement.value);
        return clienteIdElement.value;
    }

    console.error("No se encontró ningún ID de cliente");
    return null;
}

// Función para cargar notificaciones del cliente
function cargarNotificaciones(clienteId) {
    const container = document.getElementById('notificaciones-container');

    if (!container) {
        console.error("No se encontró el contenedor de notificaciones");
        return;
    }

    // Si no hay clienteId, mostrar mensaje apropiado
    if (!clienteId) {
        container.innerHTML = `
            <div class="text-center py-4">
                <i class="bi bi-person-x fs-1 text-muted"></i>
                <p class="mt-2">Seleccione un cliente para ver sus notificaciones</p>
            </div>
        `;
        return;
    }

    console.log(`Cargando notificaciones para cliente ID: ${clienteId}`);

    // Mostrar indicador de carga
    container.innerHTML = `
        <div class="text-center py-4">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Cargando...</span>
            </div>
            <p class="mt-2">Cargando notificaciones...</p>
        </div>
    `;

    // Llamar a la API para obtener notificaciones
    fetch(`/api/Notificacion/cliente/${clienteId}`)
        .then(response => {
            console.log("Respuesta del servidor:", response.status, response.statusText);
            if (!response.ok) {
                throw new Error(`Error al cargar notificaciones: ${response.status} ${response.statusText}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Notificaciones cargadas:', data);
            renderNotificaciones(data, container);
        })
        .catch(error => {
            console.error('Error:', error);
            container.innerHTML = `
                <div class="alert alert-danger" role="alert">
                    <i class="bi bi-exclamation-triangle-fill me-2"></i>
                    Error al cargar notificaciones: ${error.message}
                </div>
            `;
        });
}

// Renderizar notificaciones en el contenedor
function renderNotificaciones(notificaciones, container) {
    if (!notificaciones || notificaciones.length === 0) {
        container.innerHTML = `
            <div class="text-center py-4">
                <i class="bi bi-bell-slash fs-1 text-muted"></i>
                <p class="mt-2">No tienes notificaciones recientes</p>
            </div>
        `;
        return;
    }

    // Crear lista de notificaciones
    let html = '<div class="list-group notificaciones-lista">';

    notificaciones.forEach(notif => {
        // Determinar clase y color según el estado
        let badgeClass = getBadgeClassForEstado(notif.estadoDeLaVenta);
        let iconClass = getIconForEstado(notif.estadoDeLaVenta);

        // Formatear fecha
        const fecha = new Date(notif.fecha);
        const fechaFormateada = formatearFechaAmPm(fecha);

        html += `
            <div class="list-group-item list-group-item-action ${notif.leida ? 'bg-light' : ''}" 
                 data-notif-id="${notif.id}"
                 data-notif-estado="${notif.estadoDeLaVenta}"
                 data-notif-venta-id="${notif.idVenta}"
                 data-notif-factura="${notif.numeroDeFactura}">
                <div class="d-flex w-100 justify-content-between align-items-center">
                    <div class="estado-icon me-3">
                        <span class="badge ${badgeClass} p-2">
                            <i class="${iconClass}"></i>
                        </span>
                    </div>
                    <div class="flex-grow-1">
                        <h6 class="mb-1 fw-bold">${notif.titulo}</h6>
                        <p class="mb-1">${notif.mensaje}</p>
                        <small class="text-muted">${fechaFormateada}</small>
                    </div>
                    <div>
                        <button class="btn btn-sm btn-secondary ver-detalle-btn" 
                                data-notif-id="${notif.id}"
                                data-notif-estado="${notif.estadoDeLaVenta}"
                                data-notif-venta-id="${notif.idVenta}"
                                data-notif-factura="${notif.numeroDeFactura}">
                            <i class="fas fa-eye"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
    });

    html += '</div>';
    container.innerHTML = html;

    // Configurar eventos para los botones de ver detalle
    const detalleBtns = document.querySelectorAll('.ver-detalle-btn');
    detalleBtns.forEach(btn => {
        btn.addEventListener('click', (e) => {
            const id = btn.getAttribute('data-notif-id');
            const estado = btn.getAttribute('data-notif-estado');
            const ventaId = btn.getAttribute('data-notif-venta-id');
            const factura = btn.getAttribute('data-notif-factura');
            mostrarDetalleNotificacion(id, estado, ventaId, factura);
        });
    });

    // Hacer que las filas también sean clickeables
    const notificacionesItems = document.querySelectorAll('.notificaciones-lista .list-group-item');
    notificacionesItems.forEach(item => {
        item.addEventListener('click', (e) => {
            // Evitar que se active si se hizo click en el botón
            if (e.target.closest('.ver-detalle-btn')) return;

            const id = item.getAttribute('data-notif-id');
            const estado = item.getAttribute('data-notif-estado');
            const ventaId = item.getAttribute('data-notif-venta-id');
            const factura = item.getAttribute('data-notif-factura');
            mostrarDetalleNotificacion(id, estado, ventaId, factura);
        });
    });
}

// Mostrar detalle de una notificación en el modal
function mostrarDetalleNotificacion(id, estado, ventaId, factura) {
    // Mostrar cargando
    Swal.fire({
        title: 'Cargando detalles...',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    // Obtener detalles completos de la venta
    fetch(`/api/Venta/${ventaId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('No se pudieron cargar los detalles de la compra');
            }
            return response.json();
        })
        .then(venta => {
            console.log("Detalles de venta:", venta);

            const montoTotal = venta.montoTotal || venta.MontoTotal || 0;
            const montoPago = venta.montoDePago || venta.MontoDePago || venta.montoPago || venta.MontoPago || 0;
            const montoCambio = venta.montoDeCambio || venta.MontoDeCambio || venta.montoCambio || venta.MontoCambio || 0;
            const clienteNombre = venta.nombreCompletoCliente || venta.NombreCompletoCliente || venta.cliente?.nombreCompleto || 'No disponible';
            const clienteEmail = venta.emailCliente || venta.EmailCliente || venta.cliente?.email || 'No disponible';

            // Obtener estado y clase de badge
            const badgeClass = getBadgeClassForEstado(estado);
            const iconClass = getIconForEstado(estado);

            let detallesHTML = `
                <div class="text-center mb-4">
                    <div class="estado-badge mb-3">
                        <span class="badge ${badgeClass} p-3 fs-4">
                            <i class="${iconClass} me-2"></i> ${estado}
                        </span>
                    </div>
                    <p class="fs-5">${getMensajeEstadoDetallado(estado, factura)}</p>
                </div>
                
                <div class="mb-3">
                    <h5 class="border-bottom pb-2">Información del Pedido</h5>
                    <div class="row">
                        <div class="col-6">
                            <p><strong>Cliente:</strong> ${clienteNombre}</p>
                            <p><strong>Email:</strong> ${clienteEmail}</p>
                        </div>
                        <div class="col-6">
                            <p><strong>Fecha:</strong> ${formatearFechaAmPm(venta.fechaDeRegistro || venta.FechaDeRegistro)}</p>
                            <p><strong>Estado:</strong> ${venta.nombreEstadoDeLaVenta || venta.NombreEstadoDeLaVenta || estado}</p>
                        </div>
                    </div>
                </div>
                
                <h5 class="border-bottom pb-2">Detalle de Productos</h5>
                <div class="table-responsive">
                    <table class="table table-striped">
                        <thead>
                            <tr>
                                <th>Código</th>
                                <th>Producto</th>
                                <th>Categoría</th>
                                <th>Precio</th>
                                <th>Cantidad</th>
                                <th>Subtotal</th>
                            </tr>
                        </thead>
                        <tbody>
            `;

            const detalles = venta.detalles || venta.detallesDeLaVenta || venta.DetallesDeLaVenta || [];

            if (detalles && detalles.length > 0) {
                detalles.forEach(detalle => {
                    detallesHTML += `
                        <tr>
                            <td>${detalle.codigoDelProducto || detalle.CodigoDelProducto || detalle.producto?.codigo || 'N/A'}</td>
                            <td>
                                ${detalle.nombreDelProducto || detalle.NombreDelProducto || detalle.producto?.nombre || 'Producto no disponible'}
                                ${detalle.imagenDelProducto ?
                        `<br><img src="${detalle.imagenDelProducto}" alt="${detalle.nombreDelProducto || 'Producto'}" class="img-thumbnail" style="max-width: 50px;">` :
                        detalle.producto?.imagenUrl ?
                            `<br><img src="${detalle.producto.imagenUrl}" alt="${detalle.producto.nombre || 'Producto'}" class="img-thumbnail" style="max-width: 50px;">` :
                            ''
                    }
                            </td>
                            <td>${detalle.nombreCategoria || detalle.NombreCategoria || detalle.producto?.categoria?.nombre || 'Sin categoría'}</td>
                            <td>Q ${(detalle.precioDeVenta || detalle.PrecioDeVenta || 0).toFixed(2)}</td>
                            <td>${detalle.cantidad || detalle.Cantidad || 0}</td>
                            <td>Q ${((detalle.precioDeVenta || detalle.PrecioDeVenta || 0) * (detalle.cantidad || detalle.Cantidad || 0)).toFixed(2)}</td>
                        </tr>
                    `;
                });
            } else {
                detallesHTML += `
                    <tr>
                        <td colspan="6" class="text-center">No hay detalles disponibles para esta compra</td>
                    </tr>
                `;
            }

            detallesHTML += `
                        </tbody>
                        <tfoot>
                            <tr>
                                <th colspan="5" class="text-end">Total:</th>
                                <th>Q ${montoTotal.toFixed(2)}</th>
                            </tr>
                            <tr>
                                <th colspan="5" class="text-end">Pago:</th>
                                <td>Q ${montoPago.toFixed(2)}</td>
                            </tr>
                            <tr>
                                <th colspan="5" class="text-end">Cambio:</th>
                                <td>Q ${montoCambio.toFixed(2)}</td>
                            </tr>
                        </tfoot>
                    </table>
                </div>
            `;

            // Mostrar modal con los detalles usando SweetAlert2
            Swal.fire({
                title: `Detalles de Compra #${factura}`,
                html: detallesHTML,
                width: '800px',
                confirmButtonText: 'Cerrar',
                customClass: {
                    container: 'swal-wide',
                    popup: 'swal-wide'
                }
            });
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                title: 'Error',
                text: 'No se pudieron cargar los detalles de la compra',
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        });

    // Configurar imágenes ampliables después de agregar los productos al DOM
    setTimeout(() => configurarImagenesAmpliables(), 50);
}

// Obtener clase de badge según el estado
function getBadgeClassForEstado(estado) {
    const estadoLower = estado.toLowerCase();
    switch (estadoLower) {
        case 'recibida':
            return 'bg-info';
        case 'despachada':
            return 'bg-warning';
        case 'entregada':
            return 'bg-success';
        case 'devolución solicitada':
        case 'devolucion solicitada':
            return 'bg-danger';
        case 'devolución completada':
        case 'devolucion completada':
            return 'bg-secondary';
        default:
            return 'bg-primary';
    }
}

// Obtener icono según el estado
function getIconForEstado(estado) {
    const estadoLower = estado.toLowerCase();
    switch (estadoLower) {
        case 'recibida':
            return 'bi bi-inbox';
        case 'despachada':
            return 'bi bi-truck';
        case 'entregada':
            return 'bi bi-check-circle';
        case 'devolución solicitada':
        case 'devolucion solicitada':
            return 'bi bi-arrow-return-left';
        case 'devolución completada':
        case 'devolucion completada':
            return 'bi bi-arrow-repeat';
        default:
            return 'bi bi-tag';
    }
}

// Obtener mensaje detallado para el modal según el estado
function getMensajeEstadoDetallado(estado, factura) {
    const estadoLower = estado.toLowerCase();
    switch (estadoLower) {
        case 'recibida':
            return `Tu pedido #${factura} ha sido recibido correctamente. Estamos preparándolo para su despacho. Te notificaremos cuando sea enviado.`;
        case 'despachada':
            return `¡Buenas noticias! Tu pedido #${factura} ha sido despachado y está en camino. Pronto llegará a tu dirección de entrega.`;
        case 'entregada':
            return `¡Tu pedido #${factura} ha sido entregado exitosamente! Esperamos que disfrutes de tus productos. Gracias por confiar en SuperBodega.`;
        case 'devolución solicitada':
        case 'devolucion solicitada':
            return `Hemos recibido tu solicitud de devolución para el pedido #${factura}. Estamos procesando tu solicitud y te mantendremos informado.`;
        case 'devolución completada':
        case 'devolucion completada':
            return `La devolución de tu pedido #${factura} ha sido procesada exitosamente. Los productos han sido retornados a nuestro inventario. Gracias por elegirnos.`;
        default:
            return `Tu pedido #${factura} ha sido actualizado. El nuevo estado es: ${estado}.`;
    }
}

// Formatear fecha con formato AM/PM
function formatearFechaAmPm(fechaStr) {
    // Si no hay fecha, devolver cadena vacía
    if (!fechaStr) return '';

    // Crear objeto Date a partir de la cadena
    const fecha = new Date(fechaStr);

    // Verificar si la fecha es válida
    if (isNaN(fecha.getTime())) return 'Fecha inválida';

    // Ajustamos la fecha a la zona horaria local explícitamente
    const fechaLocal = new Date(fecha.getTime());

    // Formatear los componentes de la fecha
    const dia = fechaLocal.getDate().toString().padStart(2, '0');
    const mes = (fechaLocal.getMonth() + 1).toString().padStart(2, '0');
    const año = fechaLocal.getFullYear();

    // Formatear hora en formato 12 horas
    let horas = fechaLocal.getHours();
    const minutos = fechaLocal.getMinutes().toString().padStart(2, '0');
    const ampm = horas >= 12 ? 'p.m.' : 'a.m.';
    horas = horas % 12;
    horas = horas ? horas : 12; // La hora '0' debe mostrarse como '12'
    const horasStr = horas.toString().padStart(2, '0');

    return `${dia}/${mes}/${año} ${horasStr}:${minutos} ${ampm}`;
}

// Actualizar contador de notificaciones en menú lateral
function actualizarContadorNotificaciones() {
    const clienteId = obtenerClienteId();

    fetch(`/api/Notificacion/cliente/${clienteId}`)
        .then(response => response.ok ? response.json() : [])
        .then(notificaciones => {
            const noLeidas = notificaciones.filter(n => !n.leida).length;
            const countBadge = document.getElementById('notificacionesCountBadge');
            if (countBadge) {
                countBadge.textContent = noLeidas > 0 ? noLeidas : '';
                countBadge.classList.toggle('d-none', noLeidas === 0);
            }
        })
        .catch(error => console.error('Error al actualizar contador:', error));
}

// Función para mostrar mensaje de error
function mostrarMensajeError(mensaje) {
    Swal.fire({
        icon: 'error',
        title: 'Error',
        text: mensaje
    });
}