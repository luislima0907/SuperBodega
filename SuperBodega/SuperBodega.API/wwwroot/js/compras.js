// Variables globales
let detallesCompra = [];
let productos = [];
let proveedores = [];
let currentPage = 1;
const pageSize = 10;
let totalPages = 0;
let compras = [];
let formularioModificado = false;

// Variables globales para control de cambios
let compraOriginal = null;
let formHasChanges = false;

// Función principal que se ejecuta cuando se carga la página
document.addEventListener('DOMContentLoaded', function() {
    inicializarCompra();

    // Reconfigurar los botones cuando se abre el modal de detalle de producto
    document.getElementById('detalleProductoModal').addEventListener('shown.bs.modal', function () {
        setupCantidadButtons();
    });
});

// Inicialización de la página de compras
function inicializarCompra() {
    // Cargar productos y proveedores
    cargarProductos();
    cargarProveedores();
    configurarBotonesCantidad();

    // Cargar detalles de la compra si estamos en modo edición
    if (window.location.pathname.includes("/Compras/Edit/")) {
        // Verificar si hay detalles de compra en el modelo pasado desde el servidor
        if (window.detallesCompraOriginal) {
            // Cargar los detalles en el array global
            cargarDetallesExistentes(window.detallesCompraOriginal);
        }
    }

    if (document.getElementById('comprasTableBody')) {
        loadCompras();

        // Configurar búsqueda
        document.getElementById('searchButton').addEventListener('click', function() {
            loadCompras(document.getElementById('searchInput').value);
        });

        document.getElementById('searchInput').addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                loadCompras(this.value);
            }
        });

        // Configurar el botón de limpiar búsqueda
        const btnLimpiar = document.getElementById('clearSearchButton');
        if (btnLimpiar) {
            btnLimpiar.addEventListener('click', function() {
                document.getElementById('searchInput').value = '';
                loadCompras();
            });
        }
    }

    // Configurar eventos
    $('#precio-compra, #cantidad').on('input', calcularSubtotal);
    $('#btn-agregar-detalle').on('click', agregarDetalle);
    $('#formCompra').on('submit', prepararEnvio);

    // Establecer fecha actual
    const now = new Date();
    const fechaInput = document.getElementById('fechaDeRegistro');
    const fechaVisibleInput = document.getElementById('fechaDeRegistroVisible');

    if (fechaInput && fechaVisibleInput) {
        // Si no hay valor en el campo oculto, establecer la fecha actual
        if (!fechaInput.value) {
            // Guardar fecha en formato ISO en el campo oculto
            // fechaInput.value = now.toISOString();

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

    // Configurar eventos para el formulario de edición
    const formEditarCompra = document.getElementById('formEditarCompra');
    if (formEditarCompra) {
        formEditarCompra.addEventListener('submit', function(e) {
            e.preventDefault();
            editarCompra();
        });
    }

    // Buscar productos
    $('#btn-buscar-producto').on('click', function() {
        const termino = $('#buscar-producto').val();
        buscarProductos(termino);
    });

    // Buscar proveedores
    $('#btn-buscar-proveedor').on('click', function() {
        const termino = $('#buscar-proveedor').val();
        buscarProveedores(termino);
    });

    // Permitir búsqueda al presionar Enter
    $('#buscar-producto').on('keypress', function(e) {
        if (e.which === 13) {
            const termino = $(this).val();
            buscarProductos(termino);
            e.preventDefault();
        }
    });

    $('#buscar-proveedor').on('keypress', function(e) {
        if (e.which === 13) {
            const termino = $(this).val();
            buscarProveedores(termino);
            e.preventDefault();
        }
    });

    $('#btn-limpiar-busqueda-producto').on('click', function() {
        $('#buscar-producto').val('');
        buscarProductos('');
    });

    $('#btn-limpiar-busqueda-proveedor').on('click', function() {
        $('#buscar-proveedor').val('');
        buscarProveedores('');
    });

    // Detectar cambios en el formulario
    $('#NumeroDeFactura, #proveedorSelect').on('change input', function() {
        formularioModificado = true;
    });

    // Inicializar verificación de factura duplicada
    $('#NumeroDeFactura').on('blur', function() {
        const numeroFactura = $(this).val().trim();
        if (numeroFactura) {
            verificarFacturaDuplicada(numeroFactura)
                .then(existe => {
                    if (existe) {
                        $('#NumeroDeFactura').addClass('is-invalid');
                        mostrarAlerta('warning', 'Advertencia', 'Este número de factura ya existe en el sistema.');
                    } else {
                        $('#NumeroDeFactura').removeClass('is-invalid');
                    }
                })
                .catch(error => console.error('Error al verificar factura:', error));
        }
    });

    // Generar automáticamente un número de factura al cargar el formulario de creación
    const numFacturaInput = document.getElementById('NumeroDeFactura');
    if (numFacturaInput && !numFacturaInput.value) {
        generarNumeroFacturaUnico().then(numeroFactura => {
            numFacturaInput.value = numeroFactura;
            // Hacer el campo de solo lectura para evitar ediciones manuales
            numFacturaInput.readOnly = true;
        });
    }

    // Agregar botón para regenerar número de factura en formulario de edición
    const numeroFacturaInput = document.getElementById('numeroFactura');
    if (numeroFacturaInput) {
        // Guardar el número original para el botón restaurar
        const numeroFacturaOriginal = numeroFacturaInput.value;
        numeroFacturaInput.setAttribute('data-original-factura', numeroFacturaOriginal);

        // Crear contenedor para el botón si no existe
        const inputGroup = numeroFacturaInput.closest('.input-group');
        // Verificar si el botón ya existe para evitar duplicados
        const existingButton = document.getElementById('btnRegenerarFactura');

        if (inputGroup && !existingButton) {
            let regenerarBtn = document.createElement('button');
            regenerarBtn.type = 'button';
            regenerarBtn.className = 'btn btn-secondary';
            regenerarBtn.id = 'btnRegenerarFactura';
            regenerarBtn.title = 'Generar nuevo número';
            regenerarBtn.innerHTML = '<i class="bi bi-arrow-repeat"></i>';

            // Agregar evento para regenerar
            regenerarBtn.addEventListener('click', function() {
                generarNumeroFacturaUnico().then(numeroFactura => {
                    numeroFacturaInput.value = numeroFactura;
                    formularioModificado = true;
                });
            });

            inputGroup.appendChild(regenerarBtn);
        }
    }

    // Configurar eventos para botones de limpiar y restaurar
    const btnLimpiarCompra = document.getElementById('btnLimpiarCompra');
    if (btnLimpiarCompra) {
        btnLimpiarCompra.addEventListener('click', function() {
            Swal.fire({
                title: 'Confirmar',
                text: '¿Está seguro que desea limpiar el formulario? Se perderán todos los datos ingresados.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Confirmar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    limpiarFormularioCompra();
                }
            });
        });
    }

    const btnRestaurarCompra = document.getElementById('btnRestaurarCompra');
    if (btnRestaurarCompra) {
        // Al cargar la página, guardar los datos originales
        guardarDatosOriginalesCompra();

        // Configurar el monitoreo de cambios
        monitorearCambiosEditarCompra();

        btnRestaurarCompra.addEventListener('click', function() {
            Swal.fire({
                title: 'Confirmar Restauración',
                text: '¿Está seguro que desea restaurar los datos a su versión original? Se perderán todos los cambios realizados.',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Restaurar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    restaurarDatosCompra();
                }
            });
        });
    }

    // Resetear el estado del modal al cerrarse
    $('#detalleProductoModal').on('hidden.bs.modal', function () {
        $(this).removeData('edit-index');
        $('#detalleProductoModalLabel').text('Agregar Producto');
        $('#btn-agregar-detalle').text('Agregar');
    });

    // Configurar interceptación de enlaces
    setupLinkInterception();
}

// Función para cargar detalles existentes
function cargarDetallesExistentes(detallesOriginales) {
    // Limpiar array global
    detallesCompra = [];

    // Convertir detalles del modelo a formato compatible con nuestra aplicación
    detallesOriginales.forEach(detalle => {
        // Construir una URL de imagen correcta
        let imagenUrl = detalle.imagenDelProducto || '/images/productos/default.png';
        // Verificar si la ruta es relativa y añadir la base si es necesario
        if (imagenUrl && !imagenUrl.startsWith('/') && !imagenUrl.startsWith('http')) {
            imagenUrl = '/' + imagenUrl;
        }

        detallesCompra.push({
            idProducto: detalle.idProducto,
            codigo: detalle.codigoDelProducto,
            nombre: detalle.nombreDelProducto,
            categoria: detalle.categoriaDelProducto || 'Sin categoría',
            precioDeCompra: detalle.precioDeCompra,
            precioDeVenta: detalle.precioDeVenta,
            cantidad: detalle.cantidad,
            subtotal: detalle.precioDeCompra * detalle.cantidad,
            detalleId: detalle.id,  // Guardar el ID original del detalle si existe
            imagenUrl: imagenUrl     // Almacenar la URL de la imagen
        });
    });

    // Actualizar la tabla con estos detalles
    actualizarTablaDetalles();
}

// Generar número de factura único verificando que no exista en la base de datos
async function generarNumeroFacturaUnico() {
    let numeroFactura;
    let existeFactura;

    // Intentamos hasta 10 veces para evitar un bucle infinito
    for (let i = 0; i < 10; i++) {
        numeroFactura = generarNumeroFactura();
        try {
            existeFactura = await verificarFacturaDuplicada(numeroFactura);
            if (!existeFactura) {
                return numeroFactura;
            }
        } catch (error) {
            console.error('Error al verificar factura:', error);
        }
    }

    // Si después de 10 intentos no encontramos uno único, añadimos timestamp
    const timestamp = Date.now().toString().slice(-3);
    return `${generarNumeroFactura()}-${timestamp}`;
}

// Calcular subtotal basado en precio y cantidad
function calcularSubtotal() {
    const precioCompra = parseFloat($('#precio-compra').val()) || 0;
    const cantidad = parseInt($('#cantidad').val()) || 0;
    const subtotal = precioCompra * cantidad;

    $('#subtotal').val(`Q ${subtotal.toFixed(2)}`);
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

// Función para generar un número de factura aleatorio en formato X-000
function generarNumeroFactura() {
    // Generar una letra aleatoria (A-Z)
    const letras = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    const letra = letras.charAt(Math.floor(Math.random() * letras.length));

    // Generar tres números aleatorios (100-999)
    const numeros = Math.floor(Math.random() * 900 + 100);

    // Combinar en formato X-000
    return `${letra}-${numeros}`;
}

// Verificar si el número de factura ya existe
function verificarFacturaDuplicada(numeroFactura) {
    return new Promise((resolve, reject) => {
        fetch('/api/Compra')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al verificar número de factura');
                }
                return response.json();
            })
            .then(compras => {
                // Filtrar en el cliente para buscar el número de factura
                const existeFactura = compras.some(compra =>
                    compra.numeroDeFactura.toLowerCase() === numeroFactura.toLowerCase().trim());

                resolve(existeFactura);
            })
            .catch(error => {
                console.error('Error al verificar factura:', error);
                reject(error);
            });
    });
}

// Mostrar alerta usando SweetAlert
function mostrarAlerta(tipo, titulo, mensaje) {
    Swal.fire({
        icon: tipo,
        title: titulo,
        text: mensaje,
        confirmButtonText: 'Aceptar',
        confirmButtonColor: tipo === 'success' ? '#198754' : '#0d6efd'
    });
}

// Configurar búsqueda en la tabla de compras
function configurarBusquedaCompras() {
    $('#searchInput').on('keyup', function() {
        const valor = $(this).val().toLowerCase();
        $('#comprasTableBody tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(valor) > -1);
        });
    });

    $('#searchButton').on('click', function() {
        const valor = $('#searchInput').val().toLowerCase();
        $('#comprasTableBody tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(valor) > -1);
        });
    });

    $('#clearSearchButton').on('click', function() {
        $('#searchInput').val('');
        $('#comprasTableBody tr').show();
    });
}

// Buscar proveedores por termino
function buscarProveedores(termino) {
    if (!termino) {
        mostrarProveedores(proveedores);
        return;
    }

    const filtrados = proveedores.filter(p =>
        p.nombre.toLowerCase().includes(termino.toLowerCase()) ||
        (p.email && p.email.toLowerCase().includes(termino.toLowerCase())) ||
        (p.telefono && p.telefono.toLowerCase().includes(termino.toLowerCase()))
    );

    mostrarProveedores(filtrados);
}

// Buscar productos por término
function buscarProductos(termino) {
    // Primero filtrar los productos que ya están en la compra
    const productosDisponibles = productos.filter(p =>
        !detallesCompra.some(d => d.idProducto === p.id)
    );

    if (!termino) {
        mostrarProductos(productosDisponibles);
        return;
    }

    const filtrados = productosDisponibles.filter(p =>
        p.codigo.toLowerCase().includes(termino.toLowerCase()) ||
        p.nombre.toLowerCase().includes(termino.toLowerCase())
    );

    mostrarProductos(filtrados);
}


// Cargar productos desde la API
function cargarProductos() {
    mostrarSpinner();

    fetch('/api/Producto/GetAll')
        .then(response => response.json())
        .then(data => {
            // Asegurarse de que todos los productos tengan categoriaNombre
            productos = data.map(p => ({
                ...p,
                categoriaNombre: p.categoriaNombre || 'Sin categoría'
            }));

            // Filtrar para mostrar solo productos no agregados aún a la compra
            buscarProductos('');
            ocultarSpinner();
        })
        .catch(error => {
            console.error('Error al cargar productos:', error);
            mostrarAlerta('error', 'Error', 'No se pudieron cargar los productos.');
            ocultarSpinner();
        });
}

// Cargar proveedores desde la API
function cargarProveedores() {
    mostrarSpinner();

    fetch('/api/Proveedor')
        .then(response => response.json())
        .then(data => {
            proveedores = data;
            mostrarProveedores(proveedores);
            ocultarSpinner();
        })
        .catch(error => {
            console.error('Error al cargar proveedores:', error);
            mostrarAlerta('error', 'Error', 'No se pudieron cargar los proveedores.');
            ocultarSpinner();
        });
}

// Mostrar/ocultar spinner de carga
function mostrarSpinner() {
    if (!$('#spinner-carga').length) {
        $('body').append('<div id="spinner-carga" class="position-fixed top-0 start-0 w-100 h-100 d-flex justify-content-center align-items-center bg-dark bg-opacity-25" style="z-index: 9999;"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></div>');
    }
}

function ocultarSpinner() {
    $('#spinner-carga').remove();
}

// Mostrar productos en el modal
function mostrarProductos(productos) {
    const tbody = $('#productos-body');
    tbody.empty();

    productos.forEach(producto => {
        // Determinar el color de la fila según el estado del producto y su categoría
        let rowClass = '';
        let estadoTag = '';
        let buttonTitle = 'Seleccionar producto';

        if (!producto.estado) {
            // Producto inactivo
            rowClass = 'table-danger';
            estadoTag = '<span class="badge bg-danger">Inactivo</span>';
            buttonTitle = 'Producto inactivo - Hacer clic para más información';
        } else if (!producto.categoriaActiva) {
            // Categoría inactiva
            rowClass = 'table-warning';
            estadoTag = '<span class="badge bg-warning text-dark">Categoría Inactiva</span>';
            buttonTitle = 'Categoría inactiva - Hacer clic para más información';
        } else {
            // Todo normal
            estadoTag = '<span class="badge bg-success">Activo</span>';
        }

        // Obtener ruta de imagen
        let imagenUrl = '/images/productos/default.png'; // Imagen por defecto
        if (producto.imagenUrl && producto.imagenUrl.trim() !== '') {
            imagenUrl = producto.imagenUrl;
            // Verificar si la ruta es relativa y añadir la base si es necesario
            if (!imagenUrl.startsWith('/') && !imagenUrl.startsWith('http')) {
                imagenUrl = '/' + imagenUrl;
            }
        }

        // Mostrar información de stock
        let stockInfo = producto.stock <= 0 ?
            `<span class="badge bg-secondary">${producto.stock}</span>` :
            producto.stock;

        // Todos los botones ahora son clicables (no se usa disabled)
        const fila = `
            <tr class="${rowClass}">
                <td>${producto.codigo}</td>
                <td>
                    <div class="d-flex align-items-center">
                        <img src="${imagenUrl}" alt="${producto.nombre}" 
                            class="img-thumbnail producto-thumbnail me-2" 
                            data-nombre="${producto.nombre}" 
                            style="width: 40px; height: 40px; object-fit: cover;">
                        <span>${producto.nombre}</span>
                    </div>
                </td>
                <td>${producto.categoriaNombre || 'Sin categoría'}</td>
                <td>${stockInfo}</td>
                <td>Q ${producto.precioDeCompra.toFixed(2)}</td>
                <td>Q ${producto.precioDeVenta.toFixed(2)}</td>
                <td>
                    ${estadoTag}
                    <button type="button" class="btn btn-sm btn-primary ms-2" 
                            title="${buttonTitle}"
                            onclick="seleccionarProducto(${producto.id}, '${producto.codigo}', '${producto.nombre.replace("'", "\\'")}', 
                                    ${producto.precioDeCompra}, ${producto.precioDeVenta}, ${producto.estado}, ${producto.categoriaActiva})">
                        <i class="fas fa-plus"></i> Seleccionar
                    </button>
                </td>
            </tr>
        `;
        tbody.append(fila);
    });

    // Configurar imágenes ampliables después de cargar los productos
    setTimeout(() => configurarImagenesAmpliables(), 50);
}

// Mostrar proveedores en el modal
function mostrarProveedores(proveedores) {
    const tbody = $('#proveedores-body');
    tbody.empty();

    proveedores.forEach(proveedor => {
        // Determinar el color de la fila según el estado del proveedor
        let rowClass = '';
        let estadoTag = '';

        if (!proveedor.estado) {
            // Proveedor inactivo
            rowClass = 'table-danger';
            estadoTag = '<span class="badge bg-danger">Inactivo</span>';
        } else {
            // Proveedor activo
            estadoTag = '<span class="badge bg-success">Activo</span>';
        }

        const fila = `
            <tr class="${rowClass}">
                <td>${proveedor.nombre}</td>
                <td>${proveedor.email || '-'}</td>
                <td>${proveedor.telefono || '-'}</td>
                <td>
                    ${estadoTag}
                    <button type="button" class="btn btn-primary btn-sm ms-2" 
                            onclick="seleccionarProveedor(${proveedor.id}, '${proveedor.nombre.replace(/'/g, "\\'")}', ${proveedor.estado})">
                        <i class="fas fa-check"></i> Seleccionar
                    </button>
                </td>
            </tr>
        `;
        tbody.append(fila);
    });
}

// Seleccionar un producto para agregarlo a la compra
function seleccionarProducto(id, codigo, nombre, precioCompra, precioVenta, estado, categoriaActiva) {
    console.log("Datos del producto seleccionado:");
    console.log("ID:", id);
    console.log("Código:", codigo);
    console.log("Nombre:", nombre);
    console.log("Estado del producto:", estado, typeof estado);
    console.log("Estado de la categoría:", categoriaActiva, typeof categoriaActiva);

    // Verificar si el producto está activo
    if (estado === false) {
        Swal.fire({
            title: 'Advertencia: Producto Inactivo',
            text: 'Este producto está marcado como inactivo. Por lo tanto, este producto no se puede agregar a la compra. Escoja otro producto.',
            icon: 'error',
            confirmButtonText: 'Aceptar',
            confirmButtonColor: '#6c757d'
        })
        return;
    }

    // Verificar si la categoría está activa
    if (categoriaActiva === false || categoriaActiva === "false" || categoriaActiva === 0 || categoriaActiva === null || categoriaActiva === undefined) {
        Swal.fire({
            title: 'Advertencia: Categoría Inactiva',
            text: 'La categoría de este producto está inactiva. Por lo tanto, este producto no se puede agregar a la compra. Escoja otro producto.',
            icon: 'warning',
            confirmButtonText: 'Aceptar',
            confirmButtonColor: '#6c757d'
        })
        return;
    }

    // Obtener la URL de la imagen del producto seleccionado
    const productoSeleccionado = productos.find(p => p.id === id);
    let imagenUrl = '/images/productos/default.png';
    let categoriaNombre = 'Sin categoría';

    if (productoSeleccionado) {
        if (productoSeleccionado.imagenUrl) {
            imagenUrl = productoSeleccionado.imagenUrl.startsWith('/') ?
                productoSeleccionado.imagenUrl :
                '/' + productoSeleccionado.imagenUrl;
        }

        // Asegurarse de obtener el nombre de la categoría
        categoriaNombre = productoSeleccionado.categoriaNombre || 'Sin categoría';
    }

    // Si todo está bien, continuar con la selección directamente
    continuarSeleccionProducto(id, codigo, nombre, precioCompra, precioVenta, imagenUrl, categoriaNombre);
}

// Continuar con la selección del producto
function continuarSeleccionProducto(id, codigo, nombre, precioCompra, precioVenta, imagenUrl, categoriaNombre) {
    $('#producto-id').val(id);
    $('#producto-codigo').val(codigo);
    $('#producto-nombre').val(nombre);
    $('#producto-info').val(`${codigo} - ${nombre}`);
    $('#precio-compra').val(precioCompra.toFixed(2));
    $('#precio-venta').val(precioVenta.toFixed(2));
    $('#cantidad').val(1);

    // Guardar la URL de la imagen para usarla más tarde
    $('#detalleProductoModal').data('imagen-url', imagenUrl);

    $('#detalleProductoModal').data('categoria', categoriaNombre);

    calcularSubtotal();

    $('#productoModal').modal('hide');
    $('#detalleProductoModal').modal('show');
}

// Seleccionar un proveedor
function seleccionarProveedor(id, nombre, estado) {
    // Verificar si el proveedor está activo
    if (!estado) {
        Swal.fire({
            title: 'Proveedor inactivo',
            text: 'Este proveedor está marcado como inactivo. No se permite realizar compras con este proveedor.',
            icon: 'error',
            confirmButtonText: 'Entendido',
            confirmButtonColor: '#6c757d'
        });
        return;
    }

    // Si está activo, seleccionarlo directamente
    $('#proveedorSelect').val(id);
    $('#proveedorModal').modal('hide');
}

// Agregar o actualizar detalle en la tabla de compra
function agregarDetalle() {
    const id = parseInt($('#producto-id').val());
    const codigo = $('#producto-codigo').val();
    const nombre = $('#producto-nombre').val();
    const precioCompra = parseFloat($('#precio-compra').val());
    const precioVenta = parseFloat($('#precio-venta').val());
    const cantidad = parseInt($('#cantidad').val());
    const subtotal = precioCompra * cantidad;

    // Verificar si estamos editando
    const editIndex = $('#detalleProductoModal').data('edit-index');
    const isEditing = editIndex !== undefined;

    // Validaciones básicas
    if (!id || !precioCompra || !precioVenta || !cantidad) {
        mostrarAlerta('error', 'Error', 'Por favor complete todos los campos requeridos.');
        $('#producto-id, #precio-compra, #precio-venta, #cantidad').addClass('is-invalid');
        return;
    } else {
        $('#producto-id, #precio-compra, #precio-venta, #cantidad').removeClass('is-invalid');
    }

    if (precioCompra <= 0 || precioVenta <= 0 || cantidad <= 0) {
        mostrarAlerta('error', 'Error', 'Los precios y cantidad deben ser mayores que cero.');
        $('#precio-compra, #precio-venta, #cantidad').addClass('is-invalid');
        return;
    } else {
        $('#precio-compra, #precio-venta, #cantidad').removeClass('is-invalid');
    }

    if (precioVenta <= precioCompra) {
        Swal.fire({
            icon: 'warning',
            title: 'Verificar precios',
            text: 'El precio de venta es igual o menor que el precio de compra. ¿Desea continuar?',
            showCancelButton: true,
            confirmButtonText: 'Sí, continuar',
            cancelButtonText: 'Cancelar'
        }).then((result) => {
            if (result.isConfirmed) {
                procesarDetalle(id, codigo, nombre, precioCompra, precioVenta, cantidad, subtotal, isEditing, editIndex);
            }
        });
    } else {
        procesarDetalle(id, codigo, nombre, precioCompra, precioVenta, cantidad, subtotal, isEditing, editIndex);
    }

    formularioModificado = true;
}

function procesarDetalle(id, codigo, nombre, precioCompra, precioVenta, cantidad, subtotal, isEditing, editIndex) {
    // Obtener la URL de imagen guardada en el modal
    const imagenUrl = $('#detalleProductoModal').data('imagen-url') || '/images/productos/default.png';
    // Obtener la categoría guardada en el modal
    const categoria = $('#detalleProductoModal').data('categoria') || 'Sin Categoría';

    if (isEditing) {
        // Actualizar detalle existente
        detallesCompra[editIndex] = {
            idProducto: id,
            codigo: codigo,
            nombre: nombre,
            categoria: categoria,
            precioDeCompra: precioCompra,
            precioDeVenta: precioVenta,
            cantidad: cantidad,
            subtotal: subtotal,
            imagenUrl: imagenUrl 
        };

        // Resetear el estado del modal
        $('#detalleProductoModal').removeData('edit-index');
        $('#detalleProductoModalLabel').text('Agregar Producto');
        $('#btn-agregar-detalle').text('Agregar');

    } else {
        // Es un nuevo detalle - verificar si ya existe
        const indiceExistente = detallesCompra.findIndex(d => d.idProducto === id);
        if (indiceExistente >= 0) {
            Swal.fire({
                icon: 'question',
                title: 'Producto Duplicado',
                text: 'Este producto ya está en la lista de compra. ¿Desea actualizar los datos?',
                showCancelButton: true,
                confirmButtonText: 'Sí, actualizar',
                cancelButtonText: 'Cancelar'
            }).then((result) => {
                if (result.isConfirmed) {
                    detallesCompra[indiceExistente].precioDeCompra = precioCompra;
                    detallesCompra[indiceExistente].precioDeVenta = precioVenta;
                    detallesCompra[indiceExistente].cantidad = cantidad;
                    detallesCompra[indiceExistente].subtotal = subtotal;
                    actualizarTablaDetalles();
                }
            });
            $('#detalleProductoModal').modal('hide');
            return;
        }

        // Agregar nuevo detalle
        const detalle = {
            idProducto: id,
            codigo: codigo,
            nombre: nombre,
            categoria: categoria,
            precioDeCompra: precioCompra,
            precioDeVenta: precioVenta,
            cantidad: cantidad,
            subtotal: subtotal,
            imagenUrl: imagenUrl 
        };

        detallesCompra.push(detalle);
    }

    // Limpiar datos del modal
    $('#detalleProductoModal').removeData('imagen-url');
    $('#detalleProductoModal').removeData('categoria');

    actualizarTablaDetalles();
    $('#detalleProductoModal').modal('hide');

    // Habilitar el botón de guardar si hay al menos un detalle
    if (detallesCompra.length > 0) {
        $('#btn-guardar-compra').prop('disabled', false);
    }

    // Recargar productos para reflejar el cambio
    buscarProductos('');
}

// Actualizar la tabla de detalles de compra
function actualizarTablaDetalles() {
    const tabla = $('#detalles-body');
    tabla.empty();

    let total = 0;

    detallesCompra.forEach((detalle, index) => {
        total += detalle.subtotal;

        // Obtener la URL de imagen
        let imagenUrl = detalle.imagenUrl || '/images/productos/default.png';

        // Si no hay imagen guardada en el detalle, buscar en el array de productos
        if (!imagenUrl || imagenUrl === '/images/productos/default.png') {
            const productoCompleto = productos.find(p => p.id === detalle.idProducto);
            if (productoCompleto && productoCompleto.imagenUrl) {
                imagenUrl = productoCompleto.imagenUrl.startsWith('/') ?
                    productoCompleto.imagenUrl :
                    '/' + productoCompleto.imagenUrl;
            }
        }

        tabla.append(`
            <tr>
                <td>${detalle.codigo}</td>
                <td>
                    <div class="d-flex align-items-center">
                        <img src="${imagenUrl}" alt="${detalle.nombre}" 
                            class="img-thumbnail me-2" 
                            style="width: 40px; height: 40px; object-fit: cover;"
                            onerror="this.src='/images/productos/default.png'">
                        <div>
                            <span>${detalle.nombre}</span>
                            <br>
                            <small class="text-muted">${detalle.categoria}</small>
                        </div>                        
                    </div>
                </td>
                <td>Q ${detalle.precioDeCompra.toFixed(2)}</td>
                <td>Q ${detalle.precioDeVenta.toFixed(2)}</td>
                <td>${detalle.cantidad}</td>
                <td>Q ${detalle.subtotal.toFixed(2)}</td>
                <td>
                    <button type="button" class="btn btn-primary btn-sm me-1" onclick="editarDetalle(${index})">
                        <i class="fas fa-edit"></i>
                    </button>
                    <button type="button" class="btn btn-danger btn-sm" onclick="eliminarDetalle(${index})">
                        <i class="fas fa-trash"></i>
                    </button>
                </td>
            </tr>
        `);
    });

    // Actualizar totales
    $('#total-compra').text(`Q ${total.toFixed(2)}`);

    configurarImagenesAmpliables();
}

// Editar un detalle de la compra
function editarDetalle(index) {
    // Obtener el detalle a editar
    const detalle = detallesCompra[index];

    // Llenar el modal con los datos actuales del detalle
    $('#producto-id').val(detalle.idProducto);
    $('#producto-codigo').val(detalle.codigo);
    $('#producto-nombre').val(detalle.nombre);
    $('#producto-info').val(`${detalle.codigo} - ${detalle.nombre}`);
    $('#precio-compra').val(detalle.precioDeCompra.toFixed(2));
    $('#precio-venta').val(detalle.precioDeVenta.toFixed(2));
    $('#cantidad').val(detalle.cantidad);

    // Guardar el índice que se está editando
    $('#detalleProductoModal').data('edit-index', index);

    // Configurar la URL de la imagen
    const imagenUrl = detalle.imagenDelProducto || '/images/productos/default.png';
    $('#detalleProductoModal').data('imagen-url', imagenUrl);

    calcularSubtotal();

    // Cambiar el título del modal, el texto del botón y su icono
    $('#detalleProductoModalLabel').text('Editar Producto');
    $('#btn-agregar-detalle').text('Actualizar');
    $('#btn-agregar-detalle').html('<i class="fas fa-save"></i> Actualizar');

    // Mostrar el modal
    $('#detalleProductoModal').modal('show');
}

// Eliminar un detalle de la compra
function eliminarDetalle(index) {
    Swal.fire({
        icon: 'question',
        title: '¿Eliminar producto?',
        text: '¿Está seguro de eliminar este producto de la compra?',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#dc3545'
    }).then((result) => {
        if (result.isConfirmed) {
            detallesCompra.splice(index, 1);
            actualizarTablaDetalles();

            // Deshabilitar el botón de guardar si no hay detalles
            if (detallesCompra.length === 0) {
                $('#btn-guardar-compra').prop('disabled', true);
            }
        }
    });
}

// Función para configurar botones de cantidad
function configurarBotonesCantidad() {
    setupCantidadButtons();
}

// Función para manejar los eventos de los botones de cantidad
function setupCantidadButtons() {
    const decreaseBtn = document.getElementById('decreaseCantidad');
    const increaseBtn = document.getElementById('increaseCantidad');
    const cantidadInput = document.getElementById('cantidad');

    if (decreaseBtn && increaseBtn && cantidadInput) {
        // Remover listeners previos para evitar duplicados (usando clonación)
        const newDecreaseBtn = decreaseBtn.cloneNode(true);
        const newIncreaseBtn = increaseBtn.cloneNode(true);

        decreaseBtn.parentNode.replaceChild(newDecreaseBtn, decreaseBtn);
        increaseBtn.parentNode.replaceChild(newIncreaseBtn, increaseBtn);

        // Agregar nuevos listeners
        newDecreaseBtn.addEventListener('click', function() {
            const currentValue = parseInt(cantidadInput.value) || 1;
            if (currentValue > 1) {
                cantidadInput.value = currentValue - 1;
                calcularSubtotal(); // Calcular el subtotal después de cambiar la cantidad
            }
        });

        newIncreaseBtn.addEventListener('click', function() {
            const currentValue = parseInt(cantidadInput.value) || 1;
            cantidadInput.value = currentValue + 1;
            calcularSubtotal(); // Calcular el subtotal después de cambiar la cantidad
        });

        // También actualizar el subtotal cuando se cambia manualmente el valor
        cantidadInput.addEventListener('input', calcularSubtotal);
    }
}

// Preparar datos para enviar al servidor
function prepararEnvio(e) {
    e.preventDefault();

    if (detallesCompra.length === 0) {
        mostrarAlerta('error', 'Error', 'Debe agregar al menos un producto a la compra.');
        return false;
    }

    const numeroFactura = $('#NumeroDeFactura').val();
    const idProveedor = $('#proveedorSelect').val();

    if (!numeroFactura) {
        mostrarAlerta('error', 'Error', 'El número de factura es obligatorio.');
        // colocar el campo con estilo de invalido
        $('#NumeroDeFactura').addClass('is-invalid');
        return false;
    } else {
        // remover el campo con estilo invalido
        $('#NumeroDeFactura').removeClass('is-invalid');
    }

    if (!idProveedor) {
        $('#proveedorSelect').addClass('is-invalid');
        mostrarAlerta('error', 'Error', 'El proveedor es obligatorio.');
        return false;
    } else {
        $('#proveedorSelect').removeClass('is-invalid');
    }

    // Verificar si el proveedor está activo
    const proveedorActivo = verificarProveedorActivo(idProveedor);
    if (!proveedorActivo) {
        mostrarAlerta('error', 'Error', 'El proveedor seleccionado está inactivo. No se permite realizar compras con proveedores inactivos.');
        $('#proveedorSelect').addClass('is-invalid');
        return false;
    } else {
        $('#proveedorSelect').removeClass('is-invalid');
    }

    // Mostrar confirmación antes de guardar
    Swal.fire({
        icon: 'question',
        title: 'Confirmar compra',
        text: `¿Está seguro de registrar la compra con factura ${numeroFactura}?`,
        showCancelButton: true,
        confirmButtonText: 'Sí, guardar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            enviarCompra(numeroFactura, idProveedor);
        }
    });

    return false;
}

// Función para verificar si un proveedor está activo
function verificarProveedorActivo(idProveedor) {
    // Encontrar el proveedor en el array global de proveedores
    const proveedor = proveedores.find(p => p.id == idProveedor);

    // Verificar si existe y está activo
    return proveedor && proveedor.estado === true;
}

// Enviar datos de la compra al servidor
function enviarCompra(numeroFactura, idProveedor) {
    mostrarSpinner();

    // Crear objeto para enviar al servidor
    const compraData = {
        numeroDeFactura: numeroFactura,
        idProveedor: parseInt(idProveedor),
        fechaDeRegistro: document.getElementById('fechaDeRegistro').value,
        detallesDeLaCompra: detallesCompra.map(d => ({
            idProducto: d.idProducto,
            precioDeCompra: d.precioDeCompra,
            precioDeVenta: d.precioDeVenta,
            cantidad: d.cantidad
        }))
    };

    // Enviar datos mediante fetch
    fetch('/api/Compra/Create', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(compraData)
    })
        .then(response => {
            if (!response.ok) {
                return response.json().then(err => { throw err; });
            }
            return response.json();
        })
        .then(() => {
            ocultarSpinner();
            formularioModificado = false;
            Swal.fire({
                icon: 'success',
                title: 'Éxito',
                text: 'Compra registrada correctamente.',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                // Desactivar advertencia de navegacion
                window.onbeforeunload = null;
                window.location.href = '/Compras/Index';
            });
        })
        .catch(error => {
            ocultarSpinner();
            console.error('Error al registrar la compra:', error);
            mostrarAlerta('error', 'Error', `Error al registrar la compra: ${error.message || 'Revise los datos ingresados'}`);
        });
}

// Función para editar una compra existente
function editarCompra() {
    const id = $('#compraId').val();
    const numeroFactura = $('#numeroFactura').val();
    const idProveedor = $('#proveedorSelect').val();
    const proveedorOriginalId = $('#proveedorOriginalId').val();

    // Obtener el botón de submit
    const submitButton = document.querySelector('#formEditarCompra button[type="submit"]');
    const originalText = submitButton.innerHTML;

    if (!numeroFactura) {
        mostrarAlerta('error', 'Error', 'El número de factura es obligatorio.');
        // colocar el campo con estilo de invalido
        $('#numeroFactura').addClass('is-invalid');
        return false;
    } else {
        // remover el campo con estilo invalido
        $('#numeroFactura').removeClass('is-invalid');
    }

    if (!idProveedor) {
        $('#proveedorSelect').addClass('is-invalid');
        mostrarAlerta('error', 'Error', 'El proveedor es obligatorio.');
        return false;
    } else {
        $('#proveedorSelect').removeClass('is-invalid');
    }

    // Verificar si el proveedor está activo (con excepción si es el mismo proveedor original)
    const proveedorActivo = verificarProveedorActivo(idProveedor);
    if (!proveedorActivo && idProveedor != proveedorOriginalId) {
        mostrarAlerta('error', 'Error', 'No se permite cambiar a un proveedor inactivo. Seleccione un proveedor activo o mantenga el proveedor original.');
        $('#proveedorSelect').addClass('is-invalid');
        return;
    } else {
        $('#proveedorSelect').removeClass('is-invalid');
    }

    // Verificar si es un número duplicado de factura (excluyendo la factura actual)
    verificarFacturaDuplicadaParaEdicion(numeroFactura, id)
        .then(existe => {
            if (existe) {
                mostrarAlerta('warning', 'Advertencia', 'Este número de factura ya existe para otra compra.');
                $('#numeroFactura').addClass('is-invalid');
                return;
            }

            // Si todo está bien, continuar con el proceso de actualización
            $('#numeroFactura').removeClass('is-invalid');

            // Mostrar confirmación antes de guardar los cambios
            Swal.fire({
                icon: 'question',
                title: 'Confirmar cambios',
                text: `¿Está seguro de guardar los cambios en la compra ${numeroFactura}?`,
                showCancelButton: true,
                confirmButtonText: 'Sí, guardar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Deshabilitar botón y mostrar indicador de carga
                    submitButton.disabled = true;
                    submitButton.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';

                    // Crear objeto para enviar al servidor
                    const compraData = {
                        numeroDeFactura: numeroFactura,
                        idProveedor: parseInt(idProveedor),
                        // Añadimos los detalles de la compra para actualización
                        detallesDeLaCompra: detallesCompra.map(d => ({
                            id: d.detalleId || 0,
                            idProducto: d.idProducto,
                            precioDeCompra: d.precioDeCompra,
                            precioDeVenta: d.precioDeVenta,
                            cantidad: d.cantidad
                        }))
                    };

                    // Enviar datos mediante fetch
                    mostrarSpinner();
                    fetch(`/api/Compra/Edit/${id}`, {
                        method: 'PUT',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(compraData)
                    })
                        .then(response => {
                            if (!response.ok) {
                                return response.text().then(text => {
                                    try {
                                        // Intentar parsear como JSON
                                        const errorData = JSON.parse(text);
                                        throw new Error(errorData.message || 'Error al actualizar la compra');
                                    } catch (e) {
                                        // Si no es JSON, usar el texto plano
                                        throw new Error(text || 'Error al actualizar la compra');
                                    }
                                });
                            }
                            // Intentar parsear la respuesta como JSON si existe
                            const contentType = response.headers.get('content-type');
                            if (contentType && contentType.includes('application/json')) {
                                return response.json();
                            }
                            return null;
                        })
                        .then(() => {
                            ocultarSpinner();
                            Swal.fire({
                                icon: 'success',
                                title: 'Éxito',
                                text: 'Compra actualizada correctamente.',
                                confirmButtonText: 'Aceptar'
                            }).then(() => {
                                window.location.href = '/Compras/Index';
                            });
                        })
                        .catch(error => {
                            ocultarSpinner();
                            console.error('Error al actualizar la compra:', error);
                            mostrarAlerta('error', 'Error', `Error al actualizar la compra: ${error.message || 'Revise los datos ingresados'}`);
                            // Restaurar el botón
                            submitButton.disabled = false;
                            submitButton.innerHTML = originalText;
                        });
                }
            });
        })
        .catch(error => {
            console.error('Error al verificar factura:', error);
            mostrarAlerta('error', 'Error', 'Error al verificar el número de factura.');
        });
}

// Verificar si el número de factura ya existe (excluyendo la factura actual)
function verificarFacturaDuplicadaParaEdicion(numeroFactura, idActual) {
    return new Promise((resolve, reject) => {
        fetch('/api/Compra')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al verificar número de factura');
                }
                return response.json();
            })
            .then(compras => {
                // Filtrar para excluir la compra actual
                const existeFactura = compras.some(compra =>
                    compra.id != idActual &&
                    compra.numeroDeFactura.toLowerCase() === numeroFactura.toLowerCase().trim());

                resolve(existeFactura);
            })
            .catch(error => {
                console.error('Error al verificar factura:', error);
                reject(error);
            });
    });
}

// Cargar compras desde la API
function loadCompras(searchTerm = '') {
    const tableBody = document.getElementById('comprasTableBody');
    if (!tableBody) return;

    // Mostrar indicador de carga
    tableBody.innerHTML = '<tr><td colspan="5" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';

    // Llamada a la API
    fetch('/api/Compra')
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    try {
                        // Intentar parsear como JSON
                        const errorData = JSON.parse(text);
                        throw new Error(`Error del servidor (${response.status}): ${errorData.message || errorData.title || text}`);
                    } catch (e) {
                        // Si no es JSON, usar el texto plano
                        throw new Error(`Error del servidor (${response.status}): ${text || response.statusText}`);
                    }
                });
            }
            return response.json();
        })
        .then(data => {
            console.log("Datos recibidos:", data); // Depuración
            compras = data;

            // Filtrar por término de búsqueda si existe
            if (searchTerm) {
                searchTerm = searchTerm.toLowerCase();
                compras = compras.filter(c =>
                    (c.numeroDeFactura && c.numeroDeFactura.toLowerCase().includes(searchTerm)) ||
                    (c.nombreDelProveedor && c.nombreDelProveedor.toLowerCase().includes(searchTerm))
                );
            }

            // Calcular paginación
            totalPages = Math.ceil(compras.length / pageSize);
            if (currentPage > totalPages && totalPages > 0) {
                currentPage = 1;
            }

            // Mostrar datos
            renderCompras();
            renderPagination();
        })
        .catch(error => {
            console.error('Error al cargar compras:', error);
            tableBody.innerHTML = `<tr><td colspan="5" class="text-center text-danger">
                <div><i class="fas fa-exclamation-triangle me-2"></i>Error al cargar las compras</div>
                <div class="small mt-1">${error.message || "Error desconocido"}</div>
                <button class="btn btn-sm btn-outline-primary mt-2" onclick="loadCompras()">
                    <i class="fas fa-sync-alt me-1"></i>Intentar nuevamente
                </button>
            </td></tr>`;
        });
}

// Renderizar compras en la tabla
function renderCompras() {
    const tableBody = document.getElementById('comprasTableBody');
    if (!tableBody) return;

    tableBody.innerHTML = '';

    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;
    const paginatedCompras = compras.slice(start, end);

    if (paginatedCompras.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="5" class="text-center">No se encontraron compras</td></tr>';
        return;
    }

    paginatedCompras.forEach(compra => {
        const row = document.createElement('tr');

        // Manejo seguro de fechas
        // Usar la función formatearFechaAmPm para formatear correctamente
        const fechaFormateada = formatearFechaAmPm(compra.fechaDeRegistro);


        // Manejo seguro de propiedades potencialmente nulas
        const numeroFactura = compra.numeroDeFactura || '-';

        // Si nombreDelProveedor es "Proveedor no encontrado", muestra un texto especial con estilo
        let proveedorDisplay = compra.nombreDelProveedor || '-';
        if (proveedorDisplay === "Proveedor no encontrado") {
            proveedorDisplay = `<span class="text-warning"><i class="fas fa-exclamation-triangle me-1"></i>${proveedorDisplay}</span>`;
        }

        const montoTotal = typeof compra.montoTotal === 'number' ? `Q ${compra.montoTotal.toFixed(2)}` : 'Q 0.00';

        row.innerHTML = `
            <td>${numeroFactura}</td>
            <td>${proveedorDisplay}</td>
            <td>${fechaFormateada}</td>
            <td>${montoTotal}</td>
            <td>
                <a href="/Compras/Details/${compra.id}" class="btn btn-sm btn-secondary me-1" title="Ver detalles">
                    <i class="fas fa-eye"></i>
                </a>
                <a href="/Compras/Edit/${compra.id}" class="btn btn-sm btn-primary me-1" title="Editar">
                    <i class="fas fa-edit"></i>
                </a>
                <button class="btn btn-sm btn-danger" onclick="confirmarEliminarCompra(${compra.id}, '${numeroFactura}')" title="Eliminar">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;

        tableBody.appendChild(row);
    });
}

// Renderizar paginación
function renderPagination() {
    const pagination = document.getElementById('comprasPagination');
    if (!pagination) return;

    pagination.innerHTML = '';

    if (totalPages <= 1) return;

    // Botón Anterior
    const prevLi = document.createElement('li');
    prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;

    const prevLink = document.createElement('a');
    prevLink.className = 'page-link';
    prevLink.href = '#';
    prevLink.setAttribute('aria-label', 'Anterior');
    prevLink.innerHTML = '<span aria-hidden="true">&laquo;</span>';

    prevLink.addEventListener('click', function(e) {
        e.preventDefault();
        if (currentPage > 1) {
            currentPage--;
            renderCompras();
            renderPagination();
        }
    });

    prevLi.appendChild(prevLink);
    pagination.appendChild(prevLi);

    // Números de página
    const maxPagesToShow = 5;
    const startPage = Math.max(1, currentPage - Math.floor(maxPagesToShow / 2));
    const endPage = Math.min(totalPages, startPage + maxPagesToShow - 1);

    for (let i = startPage; i <= endPage; i++) {
        const pageLi = document.createElement('li');
        pageLi.className = `page-item ${i === currentPage ? 'active' : ''}`;

        const pageLink = document.createElement('a');
        pageLink.className = 'page-link';
        pageLink.href = '#';
        pageLink.textContent = i;

        pageLink.addEventListener('click', function(e) {
            e.preventDefault();
            currentPage = i;
            renderCompras();
            renderPagination();
        });

        pageLi.appendChild(pageLink);
        pagination.appendChild(pageLi);
    }

    // Botón Siguiente
    const nextLi = document.createElement('li');
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;

    const nextLink = document.createElement('a');
    nextLink.className = 'page-link';
    nextLink.href = '#';
    nextLink.setAttribute('aria-label', 'Siguiente');
    nextLink.innerHTML = '<span aria-hidden="true">&raquo;</span>';

    nextLink.addEventListener('click', function(e) {
        e.preventDefault();
        if (currentPage < totalPages) {
            currentPage++;
            renderCompras();
            renderPagination();
        }
    });

    nextLi.appendChild(nextLink);
    pagination.appendChild(nextLi);
}

// Función para confirmar eliminación de compra
function confirmarEliminarCompra(id, numeroFactura) {
    Swal.fire({
        title: '¿Está seguro?',
        text: `¿Desea eliminar la compra ${numeroFactura}? Esta acción disminuirá el stock de los productos.`,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            eliminarCompra(id);
        }
    });
}

// Eliminar compra
function eliminarCompra(id) {
    fetch(`/api/Compra/Delete/${id}`, {
        method: 'DELETE'
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al eliminar la compra');
            }
            mostrarAlerta('success', 'Exito', 'Compra eliminada correctamente');
            // Verificar si estamos en la página de detalles o en la lista
            if (window.location.pathname.includes('/Compras/Details/')) {
                // Si estamos en detalles, redirigir al índice después de eliminar
                setTimeout(() => {
                    window.location.href = '/Compras/Index';
                }, 1000);
            } else {
                // Si estamos en el índice, simplemente recargar la lista
                loadCompras();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta('error', 'Error', `Error al eliminar la compra: ${error.message}`);
        });
}

// Función para limpiar el formulario de creación de compra
function limpiarFormularioCompra() {
    // Regenerar el número de factura
    generarNumeroFacturaUnico().then(numeroFactura => {
        $('#NumeroDeFactura').val(numeroFactura);
        $('#NumeroDeFactura').removeClass('is-invalid');
    });

    // Resetear el selector de proveedor
    $('#proveedorSelect').val('');
    $('#proveedorSelect').removeClass('is-invalid');

    // Limpiar la tabla de detalles
    detallesCompra = [];
    actualizarTablaDetalles();

    // Deshabilitar el botón guardar
    $('#btn-guardar-compra').prop('disabled', true);

    // Mostrar mensaje de confirmación
    mostrarAlerta('success', 'Listo', 'Formulario limpiado correctamente.');

    // Resetear estado de cambios
    formularioModificado = false;
}

// función guardarDatosOriginalesCompra para incluir los detalles
function guardarDatosOriginalesCompra() {
    compraOriginal = {
        numeroFactura: $('#numeroFactura').val(),
        proveedorId: $('#proveedorSelect').val(),
        detalles: window.detallesCompraOriginal ? [...window.detallesCompraOriginal] : []
    };
}

// función restaurarDatosCompra para incluir los detalles originales
function restaurarDatosCompra() {
    if (!compraOriginal) return;

    // Restaurar número de factura
    const numeroFacturaInput = document.getElementById('numeroFactura');
    if (numeroFacturaInput) {
        const numeroFacturaOriginal = numeroFacturaInput.getAttribute('data-original-factura') || compraOriginal.numeroFactura;
        numeroFacturaInput.value = numeroFacturaOriginal;
        numeroFacturaInput.classList.remove('is-invalid');
    }

    // Restaurar proveedor
    $('#proveedorSelect').val(compraOriginal.proveedorId);
    $('#proveedorSelect').removeClass('is-invalid');

    // Restaurar detalles de la compra si estamos en modo edición
    if (window.detallesCompraOriginal) {
        cargarDetallesExistentes(window.detallesCompraOriginal);
    }

    // Mostrar mensaje de confirmación
    mostrarAlerta('info', 'Restaurado', 'Se han restaurado los datos originales.');

    // Resetear estado de cambios
    formularioModificado = false;
}

// Detectar cambios en el formulario de edición
function detectarCambiosEditarCompra() {
    if (!compraOriginal) return false;

    const numeroFacturaActual = $('#numeroFactura').val();
    const proveedorIdActual = $('#proveedorSelect').val();

    return numeroFacturaActual !== compraOriginal.numeroFactura ||
        proveedorIdActual !== compraOriginal.proveedorId;
}

// Monitorear cambios en el formulario de edición
function monitorearCambiosEditarCompra() {
    $('#numeroFactura, #proveedorSelect').on('input change', function() {
        formularioModificado = detectarCambiosEditarCompra();
    });
}

// Función para interceptar clics en enlaces
function setupLinkInterception() {
    // Variable para controlar si la navegación fue aprobada
    let navegacionAprobada = false;

    // Interceptar solo enlaces de navegación real (excluir los que abren submenús)
    const enlaces = document.querySelectorAll('a[href]:not([href^="#"]):not([href^="javascript"])');

    enlaces.forEach(enlace => {
        enlace.addEventListener('click', function(e) {
            // Ignorar si es un evento de menú de sidebar (para desplegar/colapsar submenús)
            if (e.isSidebarMenuEvent ||
                (enlace.closest('.submenu') && enlace.getAttribute('href').startsWith('#'))) {
                return true;
            }

            // Verificar si hay cambios sin guardar y que el enlace tenga un href válido
            if (formularioModificado && !navegacionAprobada && this.getAttribute('href')) {
                e.preventDefault();

                // Guardar URL destino
                const urlDestino = this.getAttribute('href');

                // Mostrar SweetAlert para confirmar
                Swal.fire({
                    title: 'Confirmar salida',
                    text: 'Hay cambios sin guardar. ¿Está seguro que desea salir sin guardar los cambios?',
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonText: 'Salir sin guardar',
                    cancelButtonText: 'Cancelar',
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#3085d6'
                }).then((result) => {
                    if (result.isConfirmed) {
                        // Desactivar la alerta del navegador temporalmente
                        window.onbeforeunload = null;
                        // Aprobar navegación y redirigir
                        navegacionAprobada = true;
                        window.location.href = urlDestino;
                    }
                });
            }
        });
    });

    // Interceptar al cerrar la ventana o cambiar de página
    // window.onbeforeunload = function (e) {
    //     if (formularioModificado && !navegacionAprobada) {
    //         // Mensaje estándar del navegador
    //         const mensaje = 'Hay cambios sin guardar. ¿Está seguro que desea salir sin guardar los cambios?';
    //         e.returnValue = mensaje;
    //         return mensaje;
    //     }
    // };
}
