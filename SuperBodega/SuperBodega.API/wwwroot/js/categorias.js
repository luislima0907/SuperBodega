// Variables globales
let currentPage = 1;
const pageSize = 10;
let totalPages = 0;
let categorias = [];
let categoriaOriginal = null;
let formHasChanges = false;

// Función principal que se ejecuta cuando se carga la página
document.addEventListener('DOMContentLoaded', function() {
    // Detectar qué página estamos
    const currentPath = window.location.pathname;
    const viewMode = getViewMode(currentPath);

    // Inicializar según el modo
    switch(viewMode) {
        case 'index':
            initIndexView();
            break;
        case 'create':
            initCreateView();
            break;
        case 'edit':
            initEditView();
            break;
    }
});

// Determina en qué vista estamos
function getViewMode(path) {
    if (path.includes('/Categorias/Index')) {
        return 'index';
    } else if (path.includes('/Categorias/Create')) {
        return 'create';
    } else if (path.includes('/Categorias/Edit/')) {
        return 'edit';
    }
    return 'unknown';
}

// INICIALIZADORES DE VISTA
// ------------------------

// Inicializa la vista de índice (listado)
function initIndexView() {
    if (document.getElementById('categoriasTableBody')) {
        loadCategorias();

        // Configurar búsqueda
        document.getElementById('searchButton').addEventListener('click', function() {
            loadCategorias(document.getElementById('searchInput').value);
        });

        document.getElementById('searchInput').addEventListener('keypress', function(e) {
            if (e.key === 'Enter') {
                loadCategorias(this.value);
            }
        });
        // Configurar el botón de limpiar búsqueda
        const btnLimpiar = document.getElementById('clearSearchButton');
        if (btnLimpiar) {
            btnLimpiar.addEventListener('click', function() {
                document.getElementById('searchInput').value = '';
                loadCategorias();
            });
        }
    }
}

// FUNCIONES PARA CREACIÓN
// ----------------------

// Inicializar formulario de creación
function initCreateForm() {
    const form = document.getElementById('createCategoriaForm');
    if (!form) return;

    form.addEventListener('submit', function(e) {
        e.preventDefault();

        const nombre = document.getElementById('nombre').value.trim();
        if (!nombre) {
            mostrarAlerta('El nombre de la categoría es obligatorio', 'danger');
            document.getElementById('nombre').focus();
            return;
        }

        if (!form.checkValidity()) {
            e.stopPropagation();
            form.classList.add('was-validated');
            return;
        }

        // Verificar si existe una categoría con el mismo nombre
        verificarNombreDuplicado(nombre)
            .then(existeDuplicado => {
                if (existeDuplicado) {
                    mostrarAlerta('Ya existe una categoría con este nombre. Por favor, use un nombre diferente.', 'warning');
                    document.getElementById('nombre').focus();
                    return;
                }

                // Si no hay duplicados, proceder con la creación
                const categoriaData = {
                    nombre: nombre,
                    descripcion: document.getElementById('descripcion').value,
                    estado: document.getElementById('estado').checked,
                    fechaDeRegistro: document.getElementById('fechaDeRegistro').value
                };

                createCategoria(categoriaData);
            })
            .catch(error => {
                mostrarAlerta('Error al verificar nombres duplicados: ' + error.message, 'danger');
            });
    });
}

// Verifica si ya existe una categoría con el mismo nombre
function verificarNombreDuplicado(nombre) {
    return new Promise((resolve, reject) => {
        fetch('/api/Categoria')
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar categorías');
                }
                return response.json();
            })
            .then(categorias => {
                // Convertir a minúsculas para comparación sin distinción de mayúsculas/minúsculas
                const nombreNormalizado = nombre.toLowerCase().trim();
                const existeDuplicado = categorias.some(cat =>
                    cat.nombre.toLowerCase().trim() === nombreNormalizado
                );
                resolve(existeDuplicado);
            })
            .catch(error => {
                console.error('Error al verificar duplicados:', error);
                reject(error);
            });
    });
}

// Crear nueva categoría
function createCategoria(categoriaData) {
    fetch('/api/Categoria/Create', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(categoriaData)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al crear categoría');
            }
            return response.json();
        })
        .then(data => {
            mostrarModalExito('La categoría se ha creado correctamente');
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta('Ha ocurrido un error al crear la categoría: ' + error.message, 'danger');
        });
}

// FUNCIONES PARA EDICIÓN
// ---------------------

// Cargar datos de categoría para edición
function loadCategoriaData(id) {
    fetch(`/api/Categoria/${id}`)
.then(response => {
        if (!response.ok) {
            throw new Error('Categoría no encontrada');
        }
        return response.json();
    })
        .then(categoria => {
            // Guardar los datos originales
            categoriaOriginal = {...categoria};

            const form = document.getElementById('editCategoriaForm');
            if (form) {
                document.getElementById('categoriaId').value = categoria.id;
                document.getElementById('nombre').value = categoria.nombre;
                document.getElementById('descripcion').value = categoria.descripcion || '';
                document.getElementById('estado').checked = categoria.estado;

                // Formatear fecha
                const fecha = new Date(categoria.fechaDeRegistro);
                document.getElementById('fechaDeRegistro').value = fecha.toISOString().slice(0, 16);
                // Mostrar fecha en formato AM/PM en el campo visible
                document.getElementById('fechaDeRegistroVisible').value = formatearFechaAmPm(categoria.fechaDeRegistro);

                // Configurar formulario
                form.addEventListener('submit', function(e) {
                    e.preventDefault();

                    const nombre = document.getElementById('nombre').value.trim();
                    if (!nombre) {
                        mostrarAlerta('El nombre de la categoría es obligatorio', 'danger');
                        document.getElementById('nombre').focus();
                        return;
                    }

                    if (!form.checkValidity()) {
                        e.stopPropagation();
                        form.classList.add('was-validated');
                        return;
                    }

                    if (nombre !== categoriaOriginal.nombre) {
                        verificarNombreDuplicadoParaEdicion(nombre, id)
                            .then(existeDuplicado => {
                                if (existeDuplicado) {
                                    mostrarAlerta('Ya existe una categoría con este nombre. Por favor, use un nombre diferente.', 'warning');
                                    document.getElementById('nombre').focus();
                                    return;
                                }

                                // Si no hay duplicados, proceder con la actualización
                                const categoriaData = {
                                    nombre: nombre,
                                    descripcion: document.getElementById('descripcion').value,
                                    estado: document.getElementById('estado').checked
                                };

                                updateCategoria(id, categoriaData);
                            })
                            .catch(error => {
                                mostrarAlerta('Error al verificar nombres duplicados: ' + error.message, 'danger');
                            });
                    } else {
                        // Si el nombre no cambia, no necesitamos verificar duplicados
                        const categoriaData = {
                            nombre: nombre,
                            descripcion: document.getElementById('descripcion').value,
                            estado: document.getElementById('estado').checked
                        };

                        updateCategoria(id, categoriaData);
                    }

                });

                // Configurar detección de cambios
                monitorearCambios();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta(`Error: ${error.message}`, 'danger');
            setTimeout(() => {
                window.location.href = '/Categorias/Index';
            }, 3000);
        });
}

// Función para detectar cambios en el formulario
function detectarCambios() {
    if (!categoriaOriginal) return false;

    const nombreActual = document.getElementById('nombre').value;
    const descripcionActual = document.getElementById('descripcion').value || '';
    const estadoActual = document.getElementById('estado').checked;

    return nombreActual !== categoriaOriginal.nombre ||
        descripcionActual !== (categoriaOriginal.descripcion || '') ||
        estadoActual !== categoriaOriginal.estado;
}

// Monitorear cambios en tiempo real
function monitorearCambios() {
    const elementos = ['nombre', 'descripcion', 'estado'];
    elementos.forEach(id => {
        const elemento = document.getElementById(id);
        if (!elemento) return;

        if (elemento.type === 'checkbox') {
            elemento.addEventListener('change', () => {
                formHasChanges = detectarCambios();
            });
        } else {
            ['input', 'change', 'keyup'].forEach(evento => {
                elemento.addEventListener(evento, () => {
                    formHasChanges = detectarCambios();
                });
            });
        }
    });
}

// Restaurar datos a valores originales
function restaurarDatos() {
    if (!categoriaOriginal) return;

    document.getElementById('nombre').value = categoriaOriginal.nombre;
    document.getElementById('descripcion').value = categoriaOriginal.descripcion || '';
    document.getElementById('estado').checked = categoriaOriginal.estado;

    // Formatear fecha para el campo datetime-local
    const fecha = new Date(categoriaOriginal.fechaDeRegistro);
    fecha.setMinutes(fecha.getMinutes() - fecha.getTimezoneOffset());
    document.getElementById('fechaDeRegistro').value = fecha.toISOString().slice(0, 16);

    // Mostrar la fecha formateada
    document.getElementById('fechaDeRegistroVisible').value = formatearFechaAmPm(categoriaOriginal.fechaDeRegistro);

    mostrarAlerta('Datos restaurados a los valores originales', 'info');
    formHasChanges = false;
}

// Actualizar categoría
function updateCategoria(id, categoriaData) {
    fetch(`/api/Categoria/Edit/${id}`, {
        method: 'PUT',
            headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(categoriaData)
    })
.then(response => {
        if (!response.ok) {
            throw new Error('Error al actualizar categoría');
        }
        return response.json();
    })
        .then(data => {
            // Actualizar los datos originales con los nuevos
            categoriaOriginal = {...data};
            mostrarModalExito('Categoría actualizada con éxito');
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta(`Error al actualizar: ${error.message}`, 'danger');
        });
}

// Añadir esta nueva función para verificar duplicados en edición (excluye la categoría actual)
function verificarNombreDuplicadoParaEdicion(nombre, idActual) {
    return new Promise((resolve, reject) => {
        fetch(`/api/Categoria`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('Error al cargar categorías');
                }
                return response.json();
            })
            .then(categorias => {
                // Convertir a minúsculas para comparación sin distinción de mayúsculas/minúsculas
                const nombreNormalizado = nombre.toLowerCase().trim();
                // Excluir la categoría actual de la comparación
                const existeDuplicado = categorias.some(cat =>
                    cat.id !== parseInt(idActual) &&
                    cat.nombre.toLowerCase().trim() === nombreNormalizado
                );
                resolve(existeDuplicado);
            })
            .catch(error => {
                console.error('Error al verificar duplicados:', error);
                reject(error);
            });
    });
}

// FUNCIONES PARA ELIMINACIÓN
// -------------------------

// Confirmar eliminación con Swal
function confirmDelete(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: '¿Desea eliminar esta categoría? Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            deleteCategoria(id);
        }
    });
}

// Eliminar categoría
function deleteCategoria(id) {
    fetch(`/api/Categoria/Delete/${id}`, {
        method: 'DELETE'
    })
.then(response => {
        if (!response.ok) {
            throw new Error('Error al eliminar categoría');
        }

        mostrarModalExitoConAccion('Categoría eliminada con éxito', function() {
            loadCategorias();
            const modalExito = document.getElementById('modalExito');
            const bootstrapModal = bootstrap.Modal.getInstance(modalExito);
            if (bootstrapModal) {
                bootstrapModal.hide();
            }
        });
    })
        .catch(error => {
            console.error('Error:', error);
            mostrarAlerta(`Error al eliminar categoría: ${error.message}`, 'danger');
        });
}


// Inicializa la vista de creación
function initCreateView() {
    // Establecer el estado inicial de la categoria
    const estadoInput = document.getElementById('estado');
    if (estadoInput) {
        estadoInput.checked = true; // Por defecto activo
    }

    // Establecer fecha actual
    const now = new Date();
    const fechaInput = document.getElementById('fechaDeRegistro');

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

        // Agregar campo visible si no existe
        if (!document.getElementById('fechaDeRegistroVisible')) {
            const fechaVisible = document.createElement('input');
            fechaVisible.type = 'text';
            fechaVisible.id = 'fechaDeRegistroVisible';
            fechaVisible.className = 'form-control';
            fechaVisible.readOnly = true;
            fechaVisible.value = formatearFechaAmPm(now);
            fechaInput.parentNode.insertBefore(fechaVisible, fechaInput.nextSibling);
        }
    }

    // Configuración del botón limpiar con Swal
    const btnLimpiar = document.getElementById('btnLimpiar');
    if (btnLimpiar) {
        btnLimpiar.addEventListener('click', function() {
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
                    // Limpiar todos los campos excepto la fecha
                    document.getElementById('nombre').value = '';
                    document.getElementById('descripcion').value = '';
                    document.getElementById('estado').checked = true; // Por defecto activo
                    formHasChanges = false; // Resetear estado de cambios
                }
            });
        });
    }

    // Inicializar formulario de creación
    initCreateForm();

    // Configurar monitoreo de cambios
    monitorearCambiosCreacion();

    // Agregar interceptación de enlaces del menú lateral
    setupSidebarLinksInterception();

    // Configurar el botón de volver con Swal
    const btnVolver = document.getElementById('btnVolver');
    if (btnVolver) {
        btnVolver.addEventListener('click', function(e) {
            e.preventDefault();
            if (formHasChanges) {
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
                        // Desactivar la alerta del navegador antes de redirigir
                        window.onbeforeunload = null;
                        formHasChanges = false;
                        window.location.href = '/Categorias/Index';
                    }
                });
            } else {
                window.location.href = '/Categorias/Index';
            }
        });
    }
}

// Función para detectar cambios en el formulario de creación
function detectarCambiosCreacion() {
    const nombre = document.getElementById('nombre').value.trim();
    const descripcion = document.getElementById('descripcion').value.trim();
    const estado = document.getElementById('estado').checked;

    // Si cualquier campo tiene contenido, consideramos que hay cambios
    return nombre !== '' || descripcion !== '' || estado !== true;
}

// Monitorear cambios en el formulario de creación
function monitorearCambiosCreacion() {
    const elementos = ['nombre', 'descripcion', 'estado'];
    elementos.forEach(id => {
        const elemento = document.getElementById(id);
        if (!elemento) return;

        if (elemento.type === 'checkbox') {
            elemento.addEventListener('change', () => {
                formHasChanges = detectarCambiosCreacion();
            });
        } else {
            ['input', 'change', 'keyup'].forEach(evento => {
                elemento.addEventListener(evento, () => {
                    formHasChanges = detectarCambiosCreacion();
                });
            });
        }
    });
}

// Función para interceptar clics en enlaces del menú lateral
function setupSidebarLinksInterception() {
    // Variable para controlar si la navegación fue aprobada
    let navegacionAprobada = false;

    // Interceptar todos los enlaces de navegación
    const enlaces = document.querySelectorAll('a[href]:not([href^="#"]):not([href^="javascript"])');

    enlaces.forEach(enlace => {
        enlace.addEventListener('click', function(e) {
            // Ignorar si es un evento de menú de sidebar (para desplegar/colapsar submenús)
            if (e.isSidebarMenuEvent ||
                (enlace.closest('.submenu') && enlace.getAttribute('href').startsWith('#'))) {
                return true;
            }

            if (formHasChanges && !navegacionAprobada) {
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

// Inicializa la vista de edición
function initEditView() {
    // Obtener el ID de la categoría de la URL
    const pathname = window.location.pathname;
    const categoriaId = pathname.split('/').pop();

    if (categoriaId) {
        // Cargar datos de la categoría
        loadCategoriaData(categoriaId);

        // Agregar interceptación de enlaces del menú lateral
        setupSidebarLinksInterception();

        // Configurar el botón de restaurar con Swal
        const btnRestaurar = document.getElementById('btnRestaurarCategoria');
        if (btnRestaurar) {
            btnRestaurar.addEventListener('click', function() {
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
                        restaurarDatos();
                    }
                });
            });
        }

        // Configurar el botón de volver con Swal
        const btnVolver = document.getElementById('btnVolver');
        if (btnVolver) {
            btnVolver.addEventListener('click', function(e) {
                e.preventDefault();
                if (formHasChanges) {
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
                            // Desactivar la alerta del navegador antes de redirigir
                            window.onbeforeunload = null;
                            formHasChanges = false;
                            window.location.href = '/Categorias/Index';
                        }
                    });
                } else {
                    window.location.href = '/Categorias/Index';
                }
            });
        }
    } else {
        window.location.href = '/Categorias/Index';
    }
}

// FUNCIONES DE LISTADO Y PAGINACIÓN
// ---------------------------------

// Cargar categorías desde la API
function loadCategorias(searchTerm = '') {
    const tableBody = document.getElementById('categoriasTableBody');
    if (!tableBody) return;

    // Mostrar indicador de carga
    tableBody.innerHTML = '<tr><td colspan="6" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';

    // Llamada a la API
    fetch('/api/Categoria')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar categorías');
            }
            return response.json();
        })
        .then(data => {
            categorias = data;

            // Filtrar por término de búsqueda si existe
            if (searchTerm) {
                searchTerm = searchTerm.toLowerCase();
                categorias = categorias.filter(c =>
                    c.nombre.toLowerCase().includes(searchTerm) ||
                    (c.descripcion && c.descripcion.toLowerCase().includes(searchTerm))
                );
            }

            // Calcular paginación
            totalPages = Math.ceil(categorias.length / pageSize);

            // Mostrar datos
            renderCategorias();
            renderPagination();
        })
        .catch(error => {
            console.error('Error:', error);
            tableBody.innerHTML = `<tr><td colspan="6" class="text-center text-danger">Error al cargar datos: ${error.message}</td></tr>`;
        });
}

// Renderizar categorías en la tabla
function renderCategorias() {
    const tableBody = document.getElementById('categoriasTableBody');
    if (!tableBody) return;

    tableBody.innerHTML = '';

    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;
    const paginatedCategorias = categorias.slice(start, end);

    if (paginatedCategorias.length === 0) {
        tableBody.innerHTML = '<tr><td colspan="6" class="text-center">No se encontraron categorías</td></tr>';
        return;
    }

    paginatedCategorias.forEach(categoria => {
        const row = document.createElement('tr');
        const fechaFormateada = formatearFechaAmPm(categoria.fechaDeRegistro);

        row.innerHTML = `
            <td>${categoria.id}</td>
            <td>${categoria.nombre}</td>
            <td>${categoria.descripcion || '-'}</td>
            <td>
                <span class="badge ${categoria.estado ? 'bg-success' : 'bg-danger'}">
                    ${categoria.estado ? 'Activo' : 'Inactivo'}
                </span>
            </td>
            <td>${fechaFormateada}</td>
            <td>
                <a href="/Categorias/Edit/${categoria.id}" class="btn btn-sm btn-primary me-1">
                    <i class="fas fa-edit"></i>
                </a>
                <button class="btn btn-sm btn-danger" onclick="confirmDelete(${categoria.id})">
                    <i class="fas fa-trash"></i>
                </button>
            </td>
        `;

        tableBody.appendChild(row);
    });
}

// Renderizar paginación
function renderPagination() {
    const pagination = document.getElementById('pagination');
    if (!pagination) return;

    pagination.innerHTML = '';

    if (totalPages <= 1) return;

    // Botón Anterior
    const prevLi = document.createElement('li');
    prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
    prevLi.innerHTML = `<a class="page-link" href="#" onclick="changePage(${currentPage - 1})">Anterior</a>`;
    pagination.appendChild(prevLi);

    // Páginas numeradas
    for (let i = 1; i <= totalPages; i++) {
        const li = document.createElement('li');
        li.className = `page-item ${currentPage === i ? 'active' : ''}`;
        li.innerHTML = `<a class="page-link" href="#" onclick="changePage(${i})">${i}</a>`;
        pagination.appendChild(li);
    }

    // Botón Siguiente
    const nextLi = document.createElement('li');
    nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
    nextLi.innerHTML = `<a class="page-link" href="#" onclick="changePage(${currentPage + 1})">Siguiente</a>`;
    pagination.appendChild(nextLi);
}

// Cambiar página
function changePage(page) {
    if (page < 1 || page > totalPages) return;
    currentPage = page;
    renderCategorias();
    renderPagination();
}

// FUNCIONES DE FORMATO
// -------------------

// Función mejorada para formatear fechas en formato AM/PM con manejo explícito de zona horaria
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

// FUNCIONES UI
// -----------

// Función para mostrar alerta estilizada (mantenemos esta función para mensajes pequeños)
function mostrarAlerta(mensaje, tipo, duracion = 5000) {
    // Mapear los tipos de alerta a los iconos de SweetAlert
    let icon;
    switch (tipo) {
        case 'success': icon = 'success'; break;
        case 'danger': icon = 'error'; break;
        case 'warning': icon = 'warning'; break;
        case 'info': icon = 'info'; break;
        default: icon = 'info';
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

// Función para mostrar modal de éxito con redirección
function mostrarModalExito(mensaje) {
    // Desactivar el evento beforeunload para evitar doble confirmación
    window.onbeforeunload = null;
    formHasChanges = false;

    Swal.fire({
        title: '¡Éxito!',
        text: mensaje,
        icon: 'success',
        confirmButtonText: 'Aceptar',
        confirmButtonColor: '#198754'
    }).then(() => {
        window.location.href = '/Categorias/Index';
    });
}

// Función para mostrar modal de éxito con acción personalizable
function mostrarModalExitoConAccion(mensaje, accion = null) {
    // Desactivar el evento beforeunload para evitar doble confirmación
    window.onbeforeunload = null;
    formHasChanges = false;

    Swal.fire({
        title: '¡Éxito!',
        text: mensaje,
        icon: 'success',
        confirmButtonText: 'Aceptar',
        confirmButtonColor: '#198754'
    }).then(() => {
        if (accion) {
            accion();
        } else {
            window.location.href = '/Categorias/Index';
        }
    });
}