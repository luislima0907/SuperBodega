/**
 * ventas.js - Contiene todas las funcionalidades relacionadas con ventas
 */
document.addEventListener('DOMContentLoaded', () => {
    // Determinar en qué vista estamos
    const currentPath = window.location.pathname;

    // Inicializar según la vista
    if (currentPath.includes('/Ventas/Index')) {
        initIndexView();
    } else if (currentPath.includes('/Ventas/Details/')) {
        initDetailsView();
    } else if (currentPath.includes('/Ventas/Edit/')) {
        initEditView();
    }
    // Configurar imágenes ampliables después de agregar los productos al DOM
    configureVentasImages();
});

// Variables globales
let estados = [];

// ==================================
// VISTA DE ÍNDICE (LISTADO DE VENTAS)
// ==================================
function initIndexView() {
    // Cargar estados de venta para el filtro
    cargarEstados()
        .then(data => {
            estados = data;
            const sel = document.getElementById('ventas-estadoFilter');
            if (sel) {
                data.forEach(e => sel.innerHTML += `<option value="${e.id}">${e.nombre}</option>`);
            }
        });

    // Configurar el checkbox de notificación sincrónica y cargar su estado guardado
    const notificacionCheckbox = document.getElementById('notificacionSincronica');
    if (notificacionCheckbox) {
        // Cargar preferencia guardada
        notificacionCheckbox.checked = localStorage.getItem('ventasNotificacionSincronica') === 'true';

        // Guardar preferencia al cambiar
        notificacionCheckbox.addEventListener('change', function () {
            localStorage.setItem('ventasNotificacionSincronica', this.checked);
            console.log("Preferencia de notificación guardada:", this.checked);
        });
    }

    // Configurar filtro por estado
    const estadoFilter = document.getElementById('ventas-estadoFilter');
    if (estadoFilter) {
        estadoFilter.addEventListener('change', e => loadVentas(e.target.value));
    }

    // Cargar todas las ventas inicialmente
    loadVentas('');

    // Configurar botón de confirmación en modal
    const confirmBtn = document.getElementById('ce-confirmBtn');
    if (confirmBtn) {
        confirmBtn.addEventListener('click', cambiarEstadoVenta);
    }
    // Configurar imágenes ampliables después de agregar los productos al DOM
    configureVentasImages();
}

// Cargar ventas (todas o por estado)
function loadVentas(estadoId) {
    const url = estadoId ? `/api/Venta/estado/${estadoId}` : '/api/Venta';
    fetch(url)
        .then(r => r.ok ? r.json() : [])
        .then(data => renderVentas(data))
        .catch(error => {
            console.error('Error al cargar ventas:', error);
            mostrarError('No se pudieron cargar las ventas');
        });
}

function renderVentas(data) {
    const tbody = document.getElementById('ventas-tbody');
    if (!tbody) return;

    tbody.innerHTML = '';

    if (data.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center py-3">
                    No hay ventas para mostrar
                </td>
            </tr>`;
        return;
    }

    data.forEach(v => {
        // Normalizar el nombre del estado para evitar problemas con tildes
        const estadoNombre = v.nombreEstadoDeLaVenta || "";
        const estadoLower = estadoNombre.toLowerCase().normalize("NFD").replace(/[\u0300-\u036f]/g, "");

        // Determinar clase y color del badge según el estado normalizado
        let badgeClass;
        switch (estadoLower) {
            case 'recibida':
                badgeClass = 'bg-info';
                break;
            case 'despachada':
                badgeClass = 'bg-warning';
                break;
            case 'entregada':
                badgeClass = 'bg-success';
                break;
            case 'devolucion solicitada':
                badgeClass = 'bg-danger';
                break;
            case 'devolucion completada':
                badgeClass = 'bg-secondary';
                break;
            default:
                badgeClass = 'bg-secondary';
        }

        // Formatear fecha usando nuestra función
        const fechaFormateada = formatearFechaAmPm(v.fechaDeRegistro);

        // Determinar qué botones mostrar según el estado
        let botonesAccion = `
            <a href="/Ventas/Details/${v.id}" class="btn btn-sm btn-secondary me-1">
                <i class="fas fa-eye"></i>
            </a>
        `;

        // Solo mostrar botón de edición si NO está en estado entregada, devolución solicitada o completada
        if (estadoLower !== 'entregada' && estadoLower !== 'devolucion solicitada' && estadoLower !== 'devolucion completada') {
            botonesAccion += `
                <a href="/Ventas/Edit/${v.id}" class="btn btn-sm btn-primary me-1">
                    <i class="fas fa-edit"></i>
                </a>
            `;
        } else {
            // Agregar un botón desactivado con tooltip explicativo
            botonesAccion += `
                <a href="javascript:void(0)" class="btn btn-sm btn-primary me-1 disabled" 
                   title="No se puede editar una venta en estado ${v.nombreEstadoDeLaVenta}">
                    <i class="fas fa-edit"></i>
                </a>
            `;
        }

        // Añadir botón de procesar devolución si el estado es "Devolución solicitada"
        if (estadoLower === 'devolucion solicitada') {
            botonesAccion += `
                <button class="btn btn-sm btn-danger" onclick="procesarDevolucion(${v.id})">
                    <i class="fa-solid fa-arrow-rotate-right"></i> Procesar
                </button>
            `;
        }

        tbody.innerHTML += `
            <tr>
                <td>${v.numeroDeFactura}</td>
                <td>${v.nombreCompletoCliente}</td>
                <td>${fechaFormateada}</td>
                <td>Q ${v.montoTotal.toFixed(2)}</td>
                <td><span class="badge ${badgeClass}">${v.nombreEstadoDeLaVenta}</span></td>
                <td>
                    ${botonesAccion}
                </td>
            </tr>`;
    });
    // Configurar imágenes ampliables después de agregar los productos al DOM
    configureVentasImages();
}

// Actualizar estado de venta desde modal
function cambiarEstadoVenta() {
    const id = document.getElementById('ce-ventaId').value;
    const nuevo = document.getElementById('ce-nuevoEstado').value;
    const estadoActual = document.getElementById('ce-estadoActual');

    // Validar si el cambio es permitido
    if (estadoActual && (estadoActual.value == 4 || estadoActual.value == 5)) {
        mostrarError('No se puede cambiar el estado de una venta con devolución en proceso o completada');
        return;
    }

    // Obtener preferencia de notificación desde checkbox o localStorage
    const checkboxElement = document.getElementById('notificacionSincronica');
    const notificacionSincronica = checkboxElement ? checkboxElement.checked :
        (localStorage.getItem('ventasNotificacionSincronica') === 'true');

    fetch(`/api/Venta/estado/edit/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            IdEstadoDeLaVenta: parseInt(nuevo),
            UsarNotificacionSincronica: notificacionSincronica
        })
    })
        .then(r => {
            if (!r.ok) throw new Error('Error al actualizar el estado');
            return r.json();
        })
        .then(data => {
            $('#cambioEstadoModal').modal('hide');

            Swal.fire({
                title: 'Estado actualizado',
                text: `${data.message}`,
                icon: 'success'
            }).then(() => {
                // Recargar la lista de ventas
                if (document.getElementById('ventas-tbody')) {
                    loadVentas(document.getElementById('ventas-estadoFilter')?.value || '');
                } else {
                    // Si estamos en la vista de detalles, redirigir al listado
                    window.location.reload();
                }
            });
        })
        .catch(error => {
            console.error('Error:', error);
            mostrarError('Error al actualizar el estado: ' + error.message);
        });
}

// Procesar devolución de venta
function procesarDevolucion(id) {
    Swal.fire({
        title: '¿Procesar devolución?',
        text: 'Esta acción actualizará el inventario devolviendo los productos. ¿Está seguro?',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Sí, procesar',
        cancelButtonText: 'Cancelar'
    }).then((result) => {
        if (result.isConfirmed) {
            // Mostrar indicador de carga
            Swal.fire({
                title: 'Procesando...',
                text: 'Devolviendo productos al inventario',
                allowOutsideClick: false,
                didOpen: () => {
                    Swal.showLoading();
                }
            });

            // Obtener preferencia de notificación desde localStorage
            const notificacionSincronica = localStorage.getItem('ventasNotificacionSincronica') === 'true';
            console.log("Modo sincrónico para devolución:", notificacionSincronica);

            // Llamar a la API para procesar la devolución
            fetch(`/api/Venta/devolucion/${id}`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    usarNotificacionSincronica: notificacionSincronica
                })
            })
                .then(async response => {
                    if (response.ok) {
                        const data = await response.json();
                        Swal.fire('Éxito', 'Devolución procesada correctamente', 'success')
                            .then(() => {
                                // Recargar la lista de ventas
                                loadVentas(document.getElementById('ventas-estadoFilter').value || '');
                            });
                    } else {
                        const errorData = await response.json();
                        Swal.fire('Error', errorData.message || 'No se pudo procesar la devolución', 'error');
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire('Error', 'Ocurrió un problema al comunicarse con el servidor', 'error');
                });
        }
    });
}

// Añadir una función específica para configurar imágenes en ventas
function configureVentasImages() {
    // Esperar un poco más para asegurar que el contenido esté renderizado
    setTimeout(() => {
        // Seleccionar todas las imágenes en las vistas de ventas
        const ventasImages = document.querySelectorAll('.detalle-producto-img, .venta-producto-img, .img-thumbnail');

        ventasImages.forEach(img => {
            if (!img.getAttribute('data-ampliable')) {
                img.setAttribute('data-ampliable', 'true');
                img.style.cursor = 'pointer';
                if (!img.title) {
                    img.title = 'Clic para ampliar';
                }

                // Eliminar eventos anteriores (por seguridad)
                img.removeEventListener('click', handleImageClick);
                // Añadir evento de clic
                img.addEventListener('click', handleImageClick);
            }
        });

        // Llamar a la función general también
        if (window.configurarImagenesAmpliables) {
            window.configurarImagenesAmpliables();
        }
    }, 500); // Esperar 500ms
}

// Función auxiliar para manejar clics en imágenes
function handleImageClick(e) {
    // Evitar propagación si es dentro de un botón
    if (e.target.closest('button')) return;

    const nombreProducto = this.getAttribute('alt') ||
        this.getAttribute('data-nombre') ||
        this.closest('[data-nombre]')?.getAttribute('data-nombre') ||
        'Producto';

    if (window.mostrarImagenAmpliada) {
        window.mostrarImagenAmpliada(this.src, nombreProducto);
    }
}

// ==================================
// VISTA DE DETALLES
// ==================================
function initDetailsView() {
    // Formatear y mostrar la fecha
    const fechaOriginal = document.getElementById('fechaOriginal');
    if (fechaOriginal) {
        const fecha = new Date(fechaOriginal.value);
        const formatoFecha = new Intl.DateTimeFormat('es', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
        }).format(fecha);

        const fechaDisplay = document.getElementById('fechaRegistroDisplay');
        if (fechaDisplay) {
            fechaDisplay.textContent = formatoFecha;
        }
    }

    // Cargar estados para el modal de cambio de estado
    cargarEstados()
        .then(data => {
            estados = data;
        });

    // Configurar botón de confirmación en modal
    const confirmBtn = document.getElementById('ce-confirmBtn');
    if (confirmBtn) {
        confirmBtn.addEventListener('click', () => {
            const id = document.getElementById('ce-ventaId').value;
            const nuevo = document.getElementById('ce-nuevoEstado').value;

            cambiarEstadoYRedirigir(id, nuevo, '/Ventas/Index');
        });
    }
    // Configurar imágenes ampliables después de agregar los productos al DOM
    configureVentasImages();
}

// ==================================
// VISTA DE EDICIÓN
// ==================================
function initEditView() {
    // Formatear y mostrar la fecha
    const fechaOriginal = document.getElementById('fechaDeRegistro');
    if (fechaOriginal && fechaOriginal.value) {
        const fechaFormateada = formatearFechaAmPm(new Date(fechaOriginal.value));
        const fechaVisible = document.getElementById('fechaDeRegistroVisible');
        if (fechaVisible) {
            fechaVisible.value = fechaFormateada;
        }
    }

    // Cargar estados disponibles
    const estadoOriginalId = document.getElementById('estadoOriginalId');
    const estadoOriginalNombre = document.getElementById('estadoOriginalNombre');
    if (estadoOriginalId) {
        const estadoId = estadoOriginalId.value;

        // Normalizar el nombre del estado para comparaciones
        const estadoNombre = estadoOriginalNombre ? estadoOriginalNombre.value.toLowerCase() : '';

        // Verificar si es un estado de devolución
        const esDevolucion =
            estadoId == 4 || estadoId == 5 ||
            estadoNombre.includes('devolucion') || estadoNombre.includes('devolución');

        if (esDevolucion) {
            // Mostrar mensaje y deshabilitar edición para estados de devolución
            const formulario = document.getElementById('formEditarEstadoVenta');
            const nuevoEstadoContainer = document.querySelector('.row.mb-4');

            if (nuevoEstadoContainer) {
                nuevoEstadoContainer.innerHTML = `
                    <div class="col-12">
                        <div class="alert alert-warning">
                            <i class="bi bi-exclamation-triangle-fill me-2"></i>
                            <strong>No se puede cambiar el estado</strong>
                            <p class="mb-0">Las ventas con estado "${estadoOriginalNombre.value}" no pueden ser modificadas.</p>
                        </div>
                    </div>
                `;
            }

            // Deshabilitar el botón de guardar
            const submitBtn = document.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.title = 'No se pueden realizar cambios en este estado';
            }

            return;
        }

        // Para otros estados, cargar opciones válidas según el flujo
        cargarEstados(true, estadoId)
            .then(estados => {
                const select = document.getElementById('nuevoEstadoSelect');
                if (select) {
                    // Limpiar opciones existentes
                    select.innerHTML = '';

                    // Añadir las opciones filtradas
                    estados.forEach(estado => {
                        const option = document.createElement('option');
                        option.value = estado.id;
                        option.textContent = estado.nombre;

                        if (estado.id == estadoId) {
                            option.selected = true;
                        }

                        select.appendChild(option);
                    });

                    // Si no hay opciones disponibles, deshabilitar select
                    if (estados.length === 0) {
                        select.innerHTML = `<option value="${estadoId}" selected>${estadoOriginalNombre.value}</option>`;
                        select.disabled = true;
                    }
                }
            });
    }

    // Mostrar el modo de notificación actual desde localStorage
    const modeSincrono = localStorage.getItem('ventasNotificacionSincronica') === 'true';
    console.log("Modo de notificación en Edit:", modeSincrono ? "Sincrónico" : "Asincrónico");

    // Actualizar el formulario para incluir el modo de notificación
    const form = document.getElementById('formEditarEstadoVenta');
    if (form) {
        form.addEventListener('submit', function (e) {
            e.preventDefault();

            const ventaId = document.getElementById('ventaId').value;
            const nuevoEstadoId = document.getElementById('nuevoEstadoSelect').value;

            cambiarEstadoYRedirigir(ventaId, nuevoEstadoId, '/Ventas/Index');
        });
    }
    // Configurar imágenes ampliables después de agregar los productos al DOM
    configureVentasImages();
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
    const anio = fechaLocal.getFullYear();

    // Formatear hora en formato 12 horas
    let horas = fechaLocal.getHours();
    const minutos = fechaLocal.getMinutes().toString().padStart(2, '0');
    const ampm = horas >= 12 ? 'p.m.' : 'a.m.';
    horas = horas % 12;
    horas = horas ? horas : 12; // La hora '0' debe mostrarse como '12'
    const horasStr = horas.toString().padStart(2, '0');

    return `${dia}/${mes}/${anio} ${horasStr}:${minutos} ${ampm}`;
}

// ==================================
// FUNCIONES COMPARTIDAS
// ==================================

// Cargar estados de venta
function cargarEstados(filtrarPorEstadoActual = false, estadoActualId = null) {
    return fetch('/api/EstadoDeLaVenta')
        .then(r => r.ok ? r.json() : [])
        .then(data => {
            // Eliminar duplicados basados en el ID
            const estadosUnicos = [];
            const idsVistos = new Set();

            data.forEach(estado => {
                if (!idsVistos.has(estado.id)) {
                    idsVistos.add(estado.id);
                    estadosUnicos.push(estado);
                }
            });

            // Filtrar estados según el contexto
            if (filtrarPorEstadoActual && estadoActualId) {
                const estadoActual = parseInt(estadoActualId);

                // Si es devolución solicitada o completada, no permitir cambios
                if (estadoActual === 4 || estadoActual === 5) {
                    return []; // No mostrar ningún estado disponible
                }

                // Solo mostrar estados válidos según el flujo normal de ventas
                return estadosUnicos.filter(estado => {
                    const id = parseInt(estado.id);

                    // No permitir cambios a estados de devolución desde la UI
                    if (id === 4 || id === 5) return false;

                    // Validar flujo correcto: Recibida -> Despachada -> Entregada
                    if (estadoActual === 1) {
                        // Desde Recibida solo se puede pasar a Despachada
                        return id === 2;
                    } else if (estadoActual === 2) {
                        // Desde Despachada solo se puede pasar a Entregada
                        return id === 3;
                    }

                    return false; // Para cualquier otro caso, no permitir cambios
                });
            }

            // Caso para el filtro de índice: mostrar todos los estados excepto devolución completada
            if (!filtrarPorEstadoActual && estadoActualId === null) {
                return estadosUnicos.filter(estado => {
                    // Para filtros, sí mostrar devolución completada
                    return true;
                });
            }

            return estadosUnicos;
        })
        .catch(error => {
            console.error('Error al cargar estados:', error);
            return [];
        });
}

// Cambiar estado y redireccionar
function cambiarEstadoYRedirigir(ventaId, nuevoEstadoId, redirectUrl) {
    // Mostrar indicador de carga
    Swal.fire({
        title: 'Procesando...',
        text: 'Actualizando estado de la venta',
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });

    // Obtener preferencia de notificación desde localStorage
    const notificacionSincronica = localStorage.getItem('ventasNotificacionSincronica') === 'true';
    console.log("Usando modo sincrónico desde localStorage:", notificacionSincronica);

    fetch(`/api/Venta/estado/edit/${ventaId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            IdEstadoDeLaVenta: parseInt(nuevoEstadoId),
            UsarNotificacionSincronica: notificacionSincronica
        })
    })
        .then(response => {
            if (response.ok) {
                // El backend ya se encarga de enviar la notificación, no hacer llamadas adicionales

                Swal.close();
                Swal.fire({
                    title: 'Estado actualizado',
                    text: 'El estado de la venta se ha actualizado correctamente y se ha enviado una notificación por email al cliente',
                    icon: 'success',
                    confirmButtonText: 'Aceptar'
                }).then(() => {
                    window.location.href = redirectUrl;
                });
            } else {
                return response.text().then(text => {
                    throw new Error(text || 'Error al actualizar el estado de la venta');
                });
            }
        })
        .catch(error => {
            console.error('Error:', error);
            Swal.fire('Error', `Error al comunicarse con el servidor: ${error.message}`, 'error');
        });
}

// Mostrar mensaje de error
function mostrarError(mensaje) {
    Swal.fire({
        title: 'Error',
        text: mensaje,
        icon: 'error',
        confirmButtonText: 'Aceptar'
    });
}