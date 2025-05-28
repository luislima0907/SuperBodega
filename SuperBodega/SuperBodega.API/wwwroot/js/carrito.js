/**
 * carrito.js - Manejo de la interacción con el carrito
 */

function isCartPage() {
    return window.location.pathname.includes('/Carrito/Index');
}

document.addEventListener('DOMContentLoaded', function() {
    if (!isCartPage()) {
        initCartFunctionality();
    } else {
        initCartPageFunctionality();
    }

    setTimeout(() => configurarImagenesAmpliables(), 100);

    updateCartBadge();
});

function initCartPageFunctionality() {
    const clienteId = localStorage.getItem('lastClienteId');
    if (!clienteId) {
        showEmptyCartMessage(true, 'No se ha seleccionado ningún cliente');
        return;
    }

    // Cargar los clientes en el selector y seleccionar el último usado
    loadClientesForCart();

    // Cargar los elementos del carrito
    loadCartItems(clienteId);

    // Configurar el cambio de cliente
    const clienteSelect = document.getElementById('clienteSelect');
    if (clienteSelect) {
        clienteSelect.addEventListener('change', function() {
            const selectedClienteId = this.value;
            if (selectedClienteId) {
                localStorage.setItem('lastClienteId', selectedClienteId);
                loadClienteDetails(selectedClienteId);
                loadCartItems(selectedClienteId);
            }
        });
    }

    // Configurar el botón de guardar cambios del modal de edición
    const btnConfirmEdit = document.getElementById('confirmEditBtn');
    if (btnConfirmEdit) {
        // Remover listener previo si existe
        btnConfirmEdit.replaceWith(btnConfirmEdit.cloneNode(true));
        // Obtener nueva referencia y agregar listener
        const newBtnConfirmEdit = document.getElementById('confirmEditBtn');
        newBtnConfirmEdit.addEventListener('click', saveCartItemChanges);
    }

    // Configurar el botón de vaciar carrito
    const btnClearCart = document.getElementById('btnClearCart');
    if (btnClearCart) {
        // Remover listener previo si existe
        btnClearCart.replaceWith(btnClearCart.cloneNode(true));
        // Obtener nueva referencia y agregar listener
        const newBtnClearCart = document.getElementById('btnClearCart');
        newBtnClearCart.addEventListener('click', confirmClearCart);
    }
}

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

    // Mostrar indicador de carga, ocultar detalles
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
            Swal.fire({
                title: 'Error',
                text: 'No se pudo cargar la información del cliente',
                icon: 'error'
            });
        });
}

function loadClientesForCart() {
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


            // Cargar detalles y carrito del cliente inicial si existe
            if (clienteInicialId) {
                loadClienteDetails(clienteInicialId);
                loadCartItems(clienteInicialId);
            } else {
                // Si no hay clientes, mostrar mensaje adecuado
                showEmptyCartMessage(true, 'No hay clientes disponibles o activos.');
                document.getElementById('clienteDetails').style.display = 'none';
            }


            // Definir el nuevo listener
            const changeListener = function() {
                const selectedClienteId = this.value;
                if (selectedClienteId) {
                    localStorage.setItem('lastClienteId', selectedClienteId);
                    loadClienteDetails(selectedClienteId);
                    loadCartItems(selectedClienteId);
                }
            };

            // Guardar referencia y agregar el listener
            clienteSelect._changeListener = changeListener;
            clienteSelect.addEventListener('change', changeListener);

        })
        .catch(error => {
            console.error('Error al cargar clientes:', error);
            clienteSelect.innerHTML = '<option value="" selected disabled>Error al cargar clientes</option>';
            showEmptyCartMessage(true, 'Error al cargar la lista de clientes.');
            document.getElementById('clienteDetails').style.display = 'none';
        });
}

function loadCartItems(clienteId) {
    const cartContent = document.getElementById('cartContent'); // Div que contiene la tabla y total
    const cartEmptyMessage = document.getElementById('cartEmptyMessage');
    const cartItemsContainer = document.getElementById('cartItemsContainer'); // tbody de la tabla

    // Ocultar contenido y mensaje vacío inicialmente
    if (cartContent) cartContent.style.display = 'none';
    if (cartEmptyMessage) cartEmptyMessage.style.display = 'none';

    // Mostrar spinner dentro del tbody
    if (cartItemsContainer) {
        cartItemsContainer.innerHTML = `
            <tr>
                <td colspan="5" class="text-center py-4">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Cargando...</span>
                    </div>
                    <p class="mt-2">Cargando elementos del carrito...</p>
                </td>
            </tr>
        `;
        // Hacemos visible el contenedor principal del carrito para que se vea el spinner
        if (cartContent) cartContent.style.display = 'block';
    } else {
        console.error("Contenedor cartItemsContainer no encontrado.");
        return; // Salir si no existe el tbody
    }


    fetch(`/api/ecommerce/Carrito/${clienteId}`)
        .then(response => {
            if (!response.ok) {
                // Si el cliente no existe o hay otro error
                if (response.status === 404) {
                    throw new Error('Carrito no encontrado para este cliente.');
                }
                throw new Error(`Error al cargar el carrito (${response.status})`);
            }
            return response.json();
        })
        .then(data => {
            if (!data.elementos || data.elementos.length === 0) {
                showEmptyCartMessage(true); // Mostrar mensaje vacío, ocultar tabla
            } else {
                renderCartItems(data); // Renderizar items, mostrar tabla, ocultar mensaje vacío
            }
            // Actualizar badge del menú lateral
            updateCartBadge();
        })
        .catch(error => {
            console.error('Error:', error);
            // Mostrar mensaje vacío con el error, ocultar tabla
            showEmptyCartMessage(true, `Error al cargar el carrito: ${error.message}`);
        });
}

function renderCartItems(cartData) {
    const cartItemsContainer = document.getElementById('cartItemsContainer'); // tbody
    const cartContent = document.getElementById('cartContent'); // Contenedor de tabla y total
    const cartEmptyMessage = document.getElementById('cartEmptyMessage');
    const cartTotal = document.getElementById('cartTotal');
    const btnClearCart = document.getElementById('btnClearCart');

    if (!cartItemsContainer || !cartContent || !cartEmptyMessage || !cartTotal) {
        console.error("Elementos necesarios para renderizar el carrito no encontrados.");
        return;
    }


    cartItemsContainer.innerHTML = ''; // Limpiar spinner o items anteriores

    cartData.elementos.forEach(item => {
        const row = document.createElement('tr');
        row.dataset.id = item.id; // ID del ElementoCarrito

        const categoriaNombre = item.productoCategoriaNombre || 'Sin categoría';
        const imagenUrl = item.imagenUrl || '/images/productos/default.png';

        row.innerHTML = `
            <td>
                <div class="cart-item-details">
                    <img src="${imagenUrl}" class="product-image" alt="${item.productoNombre}"
                    alt="${item.nombre}" 
                    class="img-thumbnail me-2"
                    data-nombre="${item.nombre}"
                    style="width: 50px; height: 50px; object-fit: cover;">
                    <div>
                        <h6 class="mb-0">${item.productoNombre}</h6>
                        <small class="text-muted">${categoriaNombre}</small>
                    </div>
                </div>
            </td>
            <td class="text-center align-middle">Q ${parseFloat(item.precioUnitario).toFixed(2)}</td>
            <td class="text-center align-middle">${item.cantidad}</td>
            <td class="text-center align-middle">Q ${parseFloat(item.subtotal).toFixed(2)}</td>
            <td class="align-middle">
                <button class="btn btn-sm btn-primary me-1 btn-edit-item" title="Editar Cantidad" data-item-id="${item.id}" data-producto-id="${item.productoId}" data-stock="${item.productoStock}">
                    <i class="fas fa-edit"></i>
                </button>
                <button class="btn btn-sm btn-danger btn-delete-item" title="Eliminar" data-item-id="${item.id}">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;

        cartItemsContainer.appendChild(row);
    });

    // Actualizar total
    cartTotal.textContent = `Q ${parseFloat(cartData.total).toFixed(2)}`;

    // Mostrar contenido del carrito, ocultar mensaje vacío
    cartContent.style.display = 'block';
    cartEmptyMessage.style.display = 'none';
    if (btnClearCart) btnClearCart.style.display = 'inline-block';

    // Configurar los botones de editar y eliminar
    setupCartItemButtons();

    // Configurar las imágenes después de renderizar
    setTimeout(() => configurarImagenesAmpliables(), 50);
}

function showEmptyCartMessage(show, message = null) {
    const cartContent = document.getElementById('cartContent'); // Contenedor de tabla y total
    const cartEmptyMessage = document.getElementById('cartEmptyMessage');
    const btnClearCart = document.getElementById('btnClearCart'); 

    if (cartContent) {
        cartContent.style.display = show ? 'none' : 'block'; // Oculta tabla y total
    }

    if (btnClearCart) {
        btnClearCart.style.display = 'none'; 
    }

    if (cartEmptyMessage) {
        cartEmptyMessage.style.display = show ? 'block' : 'none'; // Muestra mensaje vacío

        // Si se proporcionó un mensaje personalizado, actualizarlo
        if (message && show) {
            const messageElement = cartEmptyMessage.querySelector('p.text-muted');
            if (messageElement) {
                messageElement.textContent = message;
            } else {
                const titleElement = cartEmptyMessage.querySelector('h4');
                if (titleElement) titleElement.textContent = message;
            }
        } else if (show) {
            // Restaurar mensaje por defecto si no se proporcionó uno
            const messageElement = cartEmptyMessage.querySelector('p.text-muted');
            const titleElement = cartEmptyMessage.querySelector('h4');
            if (titleElement) titleElement.textContent = 'El carrito está vacío';
            if (messageElement) messageElement.textContent = 'Este cliente no ha agregado productos a su carrito.';
        }
    }
}

/**
 * Muestra confirmación para vaciar todo el carrito.
 */
function confirmClearCart() {
    const clienteId = document.getElementById('clienteSelect').value;
    if (!clienteId) {
        Swal.fire('Error', 'No se ha seleccionado un cliente.', 'error');
        return;
    }

    Swal.fire({
        title: '¿Vaciar Carrito?',
        text: "Esta acción eliminará TODOS los productos de su carrito actual. ¿Está seguro?",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, vaciar carrito',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            clearCart(clienteId);
        }
    });
}

/**
 * Realiza la llamada a la API para vaciar el carrito del cliente.
 */
function clearCart(clienteId) {
    Swal.fire({
        title: 'Vaciando carrito...',
        didOpen: () => { Swal.showLoading(); },
        allowOutsideClick: false, allowEscapeKey: false, showConfirmButton: false
    });

    fetch(`/api/ecommerce/Carrito/clear/client/${clienteId}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text || 'Error al vaciar el carrito.'); });
            }
            return response.json();
        })
        .then(data => {
            Swal.fire('¡Carrito Vacío!', data?.message || 'Todos los productos han sido eliminados.', 'success');
            // Recargar vista del carrito (mostrará mensaje vacío)
            loadCartItems(clienteId);
            updateCartBadge(); // Actualizar badge a 0
        })
        .catch(error => {
            Swal.fire('Error', `No se pudo vaciar el carrito: ${error.message}`, 'error');
        });
}

function setupCartItemButtons() {
    // Configurar los botones de editar y eliminar
    document.querySelectorAll('.btn-edit-item').forEach(button => {
        // Remover listener previo si existe para evitar duplicados
        button.replaceWith(button.cloneNode(true));
        // Obtener nueva referencia y agregar listener
        const newButton = document.querySelector(`.btn-edit-item[data-item-id="${button.dataset.itemId}"]`);
        if (newButton) {
            newButton.addEventListener('click', function() {
                const itemId = this.dataset.itemId;
                openEditCartItemModal(itemId);
            });
        }
    });

    document.querySelectorAll('.btn-delete-item').forEach(button => {
        // Remover listener previo si existe
        button.replaceWith(button.cloneNode(true));
        // Obtener nueva referencia y agregar listener
        const newButton = document.querySelector(`.btn-delete-item[data-item-id="${button.dataset.itemId}"]`);
        if (newButton) {
            newButton.addEventListener('click', function() {
                const row = this.closest('tr');
                const itemId = this.dataset.itemId;
                confirmDeleteCartItem(itemId, row);
            });
        }
    });
}

/**
 * Inicializa la funcionalidad del carrito en el catálogo
 */
function initCartFunctionality() {
    // Agregar evento a los botones "Añadir al carrito"
    document.addEventListener('click', function(event) {
        if (event.target.classList.contains('btn-add-cart') ||
            event.target.parentElement.classList.contains('btn-add-cart')) {

            const button = event.target.classList.contains('btn-add-cart') ?
                event.target : event.target.parentElement;
            const card = button.closest('.product-card');

            if (card) {
                // Extraer datos del producto
                const productoId = card.dataset.id;
                const nombre = card.querySelector('.producto-nombre').textContent;
                const precio = parseFloat(card.dataset.precio);
                const stock = parseInt(card.dataset.stock);

                // Verificar stock antes de abrir el modal
                if (stock <= 0) {
                    Swal.fire({
                        title: 'Sin stock',
                        text: 'Este producto no tiene unidades disponibles',
                        icon: 'warning'
                    });
                    return;
                }

                // Configurar el modal con los datos del producto
                openAddToCartModal(productoId, nombre, precio, stock);
            }
        }
    });

    // Configurar los botones de incremento/decremento de cantidad
    setupQuantityButtons();

    // Cargar clientes para el select
    loadClientes();

    // Evento para confirmar la adición al carrito
    document.getElementById('confirmAddToCartBtn').addEventListener('click', function() {
        if (validateCartForm()) {
            addToCart();
        }
    });

    // Evento para actualizar subtotal cuando cambia la cantidad
    document.getElementById('cantidadInput').addEventListener('change', updateSubtotal);
}

/**
 * Abre el modal para agregar un producto al carrito
 */
function openAddToCartModal(productoId, nombre, precio, stock) {
    // Llenar los campos del modal
    document.getElementById('productoId').value = productoId;
    document.getElementById('productoNombre').value = nombre;
    document.getElementById('productoPrecio').value = precio.toFixed(2);
    document.getElementById('productoStock').value = stock;

    // Restablecer la cantidad a 1
    const cantidadInput = document.getElementById('cantidadInput');
    cantidadInput.value = 1;
    cantidadInput.max = stock; 

    const modalClienteSelect = document.getElementById('clienteSelect'); 
    const clienteId = modalClienteSelect.value || localStorage.getItem('lastClienteId'); 

    fetchAndSetModalQuantity(clienteId, productoId);

    // Mostrar el modal
    const modalElement = document.getElementById('addToCartModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalElement); 
    modal.show();
}

/**
 * Carga los clientes desde la API para mostrarlos en el selector
 */
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

            // Limpiar opciones existentes salvo la primera (placeholder)
            while (clienteSelect.options.length > 1) {
                clienteSelect.options.remove(1);
            }

            // Agregar los clientes al selector
            data.filter(cliente => cliente.estado).forEach(cliente => {
                const option = document.createElement('option');
                option.value = cliente.id;
                option.textContent = `${cliente.nombre} ${cliente.apellido}`;
                clienteSelect.appendChild(option);
            });

            // Si hay un cliente guardado en localStorage, seleccionarlo
            const lastClienteId = localStorage.getItem('lastClienteId');
            if (lastClienteId) {
                clienteSelect.value = lastClienteId;
            }
        })
        .catch(error => {
            console.error('Error al cargar clientes:', error);
            Swal.fire({
                title: 'Error',
                text: 'No se pudieron cargar los clientes. Por favor, intente nuevamente.',
                icon: 'error'
            });
        });
}

/**
 * Configura los botones para incrementar y decrementar la cantidad
 */
function setupQuantityButtons() {
    const decreaseBtn = document.getElementById('decreaseCantidad');
    const increaseBtn = document.getElementById('increaseCantidad');
    const cantidadInput = document.getElementById('cantidadInput');
    const stockInput = document.getElementById('productoStock');

    decreaseBtn.addEventListener('click', function() {
        const currentValue = parseInt(cantidadInput.value);
        if (currentValue > 1) {
            cantidadInput.value = currentValue - 1;
            updateSubtotal();
        }
    });

    increaseBtn.addEventListener('click', function() {
        const currentValue = parseInt(cantidadInput.value);
        const maxStock = parseInt(stockInput.value);

        if (currentValue < maxStock) {
            cantidadInput.value = currentValue + 1;
            updateSubtotal();
        } else {
            Swal.fire({
                title: 'Atención',
                text: 'No puede agregar más unidades que el stock disponible',
                icon: 'warning'
            });
        }
    });
}

/**
 * Actualiza el campo de subtotal cuando cambia la cantidad
 */
function updateSubtotal() {
    const cantidad = parseInt(document.getElementById('cantidadInput').value) || 0;
    const precioUnitario = parseFloat(document.getElementById('productoPrecio').value) || 0;
    const subtotal = cantidad * precioUnitario;

    document.getElementById('subtotalInput').value = subtotal.toFixed(2);
}

/**
 * Valida el formulario antes de agregar al carrito
 */
function validateCartForm() {
    const clienteSelect = document.getElementById('clienteSelect');
    const cantidadInput = document.getElementById('cantidadInput');
    const stockInput = document.getElementById('productoStock');

    // Validar que se haya seleccionado un cliente
    if (!clienteSelect.value) {
        clienteSelect.classList.add('is-invalid');
        return false;
    } else {
        clienteSelect.classList.remove('is-invalid');
    }

    // Validar que la cantidad sea un número positivo
    const cantidad = parseInt(cantidadInput.value);
    if (isNaN(cantidad) || cantidad <= 0) {
        cantidadInput.classList.add('is-invalid');
        return false;
    } else {
        cantidadInput.classList.remove('is-invalid');
    }

    // Validar que la cantidad no exceda el stock
    const maxStock = parseInt(stockInput.value);
    if (cantidad > maxStock) {
        Swal.fire({
            title: 'Error',
            text: 'La cantidad solicitada excede el stock disponible',
            icon: 'error'
        });
        return false;
    }

    return true;
}

/**
 * Añade el producto actual al carrito
 */
function addToCart() {
    // Obtener los valores del formulario
    const productoId = document.getElementById('productoId').value;
    const clienteId = document.getElementById('clienteSelect').value;
    const cantidad = parseInt(document.getElementById('cantidadInput').value);

    // Guardar el cliente seleccionado para futuras referencias
    localStorage.setItem('lastClienteId', clienteId);

    // Primero comprobar si el producto ya existe en el carrito
    fetch(`/api/ecommerce/Carrito/${clienteId}`)
        .then(response => {
            if (!response.ok) {
                if (response.status === 404) {
                    // Si no hay carrito para el cliente, crear uno nuevo directamente
                    return { elementos: [] };
                }
                throw new Error('Error al verificar el carrito');
            }
            return response.json();
        })
        .then(cartData => {
            // Buscar si el producto ya está en el carrito
            const existingItem = cartData.elementos.find(item => item.productoId == productoId);

            if (existingItem) {
                // El producto ya existe, actualizar la cantidad en lugar de agregar
                return updateCartItemQuantity(existingItem.id, cantidad);
            } else {
                // El producto no existe, agregarlo normalmente
                return addNewCartItem(clienteId, productoId, cantidad);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                title: 'Error',
                text: 'No se pudo verificar el carrito. Por favor, intente de nuevo.',
                icon: 'error'
            });
        });
}

/**
 * Actualiza la cantidad de un elemento existente del carrito
 */
function updateCartItemQuantity(elementoId, newCantidad) {
    // Mostrar spinner de carga
    Swal.fire({
        title: 'Actualizando carrito',
        text: 'Por favor espere...',
        didOpen: () => {
            Swal.showLoading();
        },
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false
    });

    // Llamar a la API para actualizar el elemento
    return fetch(`/api/ecommerce/Carrito/Edit/${elementoId}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ cantidad: newCantidad })
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al actualizar el carrito');
            }
            return response.json();
        })
        .then(data => {
            // Cerrar el spinner de carga
            Swal.close();

            // Cerrar el modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('addToCartModal'));
            modal.hide();

            // Mostrar mensaje de éxito
            Swal.fire({
                title: '¡Actualizado!',
                text: 'La cantidad del producto ha sido actualizada en el carrito.',
                icon: 'success',
                showConfirmButton: true
            });

            // Actualizar el contador de elementos en el carrito
            updateCartBadge();
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                title: 'Error',
                text: 'No se pudo actualizar el carrito. Por favor, intente de nuevo.',
                icon: 'error'
            });
        });
}

/**
 * Agrega un nuevo elemento al carrito
 */
function addNewCartItem(clienteId, productoId, cantidad) {
    // Datos para la API
    const cartData = {
        clienteId: clienteId,
        productoId: productoId,
        cantidad: cantidad
    };

    // Mostrar spinner de carga
    Swal.fire({
        title: 'Agregando al carrito',
        text: 'Por favor espere...',
        didOpen: () => {
            Swal.showLoading();
        },
        allowOutsideClick: false,
        allowEscapeKey: false,
        showConfirmButton: false
    });

    // Llamar a la API para agregar al carrito
    return fetch('/api/ecommerce/Carrito', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(cartData)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al agregar al carrito');
            }
            return response.json();
        })
        .then(data => {
            // Cerrar el spinner de carga
            Swal.close();

            // Cerrar el modal
            const modal = bootstrap.Modal.getInstance(document.getElementById('addToCartModal'));
            modal.hide();

            // Mostrar mensaje de éxito
            Swal.fire({
                title: '¡Agregado!',
                text: 'El producto se ha añadido a su carrito',
                icon: 'success',
                showConfirmButton: true,
                confirmButtonText: 'Ver Carrito'
            }).then((result) => {
                if (result.isConfirmed) {
                    window.location.href = "/Carrito/Index";
                }
            });

            // Actualizar el contador de elementos en el carrito
            updateCartBadge();
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire({
                title: 'Error',
                text: 'No se pudo agregar el producto al carrito. Por favor, intente de nuevo.',
                icon: 'error'
            });
        });
}

/**
 * Abre el modal para editar la cantidad de un elemento del carrito.
 */
function openEditCartItemModal(itemId) {
    const row = document.querySelector(`tr[data-id="${itemId}"]`);
    if (!row) {
        console.error('No se encontró la fila del item:', itemId);
        return;
    }

    // Obtener datos de la fila o del botón si es necesario
    const editButton = row.querySelector('.btn-edit-item');
    const productoNombre = row.querySelector('.cart-item-details h6').textContent;
    const precioUnitarioText = row.querySelector('td:nth-child(2)').textContent.replace('Q', '').trim();
    const cantidadActual = parseInt(row.querySelector('td:nth-child(3)').textContent);
    const stockDisponible = parseInt(editButton.dataset.stock); // Usar data-stock del botón

    // Llenar el modal de edición
    document.getElementById('editElementoCarritoId').value = itemId;
    document.getElementById('editProductoNombre').value = productoNombre;
    document.getElementById('editProductoPrecio').value = parseFloat(precioUnitarioText).toFixed(2);
    document.getElementById('editProductoStock').value = stockDisponible;
    document.getElementById('editCantidadInput').value = cantidadActual;
    document.getElementById('editCantidadInput').max = stockDisponible; // Establecer máximo

    // Calcular subtotal inicial
    updateEditSubtotal();

    // Configurar botones de cantidad del modal de edición
    setupEditQuantityButtons();

    // Mostrar el modal
    const modalElement = document.getElementById('editCartItemModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalElement);
    modal.show();
}

/**
 * Configura los botones de +/- cantidad en el modal de edición.
 */
function setupEditQuantityButtons() {
    const decreaseBtn = document.getElementById('editDecreaseCantidad');
    const increaseBtn = document.getElementById('editIncreaseCantidad');
    const cantidadInput = document.getElementById('editCantidadInput');
    const stockInput = document.getElementById('editProductoStock');

    // Remover listeners previos para evitar duplicados si se llama múltiples veces
    decreaseBtn.replaceWith(decreaseBtn.cloneNode(true));
    increaseBtn.replaceWith(increaseBtn.cloneNode(true));
    cantidadInput.replaceWith(cantidadInput.cloneNode(true));

    // Obtener referencias a los nuevos elementos clonados
    const newDecreaseBtn = document.getElementById('editDecreaseCantidad');
    const newIncreaseBtn = document.getElementById('editIncreaseCantidad');
    const newCantidadInput = document.getElementById('editCantidadInput');

    newDecreaseBtn.addEventListener('click', function() {
        const currentValue = parseInt(newCantidadInput.value);
        if (currentValue > 1) {
            newCantidadInput.value = currentValue - 1;
            updateEditSubtotal();
        }
    });

    newIncreaseBtn.addEventListener('click', function() {
        const currentValue = parseInt(newCantidadInput.value);
        const maxStock = parseInt(stockInput.value); // Usar el stock del campo readonly

        if (currentValue < maxStock) {
            newCantidadInput.value = currentValue + 1;
            updateEditSubtotal();
        } else {
            Swal.fire({
                title: 'Atención',
                text: 'No puede agregar más unidades que el stock disponible',
                icon: 'warning'
            });
        }
    });

    newCantidadInput.addEventListener('change', updateEditSubtotal); // Actualizar al cambiar manualmente
    newCantidadInput.addEventListener('input', updateEditSubtotal); // Actualizar mientras se escribe
}

/**
 * Actualiza el subtotal en el modal de edición.
 */
function updateEditSubtotal() {
    const cantidad = parseInt(document.getElementById('editCantidadInput').value) || 0;
    const precioUnitario = parseFloat(document.getElementById('editProductoPrecio').value) || 0;
    const subtotal = cantidad * precioUnitario;

    document.getElementById('editSubtotalInput').value = subtotal.toFixed(2);
}


/**
 * Guarda los cambios realizados en el modal de edición.
 */
function saveCartItemChanges() {
    const elementoId = document.getElementById('editElementoCarritoId').value;
    const cantidadInput = document.getElementById('editCantidadInput');
    const cantidad = parseInt(cantidadInput.value);
    const stock = parseInt(document.getElementById('editProductoStock').value);

    // Validaciones
    if (isNaN(cantidad) || cantidad <= 0) {
        cantidadInput.classList.add('is-invalid');
        Swal.fire('Error', 'La cantidad debe ser un número positivo.', 'error');
        return;
    } else {
        cantidadInput.classList.remove('is-invalid');
    }

    if (cantidad > stock) {
        cantidadInput.classList.add('is-invalid');
        Swal.fire('Error', 'La cantidad no puede exceder el stock disponible.', 'error');
        return;
    } else {
        cantidadInput.classList.remove('is-invalid');
    }

    // Mostrar spinner
    Swal.fire({
        title: 'Guardando cambios...',
        didOpen: () => { Swal.showLoading(); },
        allowOutsideClick: false, allowEscapeKey: false, showConfirmButton: false
    });

    // Llamada a la API para actualizar
    fetch(`/api/ecommerce/Carrito/Edit/${elementoId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ cantidad: cantidad })
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text || 'Error al actualizar el elemento.'); });
            }
            return response.json();
        })
        .then(data => {
            Swal.close();
            const modalElement = document.getElementById('editCartItemModal');
            const modal = bootstrap.Modal.getInstance(modalElement);
            modal.hide();

            Swal.fire('¡Actualizado!', 'La cantidad del producto ha sido actualizada.', 'success');

            // Recargar los items del carrito para reflejar cambios y totales
            const clienteId = document.getElementById('clienteSelect').value;
            if (clienteId) {
                loadCartItems(clienteId);
            }
            updateCartBadge(); // Actualizar badge
        })
        .catch(error => {
            Swal.fire('Error', `No se pudo actualizar el producto: ${error.message}`, 'error');
        });
}

/**
 * Muestra confirmación para eliminar un elemento del carrito.
 */
function confirmDeleteCartItem(itemId, rowElement) {
    Swal.fire({
        title: '¿Está seguro?',
        text: "Esta acción eliminará el producto de su carrito.",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            deleteCartItem(itemId, rowElement);
        }
    });
}

/**
 * Realiza la llamada a la API para eliminar un elemento del carrito.
 */
function deleteCartItem(itemId, rowElement) {
    // Mostrar un indicador de carga mientras se elimina
    Swal.fire({
        title: 'Eliminando...',
        didOpen: () => { Swal.showLoading(); },
        allowOutsideClick: false, allowEscapeKey: false, showConfirmButton: false
    });

    fetch(`/api/ecommerce/Carrito/Delete/${itemId}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (!response.ok) {
                // Si la respuesta no es OK, intenta leer el texto del error
                return response.text().then(text => {
                    throw new Error(text || `Error ${response.status} al eliminar el producto.`);
                });
            }
            // Si la respuesta es OK, intenta parsear el JSON
            return response.json();
        })
        .then(data => {
            // Éxito: Mostrar mensaje y recargar
            Swal.fire('¡Eliminado!', data?.message || 'El producto ha sido eliminado del carrito.', 'success');

            // Animar y luego recargar
            if (rowElement) {
                rowElement.classList.add('fade-out');
                setTimeout(() => {
                    const clienteId = document.getElementById('clienteSelect').value;
                    if (clienteId) {
                        loadCartItems(clienteId); // Recarga después de la animación
                    }
                    updateCartBadge();
                }, 500); // Duración de la animación
            } else {
                // Si no hay elemento de fila, recargar inmediatamente
                const clienteId = document.getElementById('clienteSelect').value;
                if (clienteId) {
                    loadCartItems(clienteId);
                }
                updateCartBadge();
            }
        })
        .catch(error => {
            // Error: Mostrar mensaje de error
            Swal.fire('Error', `No se pudo eliminar el producto: ${error.message}`, 'error');
            // Quitar clase de animación si se añadió y falló
            if (rowElement) {
                rowElement.classList.remove('fade-out');
            }
        });
}

/**
 * Actualiza el contador de productos en el carrito
 */
function updateCartBadge() {
    const clienteId = document.getElementById('clienteSelect')?.value || localStorage.getItem('lastClienteId'); // Intenta obtener del select primero

    if (!clienteId) {
        hideCartBadge();
        return;
    }

    fetchCartCount(clienteId);
}

/**
 * Obtiene el conteo de productos en el carrito del cliente dado
 */
function fetchCartCount(clienteId) {
    fetch(`/api/ecommerce/Carrito/count/${clienteId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al obtener el conteo del carrito');
            }
            return response.json();
        })
        .then(data => {
            if (data.count > 0) {
                showCartBadge(data.count);
            } else {
                hideCartBadge();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            hideCartBadge();
        });
}

function fetchAndSetModalQuantity(clienteId, productoId) {
    const cantidadInput = document.getElementById('cantidadInput');
    if (!clienteId || !productoId || !cantidadInput) {
        if (cantidadInput) cantidadInput.value = 1;
        updateSubtotal();
        return;
    }

    fetch(`/api/ecommerce/Carrito/${clienteId}`)
        .then(response => {
            if (!response.ok) {
                if (response.status === 404) return { elementos: [] };
                throw new Error('Error al buscar item en carrito');
            }
            return response.json();
        })
        .then(data => {
            const itemInCart = data.elementos.find(el => el.productoId == productoId);

            if (itemInCart) {
                cantidadInput.value = itemInCart.cantidad;
            } else {
                cantidadInput.value = 1; 
            }
            updateSubtotal(); 
        })
        .catch(error => {
            console.error("Error fetching cart item quantity:", error);
            cantidadInput.value = 1;
            updateSubtotal();
        });
}

/**
 * Muestra la insignia del contador del carrito con el número de elementos
 */
function showCartBadge(count) {
    // Buscar el enlace del carrito en el menú lateral
    const cartLink = document.querySelector('a[href*="CarritoView"]');
    if (!cartLink) return;

    // Buscar o crear el badge
    let badge = cartLink.querySelector('.badge');
    if (!badge) {
        badge = document.createElement('span');
        badge.className = 'badge bg-danger rounded-pill ms-2';
        cartLink.appendChild(badge);
    }

    // Actualizar el contador
    badge.textContent = count;
    badge.style.display = 'inline-block';
}

/**
 * Oculta la insignia del contador del carrito
 */
function hideCartBadge() {
    const cartLink = document.querySelector('a[href*="CarritoView"]');
    if (!cartLink) return;

    const badge = cartLink.querySelector('.badge');
    if (badge) {
        badge.style.display = 'none';
    }
}