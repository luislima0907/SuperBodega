// Variables globales
let formHasChanges = false;
let proveedorOriginal = null;

// Función principal que se ejecuta cuando se carga la página
document.addEventListener('DOMContentLoaded', function() {
    // Inicializar el campo de fecha de registro
    inicializarCampoFechaRegistro();

    // Detectar en qué vista estamos
    const currentPath = window.location.pathname;

    if (currentPath.includes('/Proveedores/Create')) {
        // Configurar formulario de creación
        configurarFormularioCreacion();
    }
    else if (currentPath.includes('/Proveedores/Edit/')) {
        // Configurar formulario de edición
        configurarFormularioEdicion();
    }
    else if (currentPath.includes('/Proveedores/Index')) {
        // Configurar la página de listado (si es necesario)
        configurarPaginaListado();
    }

    // Interceptar navegación cuando hay cambios sin guardar
    setupLinkInterception();

    // Configurar buscador
    configurarBuscador();

    // Configurar el botón de limpiar para proveedores
    configurarBotonLimpiarProveedor();
});

// Función para formatear fechas en formato AM/PM
function formatearFechaAmPm(fechaStr) {
    // Si no hay fecha, devolver cadena vacía
    if (!fechaStr) return '';

    // Crear objeto Date a partir de la cadena
    const fecha = new Date(fechaStr);

    // Verificar si la fecha es válida
    if (isNaN(fecha.getTime())) return 'Fecha inválida';

    // Obtener componentes de fecha y hora UTC para evitar desplazamientos por zona horaria
    const dia = fecha.getDate().toString().padStart(2, '0');
    const mes = (fecha.getMonth() + 1).toString().padStart(2, '0');
    const año = fecha.getFullYear();

    // Formatear hora en formato 12 horas
    let horas = fecha.getHours();
    const minutos = fecha.getMinutes().toString().padStart(2, '0');
    const ampm = horas >= 12 ? 'p.m.' : 'a.m.';
    horas = horas % 12;
    horas = horas ? horas : 12; // La hora '0' debe mostrarse como '12'
    const horasStr = horas.toString().padStart(2, '0');

    return `${dia}/${mes}/${año} ${horasStr}:${minutos} ${ampm}`;
}

// Ajustar la función cargarProveedores para que muestre los datos en la tabla
function cargarProveedores() {
    // Mostrar indicador de carga en la tabla
    const tablaBody = document.querySelector('#proveedoresTable tbody');
    if (tablaBody) {
        tablaBody.innerHTML = '<tr><td colspan="8" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';
    }

    fetch('/api/Proveedor')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar proveedores');
            }
            return response.json();
        })
        .then(data => {
            if (tablaBody) {
                // Limpiar tabla y mostrar datos
                tablaBody.innerHTML = '';

                if (data.length === 0) {
                    tablaBody.innerHTML = '<tr><td colspan="8" class="text-center">No se encontraron proveedores</td></tr>';
                    return;
                }

                data.forEach(proveedor => {
                    const fechaFormateada = formatearFechaAmPm(proveedor.fechaDeRegistro);

                    tablaBody.innerHTML += `
                        <tr>
                            <td>${proveedor.id}</td>
                            <td>${proveedor.nombre}</td>
                            <td>${proveedor.email || '-'}</td>
                            <td>${proveedor.telefono || '-'}</td>
                            <td>${proveedor.direccion || '-'}</td>
                            <td>
                                <span class="badge ${proveedor.estado ? 'bg-success' : 'bg-danger'}">
                                    ${proveedor.estado ? 'Activo' : 'Inactivo'}
                                </span>
                            </td>
                            <td>${fechaFormateada}</td>
                            <td>
                                <a href="/Proveedores/Edit/${proveedor.id}" class="btn btn-sm btn-primary me-1">
                                    <i class="fas fa-edit"></i>
                                </a>
                                <button class="btn btn-sm btn-danger btn-eliminar-proveedor" data-id="${proveedor.id}">
                                    <i class="fas fa-trash"></i>
                                </button>
                            </td>
                        </tr>
                    `;
                });

                // Reconfigurar los botones de eliminar
                configurarBotonesEliminar();
            }
        })
        .catch(error => {
            console.error('Error:', error);
            if (tablaBody) {
                tablaBody.innerHTML = `<tr><td colspan="8" class="text-center text-danger">Error: ${error.message}</td></tr>`;
            }
        });
}

// Función para configurar los botones de eliminar
function configurarBotonesEliminar() {
    document.querySelectorAll('.btn-eliminar-proveedor').forEach(boton => {
        boton.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            eliminarProveedor(id);
        });
    });
}

// Función para validar que solo se ingresen números en el campo teléfono
function configurarCampoTelefono() {
    const telefonoInput = document.getElementById('telefono');
    if (telefonoInput) {
        // Prevenir entrada de caracteres no numéricos
        telefonoInput.addEventListener('keypress', function(e) {
            // Obtener el código de la tecla pulsada
            const charCode = (e.which) ? e.which : e.keyCode;

            // Permitir solo números (códigos ASCII del 48 al 57)
            if (charCode > 31 && (charCode < 48 || charCode > 57)) {
                e.preventDefault();
                return false;
            }
            return true;
        });

        // Limpiar caracteres no numéricos en caso de pegar contenido
        telefonoInput.addEventListener('paste', function(e) {
            // Prevenir comportamiento por defecto para manejar manualmente
            e.preventDefault();

            // Obtener texto del portapapeles
            const pastedText = (e.clipboardData || window.clipboardData).getData('text');

            // Filtrar solo números del texto pegado
            const filteredText = pastedText.replace(/[^0-9]/g, '');

            // Insertar solo los números
            if (document.execCommand) {
                document.execCommand('insertText', false, filteredText);
            } else {
                // Alternativa para navegadores que no soporten execCommand
                this.value = this.value.substring(0, this.selectionStart) +
                    filteredText +
                    this.value.substring(this.selectionEnd);
            }
        });

        // Validación adicional en caso de modificación
        telefonoInput.addEventListener('input', function() {
            // Eliminar cualquier carácter no numérico que pudiera haberse colado
            this.value = this.value.replace(/[^0-9]/g, '');
        });
    }
}

// Inicializar campo de fecha de registro
function inicializarCampoFechaRegistro() {
    const fechaInput = document.getElementById('fechaDeRegistro');
    const fechaVisibleInput = document.getElementById('fechaDeRegistroVisible');

    if (fechaInput && fechaVisibleInput) {
        // Verificar si estamos en el formulario de edición
        if (window.location.pathname.includes('/Proveedores/Edit/')) {
            // La fecha ya debería estar establecida desde el servidor
            // Solo aseguramos que la fecha visible esté formateada correctamente
            if (fechaInput.value) {
                fechaVisibleInput.value = formatearFechaAmPm(fechaInput.value);
            }
        } else {
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
}

// Configurar formulario de creación
function configurarFormularioCreacion() {
    const formulario = document.getElementById('formCrearProveedor');
    if (formulario) {
        formulario.addEventListener('submit', function(e) {
            e.preventDefault();
            crearProveedor();
        });

        // Configurar el campo de teléfono
        configurarCampoTelefono();

        // Detectar cambios en el formulario
        monitorearCambiosFormulario();
    }
}

// Función para restaurar datos originales del proveedor
function restaurarDatosOriginalesProveedor() {
    if (!proveedorOriginal) return;

    // Restaurar valores de los campos
    document.getElementById('nombre').value = proveedorOriginal.nombre;
    document.getElementById('email').value = proveedorOriginal.email || '';
    document.getElementById('telefono').value = proveedorOriginal.telefono || '';
    document.getElementById('direccion').value = proveedorOriginal.direccion || '';

    // Restaurar estado
    const estadoCheckbox = document.getElementById('estado');
    if (estadoCheckbox) {
        estadoCheckbox.checked = proveedorOriginal.estado;
    }

    // Mostrar mensaje de confirmación
    mostrarAlerta('Se han restaurado los datos originales', 'info');

    // Actualizar estado de cambios
    formHasChanges = false;
}

// Configurar el botón de restaurar datos
function setupBotonRestaurar() {
    const btnRestaurar = document.getElementById('btnRestaurarProveedor');
    if (btnRestaurar) {
        btnRestaurar.addEventListener('click', function(e) {
            e.preventDefault();

            Swal.fire({
                title: '¿Restaurar datos?',
                text: '¿Está seguro que desea restaurar todos los campos a sus valores originales?',
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Sí, restaurar',
                cancelButtonText: 'Cancelar',
                confirmButtonColor: '#3085d6',
                cancelButtonColor: '#6c757d'
            }).then((result) => {
                if (result.isConfirmed) {
                    restaurarDatosOriginalesProveedor();
                }
            });
        });
    }
}

// Función para verificar si ya existe un proveedor con el mismo email o teléfono
function verificarDuplicados(nombre, email, telefono, id = null) {
    return fetch('/api/Proveedor')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar datos duplicados');
            }
            return response.json();
        })
        .then(proveedores => {
            // Filtrar los proveedores que tienen el mismo id en caso de edición
            const proveedoresFiltrados = id
                ? proveedores.filter(p => p.id != id)
                : proveedores;

            let duplicados = {
                nombre: proveedoresFiltrados.some(p => p.nombre.toLowerCase() === nombre.toLowerCase()),
                email: proveedoresFiltrados.some(p => p.email && p.email.toLowerCase() === email.toLowerCase()),
                telefono: proveedoresFiltrados.some(p => p.telefono && p.telefono === telefono)
            };

            return duplicados;
        })
        .catch(error => {
            console.error('Error al verificar duplicados:', error);
            return { nombre: false, email: false, telefono: false };
        });
}

// Crear proveedor
function crearProveedor() {
    const formulario = document.getElementById('formCrearProveedor');
    if (!formulario) return;

    // Validar el formulario
    if (!validarFormularioProveedor()) {
        return;
    }

    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();

    // Verificar duplicados
    verificarDuplicados(nombre, email, telefono)
        .then(duplicados => {
            let hayDuplicados = false;

            if (duplicados.nombre) {
                document.getElementById('nombre').classList.add('is-invalid');
                mostrarAlerta('Ya existe un proveedor con este nombre', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('nombre').classList.remove('is-invalid');
            }

            if (duplicados.email) {
                document.getElementById('email').classList.add('is-invalid');
                mostrarAlerta('Este email ya está registrado para otro proveedor', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('email').classList.remove('is-invalid');
            }

            if (duplicados.telefono) {
                document.getElementById('telefono').classList.add('is-invalid');
                mostrarAlerta('Este teléfono ya está registrado para otro proveedor', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('telefono').classList.remove('is-invalid');
            }

            // Continuar solo si no hay duplicados
            if (!hayDuplicados) {
                enviarFormularioProveedor();
            }
        });
}

// Función para validar el formulario antes de enviar
function validarFormularioProveedor() {
    // Obtener los valores del formulario
    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').value === 'true';
    const fechaDeRegistro = document.getElementById('fechaDeRegistro').value;

    let isValid = true;

    // Validar campos requeridos
    if (!nombre) {
        isValid = false;
        document.getElementById('nombre').classList.add('is-invalid');
        mostrarAlerta('El nombre del proveedor es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('nombre').classList.remove('is-invalid');
    }

    if (!email) {
        isValid = false;
        document.getElementById('email').classList.add('is-invalid');
        mostrarAlerta('El email es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('email').classList.remove('is-invalid');
    }

    if (!telefono) {
        isValid = false;
        document.getElementById('telefono').classList.add('is-invalid');
        mostrarAlerta('El teléfono es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('telefono').classList.remove('is-invalid');
    }

    if (!direccion) {
        isValid = false;
        document.getElementById('direccion').classList.add('is-invalid');
        mostrarAlerta('La dirección es obligatoria', 'danger');
        return false;
    } else {
        document.getElementById('direccion').classList.remove('is-invalid');
    }

    return isValid;
}

// Función para enviar el formulario después de validar
function enviarFormularioProveedor() {
    const formulario = document.getElementById('formCrearProveedor');
    const estado = document.getElementById('estado').checked;
    const fechaDeRegistro = document.getElementById('fechaDeRegistro').value;

    // Crear objeto para enviar
    const proveedorDto = {
        nombre: document.getElementById('nombre').value.trim(),
        email: document.getElementById('email').value.trim(),
        telefono: document.getElementById('telefono').value.trim(),
        direccion: document.getElementById('direccion').value.trim(),
        estado: estado,
        fechaDeRegistro: fechaDeRegistro
    };

    // Mostrar indicador de carga
    const btnSubmit = formulario.querySelector('button[type="submit"]');
    const btnText = btnSubmit.textContent;
    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Guardando...';

    // Enviar solicitud al servidor
    fetch('/api/Proveedor/Create', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(proveedorDto)
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text);
                });
            }
            return response.json();
        })
        .then(data => {
            formHasChanges = false;
            Swal.fire({
                title: '¡Éxito!',
                text: 'El proveedor ha sido creado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                window.onbeforeunload = null; // Desactivar alerta de navegador
                window.location.href = '/Proveedores/Index';
            });
        })
        .catch(error => {
            console.error('Error:', error);
            let errorMsg = 'Ocurrió un error al crear el proveedor';
            try {
                const errorObj = JSON.parse(error.message);
                if (errorObj.errors) {
                    errorMsg = Object.values(errorObj.errors).flat().join('\n');
                } else if (errorObj.title) {
                    errorMsg = errorObj.title;
                }
            } catch (e) {
                errorMsg = error.message;
            }

            Swal.fire({
                title: 'Error',
                text: errorMsg,
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        })
        .finally(() => {
            btnSubmit.disabled = false;
            btnSubmit.textContent = btnText;
        });
}

// Configurar formulario de edición
function configurarFormularioEdicion() {
    const formulario = document.getElementById('formEditProveedor');
    if (formulario) {
        // Obtener el ID del proveedor de la URL
        const id = window.location.pathname.split('/').pop();

        // Configurar el campo de teléfono
        configurarCampoTelefono();

        // Cargar datos originales del proveedor para detectar cambios
        fetch(`/api/Proveedor/${id}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('No se pudo cargar el proveedor');
                }
                return response.json();
            })
            .then(proveedor => {
                proveedorOriginal = {
                    nombre: proveedor.nombre,
                    email: proveedor.email,
                    telefono: proveedor.telefono,
                    direccion: proveedor.direccion,
                    estado: proveedor.estado,
                    fechaDeRegistro: proveedor.fechaDeRegistro
                };

                // Completar el formulario con los datos del proveedor
                document.getElementById('idProveedor').value = proveedor.id;
                document.getElementById('nombre').value = proveedor.nombre;
                document.getElementById('email').value = proveedor.email;
                document.getElementById('telefono').value = proveedor.telefono;
                document.getElementById('direccion').value = proveedor.direccion;
                document.getElementById('estado').checked = proveedor.estado;

                // Establecer fecha de registro
                if (proveedor.fechaDeRegistro) {
                    document.getElementById('fechaDeRegistro').value = proveedor.fechaDeRegistro;
                    document.getElementById('fechaDeRegistroVisible').value = formatearFechaAmPm(proveedor.fechaDeRegistro);
                }

                // Configurar monitoreo de cambios
                monitorearCambiosFormulario();

                // Configurar botón de restaurar datos
                setupBotonRestaurar();
            })
            .catch(error => {
                console.error('Error al cargar datos originales:', error);
                mostrarAlerta('Error al cargar datos del proveedor', 'error');
            });

        // Configurar evento submit
        formulario.addEventListener('submit', function(e) {
            e.preventDefault();
            actualizarProveedor();
        });
    }
}

// Actualizar proveedor existente
function actualizarProveedor() {
    const formulario = document.getElementById('formEditProveedor');
    if (!formulario) return;

    // Validar el formulario
    if (!validarFormularioProveedor()) {
        return;
    }

    // Obtener ID del proveedor del campo oculto
    const id = document.getElementById('idProveedor').value;
    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();

    // Verificar duplicados
    verificarDuplicados(nombre, email, telefono, id)
        .then(duplicados => {
            let hayDuplicados = false;

            if (duplicados.nombre) {
                document.getElementById('nombre').classList.add('is-invalid');
                mostrarAlerta('Ya existe un proveedor con este nombre', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('nombre').classList.remove('is-invalid');
            }

            if (duplicados.email) {
                document.getElementById('email').classList.add('is-invalid');
                mostrarAlerta('Este email ya está registrado para otro proveedor', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('email').classList.remove('is-invalid');
            }

            if (duplicados.telefono) {
                document.getElementById('telefono').classList.add('is-invalid');
                mostrarAlerta('Este teléfono ya está registrado para otro proveedor', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('telefono').classList.remove('is-invalid');
            }

            // Continuar solo si no hay duplicados
            if (!hayDuplicados) {
                enviarFormularioEdicion(id);
            }
        });
}

function enviarFormularioEdicion(id){
    // Obtener los valores del formulario
    const formulario = document.getElementById('formEditProveedor');
    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').checked;

    // Crear objeto para enviar
    const proveedorDto = {
        id: id, // Incluir el ID en el objeto
        nombre: nombre,
        email: email,
        telefono: telefono,
        direccion: direccion,
        estado: estado
    };

    // Mostrar indicador de carga
    const btnSubmit = formulario.querySelector('button[type="submit"]');
    const btnText = btnSubmit.textContent;
    btnSubmit.disabled = true;
    btnSubmit.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> Actualizando...';

    // Enviar solicitud al servidor
    fetch(`/api/Proveedor/Edit/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(proveedorDto)
    })
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => {
                    throw new Error(text);
                });
            }
            return response.json();
        })
        .then(data => {
            formHasChanges = false;
            Swal.fire({
                title: '¡Éxito!',
                text: 'El proveedor ha sido actualizado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                window.onbeforeunload = null; // Desactivar alerta de navegador
                window.location.href = '/Proveedores/Index';
            });
        })
        .catch(error => {
            console.error('Error:', error);
            let errorMsg = 'Ocurrió un error al actualizar el proveedor';
            try {
                const errorObj = JSON.parse(error.message);
                if (errorObj.errors) {
                    errorMsg = Object.values(errorObj.errors).flat().join('\n');
                } else if (errorObj.title) {
                    errorMsg = errorObj.title;
                }
            } catch (e) {
                errorMsg = error.message;
            }

            Swal.fire({
                title: 'Error',
                text: errorMsg,
                icon: 'error',
                confirmButtonText: 'Aceptar'
            });
        })
        .finally(() => {
            btnSubmit.disabled = false;
            btnSubmit.textContent = btnText;
        });
}

// Función para limpiar el formulario de proveedores
function limpiarFormularioProveedor() {
    const formCrearProveedor = document.getElementById('formCrearProveedor');
    if (formCrearProveedor) {
        // Limpiar campos de texto
        formCrearProveedor.querySelector('#nombre').value = '';
        formCrearProveedor.querySelector('#direccion').value = '';
        formCrearProveedor.querySelector('#email').value = '';
        formCrearProveedor.querySelector('#telefono').value = '';

        // Restablecer el estado a activo (por defecto)
        const estadoCheckbox = formCrearProveedor.querySelector('#estado');
        if (estadoCheckbox) estadoCheckbox.checked = true;

        // Remover clases de validación
        formCrearProveedor.querySelectorAll('.is-invalid').forEach(el => {
            el.classList.remove('is-invalid');
        });

        // Restablecer la bandera de cambios
        formHasChanges = false;

        // Mostrar mensaje de confirmación
        mostrarAlerta('Formulario limpiado correctamente', 'info');
    }
}

// Configurar el botón de limpiar con confirmación
function configurarBotonLimpiarProveedor() {
    const btnLimpiar = document.getElementById('btnLimpiarProveedor');
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
                    limpiarFormularioProveedor();
                }
            });
        });
    }
}

// Monitorear cambios en el formulario
function monitorearCambiosFormulario() {
    const elementos = ['nombre', 'email', 'telefono', 'direccion', 'estado'];

    elementos.forEach(id => {
        const elemento = document.getElementById(id);
        if (!elemento) return;

        if (elemento.tagName === 'SELECT') {
            elemento.addEventListener('change', () => {
                if (window.location.pathname.includes('/Proveedores/Edit/')) {
                    formHasChanges = detectarCambiosEdicion();
                } else {
                    formHasChanges = detectarCambiosCreacion();
                }
            });
        } else {
            ['input', 'change', 'keyup'].forEach(evento => {
                elemento.addEventListener(evento, () => {
                    if (window.location.pathname.includes('/Proveedores/Edit/')) {
                        formHasChanges = detectarCambiosEdicion();
                    } else {
                        formHasChanges = detectarCambiosCreacion();
                    }
                });
            });
        }
    });
}

// Detectar cambios en el formulario de edición
function detectarCambiosEdicion() {
    if (!proveedorOriginal) return false;

    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').checked;

    return nombre !== proveedorOriginal.nombre ||
        email !== proveedorOriginal.email ||
        telefono !== proveedorOriginal.telefono ||
        direccion !== proveedorOriginal.direccion ||
        estado !== proveedorOriginal.estado;
}

// Detectar cambios en el formulario de creación
function detectarCambiosCreacion() {
    const nombre = document.getElementById('nombre').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').checked;

    // Verificar si algún campo no está vacío
    return nombre !== '' || email !== '' || telefono !== '' || direccion !== '' || estado !== false;
}

// Función para interceptar clics en enlaces
function setupLinkInterception() {
    // Variable para controlar si la navegación fue aprobada
    let navegacionAprobada = false;

    // Interceptar enlaces
    const enlaces = document.querySelectorAll('a[href]:not([href^="#"]):not([href^="javascript"])');

    enlaces.forEach(enlace => {
        enlace.addEventListener('click', function(e) {
            // Ignorar si es un evento de menú de sidebar
            if (e.isSidebarMenuEvent ||
                (enlace.closest('.submenu') && enlace.getAttribute('href').startsWith('#'))) {
                return true;
            }

            // Si hay cambios sin guardar
            if (formHasChanges && !navegacionAprobada) {
                e.preventDefault();

                // Guardar URL destino
                const urlDestino = this.getAttribute('href');

                // Confirmar salida
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
                        // Desactivar alerta del navegador
                        window.onbeforeunload = null;
                        // Redirigir
                        navegacionAprobada = true;
                        window.location.href = urlDestino;
                    }
                });
            }
        });
    });

    // Alerta al cerrar ventana o cambiar página
    window.onbeforeunload = function(e) {
        if (formHasChanges && !navegacionAprobada) {
            const mensaje = 'Hay cambios sin guardar. ¿Está seguro que desea salir?';
            e.returnValue = mensaje;
            return mensaje;
        }
    };
}

// Configurar página de listado
function configurarPaginaListado() {
    cargarProveedores();
    // Configurar botones de eliminar
    document.querySelectorAll('.btn-eliminar-proveedor').forEach(boton => {
        boton.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            eliminarProveedor(id);
        });
    });
}

// Eliminar proveedor
function eliminarProveedor(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: '¿Desea eliminar este proveedor? Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/api/Proveedor/Delete/${id}`, {
                method: 'DELETE'
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('No se pudo eliminar el proveedor');
                    }

                    Swal.fire({
                        title: '¡Eliminado!',
                        text: 'El proveedor ha sido eliminado correctamente',
                        icon: 'success',
                        confirmButtonText: 'Aceptar'
                    }).then(() => {
                        // Remover fila de la tabla o recargar la página
                        const fila = document.querySelector(`tr[data-id="${id}"]`);
                        if (fila) {
                            fila.remove();
                        } else {
                            window.location.reload();
                        }
                    });
                })
                .catch(error => {
                    console.error('Error:', error);
                    Swal.fire({
                        title: 'Error',
                        text: 'Ocurrió un error al eliminar el proveedor',
                        icon: 'error',
                        confirmButtonText: 'Aceptar'
                    });
                });
        }
    });
}

// Función para configurar buscador
function configurarBuscador() {
    const searchInput = document.getElementById('searchInput');
    const searchButton = document.getElementById('searchButton');

    if (searchInput && searchButton) {
        // Realizar búsqueda al hacer clic
        searchButton.addEventListener('click', function() {
            realizarBusqueda(searchInput.value.trim());
        });

        // Realizar búsqueda al presionar Enter
        searchInput.addEventListener('keyup', function(e) {
            if (e.key === 'Enter') {
                realizarBusqueda(searchInput.value.trim());
            }
        });

        // boton para limpiar busqueda
        const clearButton = document.getElementById('clearSearchButton');
        if (clearButton) {
            clearButton.addEventListener('click', function() {
                searchInput.value = '';
                realizarBusqueda('');
            });
        }
    }
}

// Función para realizar búsqueda
function realizarBusqueda(termino) {
    // Implementar según la vista actual
    if (window.location.pathname.includes('/Proveedores/Index')) {
        buscarProveedores(termino);
    } else if (window.location.pathname.includes('/Productos/Index')) {
        buscarProductos(termino);
    }
}

// Funciones específicas según vista
function buscarProveedores(termino) {
    const tabla = document.getElementById('proveedoresTable');
    const filas = tabla.querySelectorAll('tbody tr');

    filas.forEach(fila => {
        const nombre = fila.querySelector('td:nth-child(2)').textContent.toLowerCase();
        const email = fila.querySelector('td:nth-child(3)').textContent.toLowerCase();

        if (nombre.includes(termino.toLowerCase()) || email.includes(termino.toLowerCase())) {
            fila.style.display = '';
        } else {
            fila.style.display = 'none';
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