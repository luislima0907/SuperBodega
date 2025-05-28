document.addEventListener('DOMContentLoaded',() => {
    // Cargar los clientes en el selector
    loadClientesForPedidos();

    // Configurar el cambio de cliente
    const clienteSelect = document.getElementById('clienteSelect');
    if (clienteSelect) {
        clienteSelect.addEventListener('change', function() {
            const selectedClienteId = this.value;
            if (selectedClienteId) {
                localStorage.setItem('lastClienteId', selectedClienteId);
                loadClienteDetails(selectedClienteId);
                loadPedidos(selectedClienteId);
            }
        });
    }

    // Cargar preferencias de notificación
    const preferenciasSwitch = document.getElementById('notificacionSincronica');
    if (preferenciasSwitch) {
        const preferenciaSincronica = localStorage.getItem('preferNotificacionSincronica') === 'true';
        preferenciasSwitch.checked = preferenciaSincronica;
    }
});

// Guardar preferencias de notificación
function guardarPreferenciasNotificacion() {
    const notificacionSincronica = document.getElementById('notificacionSincronica').checked;
    localStorage.setItem('preferNotificacionSincronica', notificacionSincronica);

    // Mostrar confirmación al usuario
    const toastContainer = document.createElement('div');
    toastContainer.className = 'position-fixed bottom-0 end-0 p-3';
    toastContainer.style.zIndex = '11';

    toastContainer.innerHTML = `
        <div class="toast align-items-center text-white bg-success" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="bi bi-check-circle me-1"></i>
                    Preferencia de notificación guardada correctamente
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>
    `;

    document.body.appendChild(toastContainer);
    const toast = new bootstrap.Toast(toastContainer.querySelector('.toast'));
    toast.show();

    // Eliminar el toast después de mostrarse
    toastContainer.addEventListener('hidden.bs.toast', () => {
        document.body.removeChild(toastContainer);
    });
}

// Cargar clientes para el selector
function loadClientesForPedidos() {
    const clienteSelect = document.getElementById('clienteSelect');
    if (!clienteSelect) return;

    // Guardar referencia al listener si ya existe para removerlo
    const existingListener = clienteSelect._changeListener;
    if (existingListener) {
        clienteSelect.removeEventListener('change', existingListener);
    }

    fetch('/api/cliente')
        .then(response => {
            if (!response.ok) {
                throw new Error('No se pudieron cargar los clientes');
            }
            return response.json();
        })
        .then(data => {
            // Limpiar opciones existentes
            clienteSelect.innerHTML = '<option value="" selected disabled>Seleccione un cliente</option>';

            // Agregar los clientes al selector
            data.filter(cliente => cliente.estado).forEach(cliente => {
                const option = document.createElement('option');
                option.value = cliente.id;
                option.textContent = `${cliente.nombre} ${cliente.apellido}`;
                clienteSelect.appendChild(option);
            });

            // Seleccionar el cliente guardado en localStorage o el primero si no hay guardado
            const lastClienteId = localStorage.getItem('lastClienteId');
            let clienteInicialId = null;

            if (lastClienteId && clienteSelect.querySelector(`option[value="${lastClienteId}"]`)) {
                clienteSelect.value = lastClienteId;
                clienteInicialId = lastClienteId;
            } else if (clienteSelect.options.length > 1) {
                // Si no hay cliente guardado o el guardado no existe, selecciona el primero válido
                clienteSelect.selectedIndex = 1;
                clienteInicialId = clienteSelect.value;
                localStorage.setItem('lastClienteId', clienteInicialId); // Guardar el seleccionado por defecto
            }

            // Cargar detalles y pedidos del cliente inicial si existe
            if (clienteInicialId) {
                loadClienteDetails(clienteInicialId);
                loadPedidos(clienteInicialId);
            } else {
                // Si no hay clientes, mostrar mensaje adecuado
                showNoPedidosMessage(true, 'No hay clientes disponibles o activos.');
                document.getElementById('clienteDetails').style.display = 'none';
            }

            // Definir el nuevo listener
            const changeListener = function() {
                const selectedClienteId = this.value;
                if (selectedClienteId) {
                    localStorage.setItem('lastClienteId', selectedClienteId);
                    loadClienteDetails(selectedClienteId);
                    loadPedidos(selectedClienteId);
                }
            };

            // Guardar referencia y agregar el listener
            clienteSelect._changeListener = changeListener;
            clienteSelect.addEventListener('change', changeListener);
        })
        .catch(error => {
            console.error('Error al cargar clientes:', error);
            Swal.fire('Error', 'No se pudieron cargar los clientes', 'error');
        });
}

// Cargar detalles del cliente
function loadClienteDetails(clienteId) {
    const clienteDetails = document.getElementById('clienteDetails');
    const clienteNombre = document.getElementById('clienteNombre');
    const clienteEmail = document.getElementById('clienteEmail');
    const clienteTelefono = document.getElementById('clienteTelefono');
    const clienteDireccion = document.getElementById('clienteDireccion');

    if (!clienteDetails || !clienteNombre || !clienteTelefono || !clienteDireccion) {
        console.error("Elementos de detalles del cliente no encontrados.");
        return;
    }

    // Ocultar detalles mientras se cargan
    clienteDetails.style.display = 'none';

    fetch(`/api/cliente/${clienteId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('No se pudo cargar la información del cliente');
            }
            return response.json();
        })
        .then(cliente => {
            clienteNombre.textContent = `${cliente.nombre} ${cliente.apellido}`;
            if (clienteEmail) clienteEmail.textContent = cliente.email || 'No disponible';
            clienteTelefono.textContent = cliente.telefono || 'No disponible';
            clienteDireccion.textContent = cliente.direccion || 'No disponible';
            clienteDetails.style.display = 'block';
        })
        .catch(error => {
            console.error('Error:', error);
            clienteDetails.style.display = 'none';
            Swal.fire('Error', 'No se pudo cargar la información del cliente', 'error');
        });
}

// Cargar pedidos del cliente
function loadPedidos(clienteId) {
    const tbody = document.getElementById('mp-tbody');
    const pedidosContent = document.getElementById('pedidosContent');
    const noPedidosMessage = document.getElementById('noPedidosMessage');

    // Mostrar spinner de carga
    if (tbody) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-4">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="mt-2">Cargando pedidos...</p>
                </td>
            </tr>
        `;
    }

    // Ocultar mensajes hasta que se verifiquen los datos
    if (noPedidosMessage) noPedidosMessage.style.display = 'none';
    if (pedidosContent) pedidosContent.style.display = 'block';

    fetch(`/api/Venta/cliente/${clienteId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error(`Error al cargar pedidos (${response.status})`);
            }
            return response.json();
        })
        .then(lista => {
            if (!lista || lista.length === 0) {
                showNoPedidosMessage(true);
                return;
            }

            showNoPedidosMessage(false);
            renderPedidos(lista);
        })
        .catch(error => {
            console.error('Error:', error);
            // Mostrar mensaje de error
            if (tbody) {
                tbody.innerHTML = `<tr><td colspan="5" class="text-center text-danger">${error.message}</td></tr>`;
            }
        });
}

// Mostrar mensaje de sin pedidos
function showNoPedidosMessage(show, message = null) {
    const pedidosContent = document.getElementById('pedidosContent');
    const noPedidosMessage = document.getElementById('noPedidosMessage');

    if (pedidosContent) pedidosContent.style.display = show ? 'none' : 'block';
    if (noPedidosMessage) {
        noPedidosMessage.style.display = show ? 'block' : 'none';

        // Si se proporcionó un mensaje personalizado
        if (message && show) {
            const messageElement = noPedidosMessage.querySelector('p.text-muted');
            const titleElement = noPedidosMessage.querySelector('h4');

            if (messageElement) messageElement.textContent = message;
            if (titleElement) titleElement.textContent = 'No hay pedidos disponibles';
        } else if (show) {
            // Restaurar mensaje por defecto
            const messageElement = noPedidosMessage.querySelector('p.text-muted');
            const titleElement = noPedidosMessage.querySelector('h4');

            if (titleElement) titleElement.textContent = 'No hay pedidos para mostrar';
            if (messageElement) messageElement.textContent = 'Este cliente aún no ha realizado compras.';
        }
    }
}

// Renderizar pedidos en la tabla
function renderPedidos(lista) {
    const tbody = document.getElementById('mp-tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    lista.forEach(v => {
        // Determinar clase y color del badge según el estado
        let badgeClass;
        switch (v.nombreEstadoDeLaVenta.toLowerCase()) {
            case 'recibida':
                badgeClass = 'bg-info';
                break;
            case 'despachada':
                badgeClass = 'bg-warning';
                break;
            case 'entregada':
                badgeClass = 'bg-success';
                break;
            case 'devolución solicitada':
                badgeClass = 'bg-danger';
                break;
            default:
                badgeClass = 'bg-secondary';
        }

        const estadoLower = v.nombreEstadoDeLaVenta.toLowerCase();
        const disabled = (estadoLower !== 'recibida' && estadoLower !== 'entregada') ? 'disabled' : '';
        const fechaFormateada = formatearFechaAmPm(v.fechaDeRegistro);

        const row = document.createElement('tr');
        row.className = 'pedido-row';
        row.dataset.id = v.id;

        // Crear la fila principal del pedido
        row.innerHTML = `
            <td>${v.numeroDeFactura}</td>
            <td>${fechaFormateada}</td>
            <td>Q ${v.montoTotal.toFixed(2)}</td>
            <td><span class="badge ${badgeClass}">${v.nombreEstadoDeLaVenta}</span></td>
            <td>
                <button class="btn btn-sm btn-info mb-1 me-1" onclick="toggleProductDetails(${v.id})">
                    <i class="fa-solid fa-list-ul"></i> Productos
                </button>
                <button class="btn btn-sm btn-secondary mb-1 me-1" onclick="mostrarDetallesPedido(${v.id}, '${v.nombreEstadoDeLaVenta}', '${v.numeroDeFactura}')">
                    <i class="fas fa-eye"></i> Detalles
                </button>
                <button class="btn btn-sm btn-danger mb-1" ${disabled}
                    onclick="solicitarDevolucion(${v.id})">
                    <i class="fa-solid fa-arrow-rotate-right"></i>
                </button>
            </td>
        `;

        tbody.appendChild(row);

        // Crear fila para detalles de productos (inicialmente oculta)
        const detailsRow = document.createElement('tr');
        detailsRow.className = 'product-details-row';
        detailsRow.id = `details-${v.id}`;
        detailsRow.style.display = 'none';

        // Celda que abarca todas las columnas
        const detailsCell = document.createElement('td');
        detailsCell.colSpan = 5;

        if (v.detallesDeLaVenta && v.detallesDeLaVenta.length > 0) {
            detailsCell.innerHTML = `
                <div class="product-details p-3">
                    <h6 class="mb-3">Productos de esta compra:</h6>
                    <table class="table table-sm table-bordered">
                        <thead>
                            <tr>
                                <th>Producto</th>
                                <th>Categoría</th>
                                <th>Precio Unitario</th>
                                <th>Cantidad</th>
                                <th>Subtotal</th>
                            </tr>
                        </thead>
                        <tbody>
                            ${v.detallesDeLaVenta.map(detalle => `
                                <tr>
                                    <td>
                                        <div class="d-flex align-items-center">
                                            <img src="${detalle.imagenDelProducto || '/images/productos/default.png'}" 
                                                alt="${detalle.nombreDelProducto}" 
                                                data-ampliable-pedido="true" 
                                                class="pedido-img-thumbnail me-2" 
                                                style="width: 40px; height: 40px; object-fit: cover; cursor: pointer;"
                                                title="Clic para ampliar">
                                            <div>
                                                <span class="fw-bold">${detalle.codigoDelProducto}</span>
                                                <br>
                                                <span>${detalle.nombreDelProducto}</span>
                                            </div>
                                        </div>
                                    </td>
                                    <td>${detalle.nombreCategoria || 'Sin categoría'}</td>
                                    <td>Q ${detalle.precioDeVenta.toFixed(2)}</td>
                                    <td>${detalle.cantidad}</td>
                                    <td>Q ${detalle.montoTotal.toFixed(2)}</td>
                                </tr>
                            `).join('')}
                        </tbody>
                    </table>
                </div>
            `;
        }
        else {
            detailsCell.innerHTML = `
                <div class="alert alert-info m-3">
                    No hay detalles disponibles para este pedido.
                </div>
            `;
        }

        detailsRow.appendChild(detailsCell);
        tbody.appendChild(detailsRow);
    });

    // Después de añadir todas las filas, configurar imágenes con una espera mayor
    setTimeout(() => {
        // Configurar específicamente las imágenes de detalles de pedidos
        const imagenesProductosPedido = document.querySelectorAll('[data-ampliable-pedido="true"]');
        imagenesProductosPedido.forEach(img => {
            // Solo configurar si aún no tiene listener
            if (!img.getAttribute('data-ampliable')) {
                img.setAttribute('data-ampliable', 'true');
                img.addEventListener('click', function() {
                    mostrarImagenAmpliada(this.src, this.alt || 'Producto');
                });
            }
        });
    }, 300);

}

// Mostrar detalle de un pedido en un modal
function mostrarDetallesPedido(ventaId, estado, factura) {
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
                throw new Error('No se pudieron cargar los detalles del pedido');
            }
            return response.json();
        })
        .then(venta => {
            console.log("Detalles de pedido:", venta);

            const montoTotal = venta.montoTotal || venta.MontoTotal || 0;
            const montoPago = venta.montoDePago || venta.MontoDePago || venta.montoPago || venta.MontoPago || 0;
            const montoCambio = venta.montoDeCambio || venta.MontoDeCambio || venta.montoCambio || venta.MontoCambio || 0;
            const clienteNombre = venta.nombreCompletoCliente || venta.NombreCompletoCliente || venta.cliente?.nombreCompleto || 'No disponible';
            const clienteEmail = venta.emailCliente || venta.EmailCliente || venta.cliente?.email || 'No disponible';

            // Obtener clase y icono para badge según estado
            let badgeClass, iconClass;

            switch(estado.toLowerCase()) {
                case 'recibida':
                    badgeClass = 'bg-info';
                    iconClass = 'bi bi-inbox';
                    break;
                case 'despachada':
                    badgeClass = 'bg-warning';
                    iconClass = 'bi bi-truck';
                    break;
                case 'entregada':
                    badgeClass = 'bg-success';
                    iconClass = 'bi bi-check-circle';
                    break;
                case 'devolución solicitada':
                case 'devolucion solicitada':
                    badgeClass = 'bg-danger';
                    iconClass = 'bi bi-arrow-return-left';
                    break;
                case 'devolución completada':
                case 'devolucion completada':
                    badgeClass = 'bg-secondary';
                    iconClass = 'bi bi-arrow-repeat';
                    break;
                default:
                    badgeClass = 'bg-primary';
                    iconClass = 'bi bi-tag';
            }

            let detallesHTML = `
                <div class="text-center mb-4">
                    <div class="estado-badge mb-3">
                        <span class="badge ${badgeClass} p-3 fs-4">
                            <i class="${iconClass} me-2"></i> ${estado}
                        </span>
                    </div>
                    <p class="fs-5">${obtenerMensajeEstadoPedido(estado, factura)}</p>
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
                        <td colspan="6" class="text-center">No hay detalles disponibles para este pedido</td>
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
                title: `Detalles de Pedido #${factura}`,
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
                text: 'No se pudieron cargar los detalles del pedido',
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        });

    // Configurar imágenes ampliables después de agregar los productos al DOM
    setTimeout(() => configurarImagenesAmpliables(), 50);
}

// Obtener mensaje detallado para el modal según el estado del pedido
function obtenerMensajeEstadoPedido(estado, factura) {
    const estadoLower = estado.toLowerCase();
    switch (estadoLower) {
        case 'recibida':
            return `Tu pedido #${factura} ha sido recibido correctamente. Estamos preparándolo para su despacho.`;
        case 'despachada':
            return `¡Buenas noticias! Tu pedido #${factura} ha sido despachado y está en camino. Pronto llegará a tu dirección de entrega.`;
        case 'entregada':
            return `¡Tu pedido #${factura} ha sido entregado exitosamente! Esperamos que disfrutes de tus productos. Gracias por confiar en SuperBodega.`;
        case 'devolución solicitada':
        case 'devolucion solicitada':
            return `Has solicitado la devolución para el pedido #${factura}. Estamos procesando tu solicitud y te mantendremos informado.`;
        case 'devolución completada':
        case 'devolucion completada':
            return `La devolución de tu pedido #${factura} ha sido procesada exitosamente.`;
        default:
            return `Tu pedido #${factura} se encuentra actualmente en estado: ${estado}.`;
    }
}

// Función para mostrar/ocultar detalles de productos
window.toggleProductDetails = function(pedidoId) {
    const detailsRow = document.getElementById(`details-${pedidoId}`);
    if (detailsRow) {
        if (detailsRow.style.display === 'none') {
            detailsRow.style.display = 'table-row';
            detailsRow.classList.add('animate-fade-in');
        } else {
            detailsRow.style.display = 'none';
            detailsRow.classList.remove('animate-fade-in');
        }
    }
}

function solicitarDevolucion(id) {
    Swal.fire({
        title: '¿Solicitar devolución?',
        text: 'Esta acción no se puede deshacer. ¿Está seguro de solicitar la devolución de este pedido?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, solicitar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            // Mostrar cargando
            Swal.fire({
                title: 'Procesando...',
                text: 'Enviando solicitud de devolución',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Obtener preferencia de notificación
            const notificacionSincronica = localStorage.getItem('preferNotificacionSincronica') === 'true';

            // Cambia estado a 'Devolución' (idEstado 4)
            fetch(`/api/Venta/estado/edit/${id}`, {
                method: 'PUT',
                headers: {'Content-Type': 'application/json'},
                body: JSON.stringify({
                    "idEstadoDeLaVenta": 4,
                    "usarNotificacionSincronica": notificacionSincronica
                })
            }).then(response => {
                if (response.ok) {
                    Swal.close();
                    Swal.fire(
                        '¡Listo!',
                        `Se ha solicitado la devolución correctamente y se le notificará por email ${notificacionSincronica ? 'inmediatamente' : 'en breve'}`,
                        'success'
                    ).then(() => {
                        const clienteId = document.getElementById('clienteSelect').value;
                        if (clienteId) loadPedidos(clienteId);
                    });
                } else {
                    return response.text().then(text => {
                        console.error("Error response:", text);
                        Swal.fire('Error', 'No se pudo procesar la solicitud', 'error');
                    });
                }
            }).catch(error => {
                console.error(error);
                Swal.fire('Error', 'Ocurrió un problema al comunicarse con el servidor', 'error');
            });
        }
    });
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