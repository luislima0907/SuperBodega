document.addEventListener('DOMContentLoaded', () => {
    // Cargar los clientes en el selector
    loadClientes();

    const pagoSection = document.querySelector('.card-body:last-of-type');
    if (pagoSection) {
        const preferenciasDiv = document.createElement('div');
        preferenciasDiv.className = 'form-check mb-3';
        preferenciasDiv.innerHTML = `
            <input class="form-check-input" type="checkbox" id="notificacionSincronica" 
                   ${localStorage.getItem('preferNotificacionSincronica') === 'true' ? 'checked' : ''}>
            <label class="form-check-label" for="notificacionSincronica">
                Recibir notificación inmediata por correo
            </label>
            <small class="form-text text-muted d-block">
                Si está habilitado, recibirá el email inmediatamente. De lo contrario, se procesará en segundo plano.
            </small>
        `;

        pagoSection.insertBefore(preferenciasDiv, pagoSection.querySelector('.d-flex.justify-content-end'));

        // Guardar preferencia al cambiar
        document.getElementById('notificacionSincronica').addEventListener('change', function() {
            localStorage.setItem('preferNotificacionSincronica', this.checked);
        });
    }

    // Configurar observador específico para imágenes en realizar-compra
    const rcCartItems = document.getElementById('rc-cartItems');
    if (rcCartItems) {
        const observer = new MutationObserver(() => {
            setTimeout(() => {
                const imagenes = rcCartItems.querySelectorAll('img');
                imagenes.forEach(img => {
                    if (!img.getAttribute('data-ampliable-configurado')) {
                        img.setAttribute('data-ampliable-configurado', 'true');
                        img.addEventListener('click', function() {
                            mostrarImagenAmpliada(this.src, this.alt || 'Producto');
                        });
                    }
                });
            }, 100);
        });

        observer.observe(rcCartItems, {
            childList: true,
            subtree: true
        });
    }

    // Configurar el cambio de cliente
    const clienteSelect = document.getElementById('clienteSelect');
    if (clienteSelect) {
        clienteSelect.addEventListener('change', function() {
            const selectedClienteId = this.value;
            if (selectedClienteId) {
                localStorage.setItem('lastClienteId', selectedClienteId);
                loadClienteInfo(selectedClienteId);
                loadCartItems(selectedClienteId);
            }
        });
    }

    document.getElementById('rc-montoPago')
        .addEventListener('input', () => calcularCambio());

    document.getElementById('rc-confirmBtn')
        .addEventListener('click', confirmarCompra);

    // Establecer fecha actual
    const now = new Date();
    const fechaInput = document.getElementById('fechaDeRegistro');
    const fechaVisibleInput = document.getElementById('fechaDeRegistroVisible');

    if (fechaInput && fechaVisibleInput) {
        // Si no hay valor en el campo oculto, establecer la fecha actual
        if (!fechaInput.value) {

            // Mostrar fecha formateada en el campo visible
            fechaVisibleInput.value = formatearFechaAmPm(now);
        } else {
            // Si ya hay una fecha, solo formatearla
            fechaVisibleInput.value = formatearFechaAmPm(new Date(fechaInput.value));
        }
    }

    if (fechaInput) {
        // Ocultar el campo original
        fechaInput.type = 'hidden';

        // Formatear para el input datetime-local (yyyy-MM-ddThh:mm)
        // Ajustando para usar hora local en vez de UTC
        const año = now.getFullYear();
        const mes = (now.getMonth() + 1).toString().padStart(2, '0');
        const dia = now.getDate().toString().padStart(2, '0');
        const horas = now.getHours().toString().padStart(2, '0');
        const minutos = now.getMinutes().toString().padStart(2, '0');

        // Formato correcto para datetime-local
        fechaInput.value = `${año}-${mes}-${dia}T${horas}:${minutos}`;
    }

    function loadClientes() {
        fetch('/api/cliente')
            .then(response => {
                if (!response.ok) {
                    throw new Error('No se pudieron cargar los clientes');
                }
                return response.json();
            })
            .then(data => {
                const clienteSelect = document.getElementById('clienteSelect');

                // Limpiar opciones existentes
                clienteSelect.innerHTML = '<option value="" selected disabled>Seleccione un cliente</option>';

                // Agregar los clientes activos al selector
                data.filter(cliente => cliente.estado).forEach(cliente => {
                    const option = document.createElement('option');
                    option.value = cliente.id;
                    option.textContent = `${cliente.nombre} ${cliente.apellido}`;
                    clienteSelect.appendChild(option);
                });

                // Seleccionar el cliente guardado en localStorage
                const lastClienteId = localStorage.getItem('lastClienteId');
                if (lastClienteId && clienteSelect.querySelector(`option[value="${lastClienteId}"]`)) {
                    clienteSelect.value = lastClienteId;
                    loadClienteInfo(lastClienteId);
                    loadCartItems(lastClienteId);
                } else if (clienteSelect.options.length > 1) {
                    // Si no hay cliente guardado o el guardado no existe, selecciona el primero válido
                    clienteSelect.selectedIndex = 1;
                    const clienteInicialId = clienteSelect.value;
                    localStorage.setItem('lastClienteId', clienteInicialId);
                    loadClienteInfo(clienteInicialId);
                    loadCartItems(clienteInicialId);
                } else {
                    // No hay clientes disponibles
                    Swal.fire('Error', 'No hay clientes disponibles', 'error');
                }
            })
            .catch(error => {
                console.error('Error al cargar clientes:', error);
                Swal.fire('Error', 'No se pudieron cargar los clientes', 'error');
            });
    }

    function loadClienteInfo(id) {
        fetch(`/api/cliente/${id}`)
            .then(r => r.ok ? r.json() : Promise.reject())
            .then(c => {
                document.getElementById('clienteInfo').innerHTML = `
                    <ul class="list-group">
                        <li class="list-group-item d-flex justify-content-between">
                            <span class="fw-bold">Nombre:</span>
                            <span>${c.nombre} ${c.apellido}</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between">
                            <span class="fw-bold">Email:</span>
                            <span>${c.email || 'No disponible'}</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between">
                            <span class="fw-bold">Teléfono:</span>
                            <span>${c.telefono || 'No disponible'}</span>
                        </li>
                        <li class="list-group-item d-flex justify-content-between">
                            <span class="fw-bold">Dirección:</span>
                            <span>${c.direccion || 'No disponible'}</span>
                        </li>
                    </ul>`;
            })
            .catch(error => {
                console.error('Error:', error);
                document.getElementById('clienteInfo').innerHTML = `
                    <div class="alert alert-danger">
                        No se pudo cargar la información del cliente
                    </div>`;
            });
    }

    function loadCartItems(id) {
        fetch(`/api/ecommerce/Carrito/${id}`)
            .then(r => r.ok ? r.json() : Promise.reject())
            .then(data => {
                if (data.elementos && data.elementos.length > 0) {
                    renderCart(data.elementos);
                } else {
                    const tbody = document.getElementById('rc-cartItems');
                    tbody.innerHTML = `
                        <tr>
                            <td colspan="4" class="text-center py-3">
                                <i class="bi bi-cart-x fs-1 mb-2 d-block text-secondary"></i>
                                Este cliente no tiene productos en el carrito
                            </td>
                        </tr>`;
                    document.getElementById('rc-total').textContent = 'Q 0.00';
                    document.getElementById('rc-cambio').value = '';
                    document.getElementById('rc-montoPago').value = '';
                }
            })
            .catch(error => {
                console.error('Error:', error);
                const tbody = document.getElementById('rc-cartItems');
                tbody.innerHTML = `
                    <tr>
                        <td colspan="4" class="text-center py-3 text-danger">
                            Error al cargar el carrito
                        </td>
                    </tr>`;
            });
    }

    function renderCart(items) {
        const tbody = document.getElementById('rc-cartItems');
        tbody.innerHTML = '';
        let total = 0;
        items.forEach(it => {
            const subtotal = it.precioUnitario * it.cantidad;
            total += subtotal;

            // Usar la misma estructura que en el carrito
            const imagenUrl = it.imagenUrl || '/images/productos/default.png';
            const categoriaNombre = it.productoCategoriaNombre || 'Sin categoría';

            tbody.innerHTML += `
                <tr>
                    <td>
                        <div class="d-flex align-items-center">
                            <img src="${imagenUrl}" 
                                class="me-3 pedido-img-thumbnail" 
                                alt="${it.productoNombre}" 
                                data-ampliable="true"
                                style="width: 40px; height: 40px; object-fit: cover; cursor: pointer;"
                                title="Clic para ampliar">
                            <div>
                                <h6 class="mb-0">${it.productoNombre}</h6>
                                <small class="text-muted">${categoriaNombre}</small>
                            </div>
                        </div>
                    </td>
                    <td class="text-center align-middle">Q ${it.precioUnitario.toFixed(2)}</td>
                    <td class="text-center align-middle">${it.cantidad}</td>
                    <td class="text-center align-middle">Q ${subtotal.toFixed(2)}</td>
                </tr>`;
        });
        document.getElementById('rc-total').textContent = `Q ${total.toFixed(2)}`;
        calcularCambio();

        // Configurar imágenes ampliables manualmente después de agregar al DOM
        setTimeout(() => {
            const imagenes = document.querySelectorAll('#rc-cartItems img');
            imagenes.forEach(img => {
                if (!img.getAttribute('data-ampliable-configurado')) {
                    img.setAttribute('data-ampliable-configurado', 'true');
                    img.addEventListener('click', function() {
                        mostrarImagenAmpliada(this.src, this.alt || 'Producto');
                    });
                }
            });
        }, 100);
    }

    function calcularCambio() {
        const pago = parseFloat(document.getElementById('rc-montoPago').value) || 0;
        const total = parseFloat(document.getElementById('rc-total').textContent.replace('Q','').trim())||0;
        document.getElementById('rc-cambio').value = pago>=total
            ? (pago - total).toFixed(2)
            : '';
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

    async function confirmarCompra() {
        const clienteId = document.getElementById('clienteSelect').value;
        if (!clienteId) {
            return Swal.fire('Error', 'Debe seleccionar un cliente', 'error');
        }

        const pago = parseFloat(document.getElementById('rc-montoPago').value);
        const total = parseFloat(document.getElementById('rc-total').textContent.replace('Q','').trim());

        if (isNaN(pago) || pago <= 0) {
            return Swal.fire('Error', 'Ingrese un monto de pago válido', 'error');
        }

        if (pago < total) {
            return Swal.fire('Error','El monto pagado debe ser igual o mayor al total','error');
        }

        Swal.fire({
            title: 'Procesando compra',
            text: 'Espere un momento...',
            allowOutsideClick: false,
            didOpen: () => {
                Swal.showLoading();
            }
        });

        const cartResponse = await fetch(`/api/ecommerce/Carrito/${clienteId}`);
        if (!cartResponse.ok) throw new Error('No se pudo cargar el carrito');
        const data = await cartResponse.json();

        if (!data.elementos || data.elementos.length === 0) {
            throw new Error('El carrito está vacío');
        }

        // Obtener información de proveedores para cada producto
        const detallesPromises = data.elementos.map(async (el) => {
            // Obtener detalles del producto para conocer su proveedor
            const productoResponse = await fetch(`/api/producto/${el.productoId}`);
            const producto = await productoResponse.json();

            return {
                idProducto: el.productoId,
                cantidad: el.cantidad,
                precioDeVenta: el.precioUnitario,
                idProveedor: producto.proveedorId || 1,
                nombreDelProveedor: producto.proveedorNombre || "Proveedor por defecto"
            };
        });

        const detalles = await Promise.all(detallesPromises);

        // Obtener preferencia de notificación
        const notificacionSincronica = document.getElementById('notificacionSincronica')?.checked ||
            (localStorage.getItem('preferNotificacionSincronica') === 'true');

        return fetch('/api/Venta/Create',{
            method:'POST',
            headers:{'Content-Type':'application/json'},
            body: JSON.stringify({
                idCliente: parseInt(clienteId),
                montoDePago: pago,
                detalles,
                fechaDeRegistro: document.getElementById('fechaDeRegistro').value,
                usarNotificacionSincronica: notificacionSincronica
            })
        })
            .then(response => {
                if (!response.ok) {
                    return response.text().then(text => {
                        try {
                            // Intenta analizar como JSON para obtener el mensaje de error
                            const errorData = JSON.parse(text);
                            throw new Error(errorData.message || `Error ${response.status}`);
                        } catch (e) {
                            // Si no es JSON, usa el texto tal cual
                            throw new Error(`Error ${response.status}: ${text || 'Error desconocido'}`);
                        }
                    });
                }
                return response.json();
            })
            .then(data => {
                // Proceder a limpiar el carrito SOLO si la venta se realizó correctamente
                return fetch(`/api/ecommerce/Carrito/clear/client/${clienteId}`,{ method:'DELETE' })
                    .then(() => data); // Pasar los datos de la venta al siguiente then
            })
            .then(ventaData => {
                Swal.fire({
                    title: '¡Éxito!',
                    text: `Compra realizada correctamente. Factura: ${ventaData.numeroDeFactura}`,
                    icon: 'success',
                    confirmButtonText: 'Ver mis pedidos'
                }).then(() => location.href = '/MisPedidos/Index');
            })
            .catch(error => {
                console.error('Error al confirmar la compra:', error);
                Swal.fire('Error', 'No se pudo confirmar la compra', 'error');
            });
    }
});