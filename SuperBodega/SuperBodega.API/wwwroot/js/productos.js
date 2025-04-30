// Variables globales para control de cambios
let productoOriginal = null;
let formHasChanges = false;
let currentPage = 1;
const pageSize = 10;
let totalPages = 0;
let productos = [];

// Función para detectar cambios en el formulario de producto
function detectarCambiosProducto() {
    if (!productoOriginal) return false;

    const codigoActual = document.getElementById('Codigo').value.trim();
    const nombreActual = document.getElementById('Nombre').value.trim();
    const categoriaIdActual = document.getElementById('CategoriaId').value;
    const descripcionActual = document.getElementById('Descripcion')?.value.trim() || '';
    const estadoActual = document.querySelector('input[name="Estado"]')?.checked;

    // Verificar si se seleccionó una nueva imagen
    const imagenInput = document.getElementById('imagen');
    const nuevaImagenSeleccionada = imagenInput && imagenInput.files && imagenInput.files.length > 0;

    return codigoActual !== productoOriginal.codigo ||
        nombreActual !== productoOriginal.nombre ||
        parseInt(categoriaIdActual) !== productoOriginal.categoriaId ||
        descripcionActual !== (productoOriginal.descripcion || '') ||
        estadoActual !== productoOriginal.estado ||
        nuevaImagenSeleccionada;
}

// Monitorear cambios en tiempo real
function monitorearCambiosProducto() {
    const elementos = ['Codigo', 'Nombre', 'CategoriaId', 'Descripcion', 'imagen'];
    elementos.forEach(id => {
        const elemento = document.getElementById(id);
        if (!elemento) return;

        if (elemento.type === 'checkbox') {
            elemento.addEventListener('change', () => {
                formHasChanges = detectarCambiosProducto();
            });
        } else if (elemento.type === 'file') {
            elemento.addEventListener('change', () => {
                formHasChanges = detectarCambiosProducto();
                // Si se selecciona una imagen, desmarcamos "mantener imagen"
                const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
                if (mantenerImagenCheckbox && elemento.files && elemento.files.length > 0) {
                    mantenerImagenCheckbox.checked = false;
                }
            });
        } else {
            ['input', 'change', 'keyup'].forEach(evento => {
                elemento.addEventListener(evento, () => {
                    formHasChanges = detectarCambiosProducto();
                });
            });
        }
    });

    // Monitorear cambios en el checkbox de estado
    const estadoCheckbox = document.querySelector('input[name="Estado"]');
    if (estadoCheckbox) {
        estadoCheckbox.addEventListener('change', () => {
            formHasChanges = detectarCambiosProducto();
        });
    }

    // Monitorear cambios en el checkbox de mantener imagen
    const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
    if (mantenerImagenCheckbox) {
        mantenerImagenCheckbox.addEventListener('change', () => {
            formHasChanges = detectarCambiosProducto();
        });
    }
}

// Función para interceptar clics en enlaces
function setupLinkInterception() {
    // Variable para controlar si la navegación fue aprobada
    let navegacionAprobada = false;

    // Interceptar solo enlaces de navegación real (excluir los que abren submenús)
    const enlaces = document.querySelectorAll('a[href]:not([href^="#"]):not([href^="javascript"])');

    enlaces.forEach(enlace => {
        enlace.addEventListener('click', function (e) {
            // Ignorar si es un evento de menú de sidebar (para desplegar/colapsar submenús)
            if (e.isSidebarMenuEvent ||
                (enlace.closest('.submenu') && enlace.getAttribute('href').startsWith('#'))) {
                return true;
            }

            // Verificar si hay cambios sin guardar y que el enlace tenga un href válido
            if (formHasChanges && !navegacionAprobada && this.getAttribute('href')) {
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
    window.onbeforeunload = function (e) {
        if (formHasChanges && !navegacionAprobada) {
            // Mensaje estándar del navegador
            const mensaje = 'Hay cambios sin guardar. ¿Está seguro que desea salir sin guardar los cambios?';
            e.returnValue = mensaje;
            return mensaje;
        }
    };
}

// Nueva función para detectar cambios en el formulario de creación
function setupFormCreacionChangesDetection() {
    const elementos = ['Codigo', 'Nombre', 'CategoriaId', 'Descripcion', 'imagen'];

    elementos.forEach(id => {
        const elemento = document.getElementById(id);
        if (!elemento) return;

        if (elemento.type === 'file') {
            elemento.addEventListener('change', () => {
                formHasChanges = true;
            });
        } else {
            ['input', 'change', 'keyup'].forEach(evento => {
                elemento.addEventListener(evento, () => {
                    formHasChanges = true;
                });
            });
        }
    });

    // Monitorear cambios en el checkbox de estado
    const estadoCheckbox = document.querySelector('input[name="Estado"]');
    if (estadoCheckbox) {
        estadoCheckbox.addEventListener('change', () => {
            formHasChanges = true;
        });
    }
}

// Función para formatear fechas en formato AM/PM con manejo de zona horaria
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

// Inicializar campo de fecha de registro en el formulario
function inicializarCampoFechaRegistro() {
    // Para formulario de creación y edición
    const fechaInput = document.getElementById('fechaDeRegistro');
    const fechaVisibleInput = document.getElementById('fechaDeRegistroVisible');

    if (fechaInput && fechaVisibleInput) {
        // Verificar si estamos en el formulario de edición
        const formEditarProducto = document.getElementById('formEditarProducto');
        if (formEditarProducto) {
            // Obtener la fecha de registro del producto desde la página
            const fechaProducto = document.querySelector('img.img-thumbnail')?.getAttribute('data-fecha-registro');

            if (fechaProducto) {
                // Usar la fecha del producto existente
                fechaInput.value = new Date(fechaProducto).toISOString().slice(0, 16);
                fechaVisibleInput.value = formatearFechaAmPm(fechaProducto);
                return;
            }
        }

        // Si no es edición o no hay fecha, establecer la fecha actual
        const now = new Date();

        // Formatear para el input datetime-local (yyyy-MM-ddThh:mm)
        const año = now.getFullYear();
        const mes = (now.getMonth() + 1).toString().padStart(2, '0');
        const dia = now.getDate().toString().padStart(2, '0');
        const horas = now.getHours().toString().padStart(2, '0');
        const minutos = now.getMinutes().toString().padStart(2, '0');

        // Formato para el campo hidden
        fechaInput.value = `${año}-${mes}-${dia}T${horas}:${minutos}`;

        // Formato legible para el campo visible
        fechaVisibleInput.value = formatearFechaAmPm(now);
    }
}

// Función para mostrar vista previa de la imagen seleccionada
function setupImagePreview() {
    const imagenInput = document.getElementById('imagen');
    if (!imagenInput) return;

    const oldContainer = document.getElementById('image-preview-container');
    if (oldContainer) {
        oldContainer.remove();
    }

    const previewContainer = document.createElement('div');
    previewContainer.id = 'image-preview-container';
    previewContainer.className = 'mt-3';
    previewContainer.style.display = 'none';
    previewContainer.style.position = 'relative';

    const previewImage = document.createElement('img');
    previewImage.id = 'image-preview';
    previewImage.className = 'img-thumbnail';
    previewImage.style.maxHeight = '150px';
    previewImage.alt = 'Vista previa';
    previewImage.style.cursor = 'pointer';
    previewImage.title = 'Clic para ampliar';

    // Agregar evento de clic para ampliar la imagen
    previewImage.addEventListener('click', function (e) {
        // Evitar la propagación del evento si se hace clic en el botón de eliminar
        if (!e.target.closest('#clear-image-button')) {
            mostrarImagenAmpliada(this.src, 'Vista previa');
        }
    });

    previewContainer.appendChild(previewImage);

    const inputGroup = imagenInput.closest('.input-group');
    if (inputGroup && inputGroup.parentNode) {
        inputGroup.parentNode.insertBefore(previewContainer, inputGroup.nextSibling);
    }

    imagenInput.addEventListener('change', function () {
        const previewImage = document.getElementById('image-preview');

        if (this.files && this.files[0]) {
            const reader = new FileReader();

            reader.onload = function (e) {
                previewImage.src = e.target.result;
                previewContainer.style.display = 'block';

                const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
                if (mantenerImagenCheckbox) {
                    mantenerImagenCheckbox.checked = false;
                }
            };

            reader.readAsDataURL(this.files[0]);

            let clearButton = document.getElementById('clear-image-button');
            if (!clearButton) {
                clearButton = document.createElement('button');
                clearButton.id = 'clear-image-button';
                clearButton.type = 'button';
                clearButton.className = 'btn btn-sm btn-danger position-absolute';
                clearButton.style.top = '10px';
                clearButton.style.right = '10px';
                clearButton.innerHTML = '<i class="bi bi-x-lg"></i>';
                clearButton.title = 'Quitar imagen seleccionada';

                clearButton.addEventListener('click', function (e) {
                    e.stopPropagation(); // Evitar que el clic se propague a la imagen
                    imagenInput.value = '';
                    previewContainer.style.display = 'none';
                    const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
                    if (mantenerImagenCheckbox) {
                        mantenerImagenCheckbox.checked = true;
                    }
                });

                previewContainer.appendChild(clearButton);
            }
        } else {
            previewContainer.style.display = 'none';
        }
    });
}

// Función para mostrar imagen ampliada en ventana modal
function mostrarImagenAmpliada(src, titulo) {
    // Verificar si la imagen existe
    if (!src || src === window.location.origin + '/images/productos/default.png') {
        // Si es la imagen por defecto, mostrar un mensaje en lugar de abrirla
        mostrarAlerta('No hay imagen disponible para este producto', 'info');
        return;
    }

    // Crear y mostrar la ventana modal con SweetAlert2
    Swal.fire({
        title: titulo || 'Vista previa',
        html: `<div class="text-center"><img src="${src}" class="img-fluid" style="max-height: 70vh;" alt="Imagen ampliada"></div>`,
        showCloseButton: true,
        showConfirmButton: false,
        width: 'auto',
        padding: '1rem',
        background: '#fff',
        backdrop: 'rgba(0,0,0,0.8)'
    });
}

// Función para configurar la vista previa de imágenes en toda la aplicación
function configurarVistasPreviasImagenes() {
    // 1. Configurar las imágenes en la tabla de productos
    const tablaProductos = document.getElementById('tablaProductos');
    if (tablaProductos) {
        tablaProductos.addEventListener('click', function (e) {
            // Verificar si el clic fue en una imagen
            const imagen = e.target.closest('img.img-thumbnail');
            if (imagen) {
                e.preventDefault();
                const nombreProducto = imagen.getAttribute('alt') || 'Producto';
                mostrarImagenAmpliada(imagen.src, nombreProducto);
            }
        });
    }

    // 2. Configurar la vista previa en el formulario de creación/edición
    const previewContainer = document.getElementById('image-preview-container');
    if (previewContainer) {
        previewContainer.addEventListener('click', function (e) {
            const imagen = e.target.closest('#image-preview');
            if (imagen && imagen.src) {
                // Evitar hacer clic en el botón de eliminar
                if (!e.target.closest('#clear-image-button')) {
                    mostrarImagenAmpliada(imagen.src, 'Vista previa');
                }
            }
        });
    }

    // 3. Configurar la imagen existente en el formulario de edición
    const imagenExistente = document.querySelector('.card-body .img-thumbnail');
    if (imagenExistente) {
        imagenExistente.style.cursor = 'pointer';
        imagenExistente.title = 'Clic para ampliar';
        imagenExistente.addEventListener('click', function () {
            const nombreProducto = this.getAttribute('alt') || 'Producto';
            mostrarImagenAmpliada(this.src, nombreProducto);
        });
    }
}

// Verifica si ya existe un producto con el mismo código
function verificarCodigoDuplicado(codigo) {
    return fetch('/api/Producto/GetAll')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar código duplicado');
            }
            return response.json();
        })
        .then(productos => {
            return productos.some(p => p.codigo.toLowerCase() === codigo.toLowerCase());
        })
        .catch(error => {
            console.error('Error al verificar duplicados:', error);
            return false;
        });
}

// Verifica si ya existe un producto con el mismo nombre
function verificarNombreDuplicado(nombre) {
    return fetch('/api/Producto/GetAll')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar nombre duplicado');
            }
            return response.json();
        })
        .then(productos => {
            return productos.some(p => p.nombre.toLowerCase() === nombre.toLowerCase());
        })
        .catch(error => {
            console.error('Error al verificar duplicados:', error);
            return false;
        });
}

// Función para verificar duplicados al editar, excluyendo el producto actual
function verificarDuplicadosEdicion(codigo, nombre, idActual) {
    return fetch('/api/Producto/GetAll')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar productos');
            }
            return response.json();
        })
        .then(productos => {
            // Filtrar productos excluyendo el actual
            const productosFiltrados = productos.filter(p => p.id !== parseInt(idActual));

            // Normalizar para comparación insensible a mayúsculas/minúsculas
            const codigoNormalizado = codigo.toLowerCase().trim();
            const nombreNormalizado = nombre.toLowerCase().trim();

            // Verificar duplicados
            const duplicadoCodigo = productosFiltrados.some(p =>
                p.codigo.toLowerCase().trim() === codigoNormalizado);

            const duplicadoNombre = productosFiltrados.some(p =>
                p.nombre.toLowerCase().trim() === nombreNormalizado);

            return {
                codigoDuplicado: duplicadoCodigo,
                nombreDuplicado: duplicadoNombre
            };
        });
}

function inicializarFormularioProducto() {
    // Inicializar el campo de fecha de registro
    inicializarCampoFechaRegistro();

    // Inicializar la vista previa de la imagen
    setupImagePreview();

    // Configurar todas las vistas previas de imágenes
    configurarVistasPreviasImagenes();

    // Inicializar la vista previa de la imagen
    setupImagePreview();
    // Si estamos en la página de creación de producto
    const formCrearProducto = document.getElementById('formCrearProducto');
    if (formCrearProducto) {
        // Obtener los campos que deben desactivarse
        const campoStock = document.getElementById('Stock');
        const campoPrecioCompra = document.getElementById('PrecioDeCompra');
        const campoPrecioVenta = document.getElementById('PrecioDeVenta');
        // Generar código automáticamente al cargar el formulario
        const codigoInput = document.getElementById('Codigo');
        if (codigoInput) {
            codigoInput.value = generarCodigoProducto();
            codigoInput.readOnly = true;
        }

        // Desactivar los campos
        if (campoStock) {
            campoStock.disabled = true;
            campoStock.value = '0'; // Valor predeterminado
            agregarMensajeAdvertencia(campoStock);
        }

        if (campoPrecioCompra) {
            campoPrecioCompra.disabled = true;
            campoPrecioCompra.value = '0.00'; // Valor predeterminado
            agregarMensajeAdvertencia(campoPrecioCompra);
        }

        if (campoPrecioVenta) {
            campoPrecioVenta.disabled = true;
            campoPrecioVenta.value = '0.00'; // Valor predeterminado
            agregarMensajeAdvertencia(campoPrecioVenta);
        }

        // Configurar detección de cambios para formulario de creación
        setupFormCreacionChangesDetection();
        // Configurar interceptor de enlaces
        setupLinkInterception();

    }

    // Para el formulario de edición
    const formEditarProducto = document.getElementById('formEditarProducto');
    if (formEditarProducto) {
        // Configurar botón restaurar
        configurarBotonRestaurar();

        // Agregar botón de regeneración de código junto al campo
        const codigoInput = document.getElementById('Codigo');
        if (codigoInput) {
            // Guardar el código original para el botón restaurar
            const codigoOriginal = codigoInput.value;
            // Crear contenedor para el botón si no existe
            let contenedorBoton = document.querySelector('.input-group-append.regenerar-codigo');
            // Verificar si el botón ya existe para evitar duplicados
            const existingButton = document.getElementById('btnRegenerarCodigo');

            if (!contenedorBoton && !existingButton) {
                // Ajustar el contenedor del input para incluir el botón
                const inputGroup = codigoInput.closest('.input-group');
                contenedorBoton = document.createElement('div');
                contenedorBoton.className = 'input-group-append regenerar-codigo';

                const botonRegenerar = document.createElement('button');
                botonRegenerar.type = 'button';
                botonRegenerar.className = 'btn btn-secondary';
                botonRegenerar.id = 'btnRegenerarCodigo';
                botonRegenerar.title = 'Generar nuevo código';
                botonRegenerar.innerHTML = '<i class="bi bi-arrow-repeat"></i>';
                botonRegenerar.onclick = function () {
                    codigoInput.value = generarCodigoProducto();
                    formHasChanges = true;
                };

                contenedorBoton.appendChild(botonRegenerar);
                inputGroup.appendChild(contenedorBoton);
            }

            // Guardar el código original en un atributo data para el botón de restaurar
            codigoInput.setAttribute('data-original-code', codigoOriginal);
        }

        // Guardar datos originales del producto
        const idInput = document.querySelector('input[name="Id"]');
        if (idInput) {
            const id = idInput.value;
            fetch(`/api/Producto/${id}`)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('No se pudo cargar el producto');
                    }
                    return response.json();
                })
                .then(producto => {
                    productoOriginal = {
                        codigo: producto.codigo,
                        nombre: producto.nombre,
                        descripcion: producto.descripcion || '',
                        categoriaId: producto.categoriaId,
                        estado: producto.estado
                    };

                    // Configurar monitoreo de cambios una vez cargados los datos originales
                    monitorearCambiosProducto();
                    setupLinkInterception();
                })
                .catch(error => {
                    console.error('Error al cargar producto original:', error);
                });
        }
        // los campos de precio de compra, venta y stock
        const campoStock = document.getElementById('Stock');
        const campoPrecioCompra = document.getElementById('PrecioDeCompra');
        const campoPrecioVenta = document.getElementById('PrecioDeVenta');

        if (campoStock) {
            //campoStock.disabled = true;
            agregarMensajeAdvertencia(campoStock);
        }
        if (campoPrecioCompra) {
            // Guardar el valor original como atributo de datos
            campoPrecioCompra.setAttribute('data-original-value', campoPrecioCompra.value);
            campoPrecioCompra.disabled = true;
            agregarMensajeAdvertencia(campoPrecioCompra);
        }

        if (campoPrecioVenta) {
            // Guardar el valor original como atributo de datos
            campoPrecioVenta.setAttribute('data-original-value', campoPrecioVenta.value);
            campoPrecioVenta.disabled = true;
            agregarMensajeAdvertencia(campoPrecioVenta);
        }

        // Verificar el checkbox de mantener imagen
        const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
        const imagenInput = document.getElementById('imagen');

        if (mantenerImagenCheckbox && imagenInput) {
            // Si se selecciona una nueva imagen, desmarcar "mantener imagen"
            imagenInput.addEventListener('change', function () {
                if (this.files && this.files.length > 0) {
                    mantenerImagenCheckbox.checked = false;
                }
            });
        }
    }
}

// Función para agregar un mensaje de advertencia debajo del campo
function agregarMensajeAdvertencia(elemento) {
    const divAyuda = document.createElement('div');
    divAyuda.className = 'form-text text-info small';
    divAyuda.innerHTML = '<i class="bi bi-info-circle"></i> Este valor se actualizará automáticamente al realizar compras del producto.';

    // Insertar después del campo
    elemento.parentNode.insertBefore(divAyuda, elemento.nextSibling);
}

// Esperar a que el DOM esté completamente cargado
document.addEventListener('DOMContentLoaded', function () {
    inicializarFormularioProducto();

    // Configurar botón de cancelar en edición/creación
    const btnCancelar = document.querySelector('#formCrearProducto a.btn-secondary, #formEditarProducto a.btn-secondary');
    if (btnCancelar) {
        btnCancelar.addEventListener('click', function (e) {
            if (formHasChanges) {
                e.preventDefault();

                Swal.fire({
                    title: 'Confirmar cancelación',
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
                        window.location.href = btnCancelar.getAttribute('href');
                    }
                });
            }
        });
    }

    // Añadir manejo específico para el botón de regenerar código
    const btnRegenerarCodigo = document.getElementById('btnRegenerarCodigo');
    if (btnRegenerarCodigo) {
        btnRegenerarCodigo.addEventListener('click', function () {
            const codigoInput = document.getElementById('Codigo');
            if (codigoInput) {
                codigoInput.value = generarCodigoProducto();
                formHasChanges = true;
            }
        });
    }

    // Configurar el manejador del formulario de creación de producto
    const formCrearProducto = document.getElementById('formCrearProducto');
    if (formCrearProducto) {
        formCrearProducto.addEventListener('submit', function (e) {
            e.preventDefault();
            crearProducto();
        });
    }

    // Configurar el manejador del formulario de edición de producto
    const formEditarProducto = document.getElementById('formEditarProducto');
    if (formEditarProducto) {
        formEditarProducto.addEventListener('submit', function (e) {
            e.preventDefault();
            editarProducto();
        });
    }

    // Cargar productos en la tabla si estamos en la página Index
    const tablaProductos = document.getElementById('tablaProductos');
    if (tablaProductos) {
        cargarProductos();
    }

    // Configurar el buscador de productos
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('keyup', function (e) {
            if (e.key === 'Enter') {
                loadProductos(this.value);
            }
        });

        // Botón de búsqueda (si existe)
        const searchButton = document.getElementById('searchButton');
        if (searchButton) {
            searchButton.addEventListener('click', function () {
                loadProductos(searchInput.value);
            });
        }

        // Botón para limpiar búsqueda
        const clearSearchButton = document.getElementById('clearSearchButton');
        if (clearSearchButton) {
            clearSearchButton.addEventListener('click', function () {
                searchInput.value = '';
                loadProductos('');
            });
        }
    }

    // Configurar los manejadores para eliminar producto
    configurarBotonesEliminar();

    // Configurar el botón de limpiar para productos
    configurarBotonLimpiarProducto();
});

// Función para verificar si la categoría está activa
function verificarCategoriaActiva(categoriaId) {
    return new Promise((resolve, reject) => {
        fetch(`/api/Categoria/${categoriaId}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al verificar la categoría');
                }
                return response.json();
            })
            .then(categoria => {
                resolve(categoria.estado);
            })
            .catch(error => {
                console.error('Error:', error);
                // En caso de error, asumimos que la categoría no está activa
                resolve(false);
            });
    });
}

// Cargar productos desde la API
function loadProductos(searchTerm = '') {
    const tableBody = document.getElementById('productosTableBody');
    if (!tableBody) return;

    // Mostrar indicador de carga
    tableBody.innerHTML = '<tr><td colspan="11" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';

    // Llamada a la API
    fetch(`/api/Producto/GetAll`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar productos');
            }
            return response.json();
        })
        .then(data => {
            productos = data;

            // Filtrar por término de búsqueda si existe
            if (searchTerm) {
                searchTerm = searchTerm.toLowerCase();
                productos = productos.filter(p =>
                    p.codigo.toLowerCase().includes(searchTerm) ||
                    p.nombre.toLowerCase().includes(searchTerm) ||
                    (p.descripcion && p.descripcion.toLowerCase().includes(searchTerm)) ||
                    p.categoriaNombre.toLowerCase().includes(searchTerm)
                );
            }

            // Calcular paginación
            totalPages = Math.ceil(productos.length / pageSize);

            // Mostrar datos
            renderProductos();
            renderPaginacion();
        })
        .catch(error => {
            console.error('Error:', error);
            tableBody.innerHTML = `<tr><td colspan="11" class="text-center text-danger">Error al cargar datos: ${error.message}</td></tr>`;
        });
}

// Renderizar productos en la tabla
function renderProductos() {
    const tableBody = document.getElementById('productosTableBody');
    if (!tableBody) return;

    tableBody.innerHTML = '';

    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;
    const paginatedProductos = productos.slice(start, end);

    if (paginatedProductos.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="11" class="text-center">No se encontraron productos</td></tr>';
        return;
    }

    const imagenPorDefecto = '/images/productos/default.png';

    // Renderizar productos directamente sin verificaciones adicionales
    paginatedProductos.forEach(producto => {
        const row = document.createElement('tr');
        const fechaFormateada = formatearFechaAmPm(producto.fechaDeRegistro);

        // Determinar la URL de la imagen con verificación más robusta
        let imagenUrl = imagenPorDefecto;

        if (producto.imagenUrl && producto.imagenUrl.trim() !== '' &&
            producto.imagenUrl !== null && producto.imagenUrl !== 'null') {
            // Verificar si la ruta es relativa y añadir la base si es necesario
            if (producto.imagenUrl.startsWith('/')) {
                imagenUrl = producto.imagenUrl;
            } else {
                imagenUrl = '/' + producto.imagenUrl;
            }
        }

        // Generar los botones de acción sin verificación de ventas activas
        let botonesAccion = `
            <a href="/Productos/Edit/${producto.id}" class="btn btn-sm btn-primary me-1">
                <i class="fas fa-edit"></i>
            </a>
            <button class="btn btn-sm btn-danger btn-eliminar-producto" 
                    data-id="${producto.id}" 
                    onclick="eliminarProducto(${producto.id})">
                <i class="fas fa-trash"></i>
            </button>
        `;

        row.innerHTML = `
            <td class="text-center">
                <img src="${imagenUrl}" 
                    alt="${producto.nombre}"
                    data-nombre="${producto.nombre}"
                    class="img-thumbnail" 
                    style="width: 50px; height: 50px; cursor: pointer;"
                    title="Clic para ampliar"
                    onerror="this.src='${imagenPorDefecto}';">
            </td>
            <td>${producto.codigo}</td>
            <td>${producto.nombre}</td>
            <td>${producto.descripcion}</td>
            <td>${producto.categoriaNombre}</td>
            <td class="${producto.stock <= 5 ? 'text-danger fw-bold' : ''}">${producto.stock}</td>
            <td>${formatearPrecio(producto.precioDeCompra)}</td>
            <td>${formatearPrecio(producto.precioDeVenta)}</td>
            <td>
                <span class="badge ${producto.estado ? 'bg-success' : 'bg-danger'}">
                    ${producto.estado ? 'Activo' : 'Inactivo'}
                </span>
            </td>
            <td>${fechaFormateada}</td>
            <td>
                ${botonesAccion}
            </td>
        `;

        tableBody.appendChild(row);
    });

    // Configurar las imágenes ampliables después de renderizar
    // configurarImagenesAmpliables();
}

// Renderizar paginación
function renderPaginacion() {
    const pagination = document.getElementById('paginationProductos');
    if (!pagination) return;

    pagination.innerHTML = '';

    if (totalPages <= 1) return;

    // Botón Anterior
    const prevLi = document.createElement('li');
    prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
    prevLi.innerHTML = `<a class="page-link" href="#" onclick="cambiarPagina(${currentPage - 1})">Anterior</a>`;
    pagination.appendChild(prevLi);

    // Páginas numeradas
    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement('li');
        li.className = `page-item ${currentPage === i ? 'active' : ''}`;
        li.innerHTML = `<a class="page-link" href="#" onclick="cambiarPagina(${i})">${i}</a>`;
        pagination.appendChild(li);
    }

    // Botón Siguiente
    const nextLi = document.createElement('li');
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `<a class="page-link" href="#" onclick="cambiarPagina(${currentPage + 1})">Siguiente</a>`;
    pagination.appendChild(nextLi);
}

// Función para cargar y mostrar los productos (actualizada)
function cargarProductos() {
    loadProductos();
    inicializarCampoFechaRegistro();
}

// Función para restaurar datos originales del producto
function restaurarDatosOriginalesProducto() {
    if (!productoOriginal) return;

    // Restaurar valores de los campos
    const codigoInput = document.getElementById('Codigo');
    if (codigoInput) {
        // Usar el código original guardado en el atributo data
        const codigoOriginal = codigoInput.getAttribute('data-original-code') || productoOriginal.codigo;
        codigoInput.value = codigoOriginal;
    }
    document.getElementById('Nombre').value = productoOriginal.nombre;
    document.getElementById('Descripcion').value = productoOriginal.descripcion || '';
    document.getElementById('CategoriaId').value = productoOriginal.categoriaId;

    // Restaurar estado
    const estadoCheckbox = document.querySelector('input[name="Estado"]');
    if (estadoCheckbox) {
        estadoCheckbox.checked = productoOriginal.estado;
    }

    // Restaurar imagen (marcar checkbox de mantener imagen)
    const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
    if (mantenerImagenCheckbox) {
        mantenerImagenCheckbox.checked = true;
    }

    // Ocultar vista previa de imagen nueva si existe
    const previewContainer = document.getElementById('image-preview-container');
    if (previewContainer) {
        previewContainer.style.display = 'none';
    }

    // Limpiar input de imagen
    const imagenInput = document.getElementById('imagen');
    if (imagenInput) {
        imagenInput.value = '';
    }

    // Mostrar mensaje de confirmación
    mostrarAlerta('Se han restaurado los datos originales', 'info');

    // Actualizar estado de cambios
    formHasChanges = false;
}

// Configurar el botón restaurar
function configurarBotonRestaurar() {
    const btnRestaurar = document.getElementById('btnRestaurarProducto');
    if (btnRestaurar) {
        btnRestaurar.addEventListener('click', function () {
            Swal.fire({
                title: 'Confirmar',
                text: '¿Desea restaurar todos los campos a su valor original?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Sí, restaurar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    restaurarDatosOriginalesProducto();
                }
            });
        });
    }
}

// Cambiar página
function cambiarPagina(page) {
    if (page < 1 || page > totalPages) return;
    currentPage = page;
    renderProductos();
    renderPaginacion();
}

// Formatear precio
function formatearPrecio(precio) {
    if (precio === undefined || precio === null) return '-';
    return new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'GTQ' }).format(precio);
}

// Función para crear un nuevo producto (modificada para manejar mejor el error)
function crearProducto(event) {
    if (!validarFormularioProducto()) {
        return false;
    }

    const codigo = document.getElementById('Codigo').value.trim();
    const nombre = document.getElementById('Nombre').value.trim();
    const categoriaId = document.getElementById('CategoriaId').value;

    // Primero verificar si hay duplicados
    Promise.all([
        verificarCodigoDuplicado(codigo),
        verificarNombreDuplicado(nombre),
        verificarCategoriaActiva(categoriaId)
    ])
        .then(([codigoDuplicado, nombreDuplicado, categoriaActiva]) => {
            if (codigoDuplicado) {
                mostrarAlerta('Ya existe un producto con este código', 'danger');
                document.getElementById('Codigo').focus();
                return;
            }

            if (nombreDuplicado) {
                mostrarAlerta('Ya existe un producto con este nombre', 'danger');
                document.getElementById('Nombre').focus();
                return;
            }

            if (!categoriaActiva) {
                mostrarAlerta('La categoría seleccionada no está activa', 'warning');
                document.getElementById('CategoriaId').focus();
                return;
            }

            // Si no hay duplicados, continuar con la creación
            enviarFormularioCreacion();
        });
}

function enviarFormularioCreacion() {
    const formData = new FormData(document.getElementById('formCrearProducto'));
    const btnSubmit = document.querySelector('#formCrearProducto button[type="submit"]');
    const textoOriginal = btnSubmit.innerHTML;

    // Verificar si se ha seleccionado una imagen
    const imagenInput = document.getElementById('imagen');
    if (!imagenInput.files || imagenInput.files.length === 0) {
        // Si no se seleccionó imagen, añadir un campo indicando que use la imagen predeterminada
        formData.append('UseDefaultImage', 'true');
    }

    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';

    fetch('/api/Producto/Create', {
        method: 'POST',
        body: formData
    })
        .then(async response => {
            if (!response.ok) {
                const text = await response.text();
                throw new Error(text);
            }
            return response.json();
        })
        .then(data => {
            Swal.fire({
                title: '¡Éxito!',
                text: 'El producto ha sido creado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                window.onbeforeunload = null; // Desactivar la alerta de cambios sin guardar
                window.location.href = '/Productos/Index';
            });
        })
        .catch(error => {
            console.error('Error:', error.message);
            Swal.fire({
                title: 'Error',
                text: error.message || 'Hubo un problema al crear el producto.',
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        })
        .finally(() => {
            btnSubmit.disabled = false;
            btnSubmit.innerHTML = textoOriginal;
        });
}

// Función para validar el formulario antes de enviar
function validarFormularioProducto() {
    const codigo = document.getElementById('Codigo').value.trim();
    const nombre = document.getElementById('Nombre').value.trim();
    const descripcion = document.getElementById('Descripcion').value.trim();
    const categoriaId = document.getElementById('CategoriaId').value;

    let isValid = true;

    if (!codigo) {
        isValid = false;
        document.getElementById('Codigo').classList.add('is-invalid');
        mostrarAlerta('El código del producto es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('Codigo').classList.remove('is-invalid');
    }

    if (!nombre) {
        isValid = false;
        document.getElementById('Nombre').classList.add('is-invalid');
        mostrarAlerta('El nombre del producto es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('Nombre').classList.remove('is-invalid');
    }

    if (descripcion.length > 200) {
        isValid = false;
        document.getElementById('Descripcion').classList.add('is-invalid');
        mostrarAlerta('La descripción no puede exceder los 200 caracteres', 'danger');
        return false;
    }
    else if (!descripcion) {
        isValid = false;
        document.getElementById('Descripcion').classList.add('is-invalid');
        mostrarAlerta('La descripción del producto es obligatoria', 'danger');
        return false;
    }

    if (!categoriaId) {
        isValid = false;
        document.getElementById('CategoriaId').classList.add('is-invalid');
        mostrarAlerta('Debe seleccionar una categoría', 'danger');
        return false;
    } else {
        document.getElementById('CategoriaId').classList.remove('is-invalid');
    }

    return isValid;
}

// Función para editar un producto existente
function editarProducto() {
    const form = document.getElementById('formEditarProducto');
    const idActual = form.querySelector('input[name="Id"]').value;
    const codigo = document.getElementById('Codigo').value.trim();
    const nombre = document.getElementById('Nombre').value.trim();
    const categoriaId = document.getElementById('CategoriaId').value;
    const categoriaOriginalId = document.getElementById('categoriaOriginalId').value;

    // Validar campos obligatorios
    if (!validarFormularioProducto()) {
        return;
    }

    // Verificar duplicados (excluyendo el producto actual)
    verificarDuplicadosEdicion(codigo, nombre, idActual)
        .then(resultado => {
            if (resultado.codigoDuplicado) {
                mostrarAlerta('Ya existe un producto con este código', 'warning');
                return;
            }

            if (resultado.nombreDuplicado) {
                mostrarAlerta('Ya existe un producto con este nombre', 'warning');
                return;
            }

            // Verificar si la categoría ha cambiado respecto a la original
            if (parseInt(categoriaId) !== parseInt(categoriaOriginalId)) {
                // Solo validar si la categoría está activa cuando se cambia a una nueva
                verificarCategoriaActiva(categoriaId)
                    .then(categoriaActiva => {
                        if (!categoriaActiva) {
                            Swal.fire({
                                title: 'Categoría inactiva',
                                text: '¿Desea asignar este producto a una categoría inactiva? Se recomienda usar categorías activas.',
                                icon: 'warning',
                                showCancelButton: true,
                                confirmButtonText: 'Sí, continuar',
                                cancelButtonText: 'Cancelar'
                            }).then((result) => {
                                if (result.isConfirmed) {
                                    enviarFormularioEdicion();
                                }
                            });
                        } else {
                            // Si la nueva categoría está activa, enviar el formulario
                            enviarFormularioEdicion();
                        }
                    });
            } else {
                // Si la categoría no ha cambiado, permitir la edición sin importar su estado
                enviarFormularioEdicion();
            }
        })
        .catch(error => {
            mostrarAlerta('Error al verificar datos: ' + error.message, 'danger');
        });
}
// ImagenUrl: mantenerImagen ? document.querySelector('img.img-thumbnail')?.getAttribute('src') : '/images/productos/default.png'

function enviarFormularioEdicion() {
    const form = document.getElementById('formEditarProducto');
    const formData = new FormData(form);
    const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (token) {
        formData.append('__RequestVerificationToken', token);
    }
    const id = formData.get('Id');
    const btnSubmit = document.querySelector('#formEditarProducto button[type="submit"]');
    const textoOriginal = btnSubmit.innerHTML;

    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Actualizando...';

    // Verificar si hay una nueva imagen
    const imagenInput = document.getElementById('imagen');
    const mantenerImagenCheckbox = document.getElementById('mantenerImagen');
    const mantenerImagen = mantenerImagenCheckbox ? mantenerImagenCheckbox.checked : false;
    const hayNuevaImagen = imagenInput && imagenInput.files && imagenInput.files.length > 0;

    // Obtener los precios originales
    const precioCompra = document.getElementById('PrecioDeCompra').getAttribute('data-original-value') || document.getElementById('PrecioDeCompra').value;
    const precioVenta = document.getElementById('PrecioDeVenta').getAttribute('data-original-value') || document.getElementById('PrecioDeVenta').value;

    // Obtener el estado del checkbox correctamente
    const estadoElem = document.querySelector('input[name="Estado"]');
    const estadoValor = estadoElem ? estadoElem.checked : false;

    // Si hay una nueva imagen, enviar por POST con FormData al endpoint específico
    if (hayNuevaImagen) {
        // Asegurarnos que el formulario tenga los campos necesarios
        formData.set('PrecioDeCompra', precioCompra);
        formData.set('PrecioDeVenta', precioVenta);
        // Agregar el estado explícitamente
        formData.set('Estado', estadoValor ? 'true' : 'false');

        fetch(`/api/Producto/EditImage/${id}`, {
            method: 'POST',
            body: formData
        })
            .then(async response => {
                if (!response.ok) {
                    // Solo intentar parsear JSON si hay contenido
                    if (response.headers.get('content-length') > 0) {
                        const errorData = await response.json();
                        throw new Error(errorData.message || 'Error al actualizar el producto');
                    } else {
                        throw new Error('Error al actualizar el producto');
                    }
                }

                // Si la respuesta está bien pero no tiene contenido, no intentar parsear JSON
                if (response.status === 204) {  // NoContent
                    return null;
                }

                // Solo intentar parsear JSON si hay contenido
                if (response.headers.get('content-length') > 0) {
                    return response.json();
                }
                return null;
            })
            .then(() => {
                mostrarMensajeExito();
            })
            .catch(error => {
                mostrarMensajeError(error);
            })
            .finally(() => {
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = textoOriginal;
            });
    } else {
        // Si no hay nueva imagen, usar el API endpoint PUT con JSON
        const productoData = {
            Codigo: formData.get('Codigo'),
            Nombre: formData.get('Nombre'),
            Descripcion: formData.get('Descripcion'),
            CategoriaId: parseInt(formData.get('CategoriaId')),
            PrecioDeCompra: parseFloat(precioCompra),
            PrecioDeVenta: parseFloat(precioVenta),
            Estado: estadoValor,
            ImagenUrl: null // Vamos a establecer esto correctamente
        };

        // SOLUCIÓN CORREGIDA: Obtener la URL correctamente cuando se mantiene la imagen
        if (mantenerImagen) {
            // Buscar la imagen más directamente (buscando dentro del div específico de imagen)
            const imgElement = document.querySelector('.mt-2 img.img-thumbnail');

            if (imgElement && imgElement.src) {
                // Usar la URL completa de la imagen
                productoData.ImagenUrl = imgElement.src;

                // Asegurar que guardamos la ruta relativa correcta
                if (productoData.ImagenUrl.includes(window.location.origin)) {
                    productoData.ImagenUrl = productoData.ImagenUrl.replace(window.location.origin, '');
                }
            } else {
                // Si no se encuentra la imagen, usar la ruta de la imagen por defecto
                productoData.ImagenUrl = '/images/productos/default.png';
            }
        } else {
            productoData.ImagenUrl = '/images/productos/default.png';
        }

        fetch(`/api/Producto/Edit/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify(productoData)
        })
            .then(async response => {
                if (!response.ok) {
                    // Intentar obtener el mensaje de error del servidor
                    const errorText = await response.text();
                    throw new Error(errorText || 'Error al actualizar el producto');
                }
                // No intentar analizar como JSON si el status es 204 No Content
                if (response.status === 204) {
                    return null; // No hay contenido que procesar
                }
                // Solo intentar analizar como JSON si hay contenido
                if (response.headers.get('content-length') > 0) {
                    return response.json();
                }
                return null;
            })
            .then(() => {
                mostrarMensajeExito();
            })
            .catch(error => {
                mostrarMensajeError(error);
            })
            .finally(() => {
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = textoOriginal;
            });
    }
}

// Función para limpiar el formulario de productos
function limpiarFormularioProducto() {
    const formCrearProducto = document.getElementById('formCrearProducto');
    if (formCrearProducto) {
        // Generar nuevo código aleatorio
        const codigoInput = document.getElementById('Codigo');
        if (codigoInput) {
            codigoInput.value = generarCodigoProducto();
        }
        // Limpiar campos de texto
        formCrearProducto.querySelector('#Nombre').value = '';
        formCrearProducto.querySelector('#Descripcion').value = '';

        // Restablecer la categoría
        formCrearProducto.querySelector('#CategoriaId').selectedIndex = 0;

        // Mantener los valores por defecto para los campos deshabilitados
        const campoStock = document.getElementById('Stock');
        if (campoStock) campoStock.value = '0';

        const campoPrecioCompra = document.getElementById('PrecioDeCompra');
        if (campoPrecioCompra) campoPrecioCompra.value = '0.00';

        const campoPrecioVenta = document.getElementById('PrecioDeVenta');
        if (campoPrecioVenta) campoPrecioVenta.value = '0.00';

        // Restablecer el estado a activo (por defecto)
        const estadoCheckbox = formCrearProducto.querySelector('input[name="Estado"]');
        if (estadoCheckbox) estadoCheckbox.checked = true;

        // Limpiar el campo de imagen
        const imagenInput = document.getElementById('imagen');
        if (imagenInput) imagenInput.value = '';

        // Ocultar la vista previa de imagen si existe
        const previewContainer = document.getElementById('image-preview-container');
        if (previewContainer) previewContainer.style.display = 'none';

        // Remover clases de validación
        formCrearProducto.querySelectorAll('.is-invalid').forEach(el => {
            el.classList.remove('is-invalid');
        });

        // Restablecer la bandera de cambios
        formHasChanges = false;

        // Mostrar mensaje de confirmación
        mostrarAlerta('Formulario limpiado correctamente', 'info');
    }
}

// Configurar el botón de limpiar con confirmación
function configurarBotonLimpiarProducto() {
    const btnLimpiar = document.getElementById('btnLimpiarProducto');
    if (btnLimpiar) {
        btnLimpiar.addEventListener('click', function () {
            Swal.fire({
                title: 'Confirmar',
                text: '¿Está seguro que desea limpiar el formulario?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Confirmar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    limpiarFormularioProducto();
                }
            });
        });
    }
}

// Función para generar un código de producto aleatorio en formato XX-00
function generarCodigoProducto() {
    // Generar dos letras aleatorias (A-Z)
    const letras = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    let codigoLetras = '';
    for (let i = 0; i < 2; i++) {
        codigoLetras += letras.charAt(Math.floor(Math.random() * letras.length));
    }

    // Generar dos números aleatorios (0-9)
    let codigoNumeros = Math.floor(Math.random() * 100).toString().padStart(2, '0');

    // Combinar en formato XX-00
    return `${codigoLetras}-${codigoNumeros}`;
}

function mostrarMensajeExito() {
    Swal.fire({
        title: '¡Éxito!',
        text: 'El producto ha sido actualizado correctamente',
        icon: 'success',
        confirmButtonText: 'Aceptar'
    }).then(() => {
        window.onbeforeunload = null;
        window.location.href = '/Productos/Index';
    });
}

function mostrarMensajeError(error) {
    console.error('Error:', error);
    Swal.fire({
        title: 'Error',
        text: 'Hubo un problema al actualizar el producto.',
        icon: 'error',
        confirmButtonText: 'Aceptar'
    });
}

// Función para configurar los botones de eliminar en la tabla
function configurarBotonesEliminar() {
    // Asegúrate que este selector coincida con la clase en el HTML
    const botonesEliminar = document.querySelectorAll('.btn-eliminar-producto, .btn-eliminar');

    botonesEliminar.forEach(boton => {
        boton.addEventListener('click', function () {
            const id = this.getAttribute('data-id');
            eliminarProducto(id);
        });
    });
}

// Función para eliminar un producto (actualizada)
function eliminarProducto(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: '¿Desea eliminar este producto? Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/api/Producto/Delete/${id}`, {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Hubo un problema al eliminar el producto.');
                    }
                    return response.ok ? Promise.resolve() : response.json();
                })
                .then(() => {
                    mostrarModal('Éxito', 'Producto eliminado correctamente.', 'success');

                    // Recargar la tabla de productos
                    loadProductos(document.getElementById('searchInput')?.value || '');
                })
                .catch(error => {
                    console.error('Error:', error);
                    mostrarModal('Error', 'No se pudo eliminar el producto. Intente nuevamente.', 'error');
                });
        }
    });
}

// Función para mostrar alerta estilizada (para mensajes pequeños)
function mostrarAlerta(mensaje, tipo, duracion = 5000) {
    // Mapear los tipos de alerta a los iconos de SweetAlert
    let icon;
    switch (tipo) {
        case 'success': icon = 'success'; break;
        case 'danger': icon = 'error'; break;
        case 'warning': icon = 'warning'; break;
        default: icon = 'info'; break;
    }

    // Mostrar toast de SweetAlert
    const Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: duracion,
        timerProgressBar: true,
        didOpen: (toast) => {
            toast.addEventListener('mouseenter', Swal.stopTimer);
            toast.addEventListener('mouseleave', Swal.resumeTimer);
        }
    });

    Toast.fire({
        icon: icon,
        title: mensaje
    });
}

// Función para mostrar modales
function mostrarModal(titulo, mensaje, tipo) {
    Swal.fire({
        title: titulo,
        text: mensaje,
        icon: tipo,
        confirmButtonText: 'Aceptar'
    });
}