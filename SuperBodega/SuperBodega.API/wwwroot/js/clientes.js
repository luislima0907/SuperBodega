// Variables globales
let formHasChanges = false;
let clienteOriginal = null;

// Función principal que se ejecuta cuando se carga la página
document.addEventListener('DOMContentLoaded', function() {
    // Inicializar el campo de fecha de registro
    inicializarCampoFechaRegistro();

    // Detectar en qué vista estamos
    const currentPath = window.location.pathname;

    if (currentPath.includes('/Clientes/Create')) {
        // Configurar formulario de creación
        configurarFormularioCreacion();
    }
    else if (currentPath.includes('/Clientes/Edit/')) {
        // Configurar formulario de edición
        configurarFormularioEdicion();
    }
    else if (currentPath.includes('/Clientes/Index')) {
        // Configurar la página de listado (si es necesario)
        configurarPaginaListado();
    }

    // Interceptar navegación cuando hay cambios sin guardar
    setupLinkInterception();

    // Configurar buscador
    configurarBuscador();

    // Configurar el botón de limpiar para clientes
    configurarBotonLimpiarCliente();
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

// Función para que muestre los datos en la tabla
function cargarClientes() {
    // Mostrar indicador de carga en la tabla
    const tablaBody = document.querySelector('#clientesTable tbody');
    if (tablaBody) {
        tablaBody.innerHTML = '<tr><td colspan="9" class="text-center"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Cargando...</span></div></td></tr>';
    }

    fetch('/api/Cliente')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al cargar clientes');
            }
            return response.json();
        })
        .then(data => {
            if (tablaBody) {
                // Crear promesas para verificar dependencias de todos los clientes
                const verificacionPromesas = data.map(cliente =>
                    fetch(`/api/Validaciones/cliente/${cliente.id}/tieneVentasActivas`)
                        .then(response => response.json())
                        .then(resultado => ({ id: cliente.id, tieneVentasActivas: resultado.tieneVentasActivas }))
                        .catch(() => ({ id: cliente.id, tieneVentasActivas: false }))
                );

                Promise.all(verificacionPromesas)
                    .then(resultados => {
                        // Crear un mapa para acceder rápidamente a los resultados
                        const dependenciasMap = {};
                        resultados.forEach(r => dependenciasMap[r.id] = r.tieneVentasActivas);

                        // Limpiar tabla y mostrar datos
                        tablaBody.innerHTML = '';

                        if (data.length === 0) {
                            tablaBody.innerHTML = '<tr><td colspan="9" class="text-center">No se encontraron clientes</td></tr>';
                            return;
                        }

                        data.forEach(cliente => {
                            const tieneVentasActivas = dependenciasMap[cliente.id] || false;
                            const fechaFormateada = formatearFechaAmPm(cliente.fechaDeRegistro);

                            tablaBody.innerHTML += `
                                <tr>
                                    <td>${cliente.id}</td>
                                    <td>${cliente.nombre}</td>
                                    <td>${cliente.apellido || '-'}</td>
                                    <td>${cliente.email || '-'}</td>
                                    <td>${cliente.telefono || '-'}</td>
                                    <td>${cliente.direccion || '-'}</td>
                                    <td>
                                        <span class="badge ${cliente.estado ? 'bg-success' : 'bg-danger'}">
                                            ${cliente.estado ? 'Activo' : 'Inactivo'}
                                        </span>
                                    </td>
                                    <td>${fechaFormateada}</td>
                                    <td>
                                        <a href="/Clientes/Edit/${cliente.id}" 
                                          class="btn btn-sm btn-primary me-1 ${tieneVentasActivas ? 'disabled' : ''}" 
                                          ${tieneVentasActivas ? 'title="No se puede editar mientras tenga ventas activas"' : ''}>
                                            <i class="fas fa-edit"></i>
                                        </a>
                                        <button class="btn btn-sm btn-danger btn-eliminar-cliente" 
                                                data-id="${cliente.id}" 
                                                ${tieneVentasActivas ? 'disabled title="No se puede eliminar mientras tenga ventas activas"' : ''}>
                                            <i class="fas fa-trash"></i>
                                        </button>
                                    </td>
                                </tr>
                            `;
                        });

                        // Reconfigurar los botones de eliminar
                        configurarBotonesEliminar();
                    });
            }
        })
        .catch(error => {
            console.error('Error:', error);
            if (tablaBody) {
                tablaBody.innerHTML = `<tr><td colspan="9" class="text-center text-danger">Error: ${error.message}</td></tr>`;
            }
        });
}

// Función para validar que solo se ingresen letras en los campos de nombre y apellido
function configurarCamposTexto() {
    const nombreInput = document.getElementById('nombre');
    const apellidoInput = document.getElementById('apellido');

    // Función reutilizable para validar solo letras, espacios y caracteres especiales permitidos
    function validarSoloLetras(input) {
        if (input) {
            // Prevenir entrada de caracteres numéricos
            input.addEventListener('keypress', function(e) {
                // Permitir letras, espacios, acentos y algunos caracteres especiales como ñ
                const charCode = e.which || e.keyCode;
                const char = String.fromCharCode(charCode);

                // Expresión regular que permite solo letras (mayúsculas/minúsculas), espacios, y caracteres especiales 
                const regex = /^[A-Za-záéíóúÁÉÍÓÚñÑüÜ\s'-]+$/;

                // Si es un número u otro carácter no permitido, cancelar el evento
                if (!regex.test(char) && charCode > 9) {
                    e.preventDefault();
                    mostrarAlerta('Solo se permiten letras en este campo', 'warning');
                }
            });

            // Limpiar caracteres no permitidos en caso de pegar contenido
            input.addEventListener('paste', function(e) {
                e.preventDefault();
                const text = (e.originalEvent || e).clipboardData.getData('text/plain');
                const regex = /[^A-Za-záéíóúÁÉÍÓÚñÑüÜ\s'-]/g;
                const cleanedText = text.replace(regex, '');
                document.execCommand('insertText', false, cleanedText);
            });

            // Validación adicional en caso de modificación
            input.addEventListener('input', function() {
                const value = this.value;
                const regex = /[^A-Za-záéíóúÁÉÍÓÚñÑüÜ\s'-]/g;
                if (regex.test(value)) {
                    this.value = value.replace(regex, '');
                }
            });
        }
    }

    // Aplicar validación a los campos
    validarSoloLetras(nombreInput);
    validarSoloLetras(apellidoInput);
}

// Función para configurar los botones de eliminar
function configurarBotonesEliminar() {
    document.querySelectorAll('.btn-eliminar-cliente').forEach(boton => {
        boton.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            eliminarCliente(id);
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
        if (window.location.pathname.includes('/Clientes/Edit/')) {
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
    const formulario = document.getElementById('formCrearCliente');
    if (formulario) {
        formulario.addEventListener('submit', function(e) {
            e.preventDefault();
            crearCliente();
        });

        // Configurar el campo de teléfono
        configurarCampoTelefono();

        // Configurar los campos de texto para solo letras
        configurarCamposTexto();

        // Detectar cambios en el formulario
        monitorearCambiosFormulario();
    }
}

// Función para restaurar datos originales del proveedor
function restaurarDatosOriginalesCliente() {
    if (!clienteOriginal) return;

    // Restaurar valores de los campos
    document.getElementById('nombre').value = clienteOriginal.nombre || '';
    document.getElementById('apellido').value = clienteOriginal.apellido || '';
    document.getElementById('email').value = clienteOriginal.email || '';
    document.getElementById('telefono').value = clienteOriginal.telefono || '';
    document.getElementById('direccion').value = clienteOriginal.direccion || '';

    // Restaurar estado
    const estadoCheckbox = document.getElementById('estado');
    if (estadoCheckbox) {
        estadoCheckbox.checked = clienteOriginal.estado;
    }

    // Mostrar mensaje de confirmación
    mostrarAlerta('Se han restaurado los datos originales', 'info');

    // Actualizar estado de cambios
    formHasChanges = false;
}

// Configurar el botón de restaurar datos
function setupBotonRestaurar() {
    const btnRestaurar = document.getElementById('btnRestaurarCliente');
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
                    restaurarDatosOriginalesCliente();
                }
            });
        });
    }
}

// Función para verificar si ya existe un cliente con el mismo email o teléfono
function verificarDuplicados(nombre, apellido, email, telefono, id = null) {
    return fetch('/api/Cliente')
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al verificar datos duplicados');
            }
            return response.json();
        })
        .then(clientes => {
            // Filtrar los proveedores que tienen el mismo id en caso de edición
            const clientesFiltrados = id
                ? clientes.filter(c => c.id != id)
                : clientes;

            let duplicados = {
                nombre: clientesFiltrados.some(c => c.nombre.toLowerCase() === nombre.toLowerCase()),
                apellido: clientesFiltrados.some(c => c.apellido && c.apellido.toLowerCase() === apellido.toLowerCase()),
                email: clientesFiltrados.some(c => c.email && c.email.toLowerCase() === email.toLowerCase()),
                telefono: clientesFiltrados.some(c => c.telefono && c.telefono === telefono)
            };

            return duplicados;
        })
        .catch(error => {
            console.error('Error al verificar duplicados:', error);
            return { nombre: false, apellido: false, email: false, telefono: false };
        });
}

// Crear cliente
function crearCliente() {
    const formulario = document.getElementById('formCrearCliente');
    if (!formulario) return;

    // Validar el formulario
    if (!validarFormularioCliente()) {
        return;
    }

    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();

    // Verificar duplicados
    verificarDuplicados(nombre, apellido, email, telefono)
        .then(duplicados => {
            let hayDuplicados = false;

            if (duplicados.nombre) {
                document.getElementById('nombre').classList.add('is-invalid');
                mostrarAlerta('Ya existe un cliente con este nombre', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('nombre').classList.remove('is-invalid');
            }

            if (duplicados.apellido) {
                document.getElementById('apellido').classList.add('is-invalid');
                mostrarAlerta('Ya existe un cliente con este apellido', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('apellido').classList.remove('is-invalid');
            }

            if (duplicados.email) {
                document.getElementById('email').classList.add('is-invalid');
                mostrarAlerta('Este email ya está registrado para otro cliente', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('email').classList.remove('is-invalid');
            }

            if (duplicados.telefono) {
                document.getElementById('telefono').classList.add('is-invalid');
                mostrarAlerta('Este teléfono ya está registrado para otro cliente', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('telefono').classList.remove('is-invalid');
            }

            // Continuar solo si no hay duplicados
            if (!hayDuplicados) {
                enviarFormularioCliente();
            }
        });
}

// Función para validar el formulario antes de enviar
function validarFormularioCliente() {
    // Obtener los valores del formulario
    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();

    let isValid = true;

    // Validar campos requeridos
    if (!nombre) {
        isValid = false;
        document.getElementById('nombre').classList.add('is-invalid');
        mostrarAlerta('El nombre del cliente es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('nombre').classList.remove('is-invalid');
    }

    if (!apellido) {
        isValid = false;
        document.getElementById('apellido').classList.add('is-invalid');
        mostrarAlerta('El apellido del cliente es obligatorio', 'danger');
        return false;
    } else {
        document.getElementById('apellido').classList.remove('is-invalid');
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
function enviarFormularioCliente() {
    const formulario = document.getElementById('formCrearCliente');
    const estado = document.getElementById('estado').checked;
    const fechaDeRegistro = document.getElementById('fechaDeRegistro').value;

    // Crear objeto para enviar
    const clienteDto = {
        nombre: document.getElementById('nombre').value.trim(),
        apellido: document.getElementById('apellido').value.trim(),
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
    fetch('/api/Cliente/Create', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(clienteDto)
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
                text: 'El cliente ha sido creado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                window.onbeforeunload = null; // Desactivar alerta de navegador
                window.location.href = '/Clientes/Index';
            });
        })
        .catch(error => {
            console.error('Error:', error);
            let errorMsg = 'Ocurrió un error al crear el cliente';
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
    const formulario = document.getElementById('formEditCliente');
    if (formulario) {
        // Obtener el ID del proveedor de la URL
        const id = window.location.pathname.split('/').pop();

        // Configurar el campo de teléfono
        configurarCampoTelefono();

        // Configurar los campos de texto para solo letras
        configurarCamposTexto();

        // Cargar datos originales del cliente para detectar cambios
        fetch(`/api/Cliente/${id}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error('No se pudo cargar el cliente');
                }
                return response.json();
            })
            .then(cliente => {
                clienteOriginal = {
                    nombre: cliente.nombre,
                    apellido: cliente.apellido,
                    email: cliente.email,
                    telefono: cliente.telefono,
                    direccion: cliente.direccion,
                    estado: cliente.estado,
                    fechaDeRegistro: cliente.fechaDeRegistro
                };

                // Completar el formulario con los datos del cliente
                document.getElementById('idCliente').value = cliente.id;
                document.getElementById('nombre').value = cliente.nombre;
                document.getElementById('apellido').value = cliente.apellido;
                document.getElementById('email').value = cliente.email;
                document.getElementById('telefono').value = cliente.telefono;
                document.getElementById('direccion').value = cliente.direccion;
                document.getElementById('estado').checked = cliente.estado;

                // Establecer fecha de registro
                if (cliente.fechaDeRegistro) {
                    document.getElementById('fechaDeRegistro').value = cliente.fechaDeRegistro;
                    document.getElementById('fechaDeRegistroVisible').value = formatearFechaAmPm(cliente.fechaDeRegistro);
                }

                // Configurar monitoreo de cambios
                monitorearCambiosFormulario();

                // Configurar botón de restaurar datos
                setupBotonRestaurar();
            })
            .catch(error => {
                console.error('Error al cargar datos originales:', error);
                mostrarAlerta('Error al cargar datos del cliente', 'error');
            });

        // Configurar evento submit
        formulario.addEventListener('submit', function(e) {
            e.preventDefault();
            actualizarCliente();
        });
    }
}

// Actualizar cliente existente
function actualizarCliente() {
    const formulario = document.getElementById('formEditCliente');
    if (!formulario) return;

    // Validar el formulario
    if (!validarFormularioCliente()) {
        return;
    }

    // Obtener ID del cliente del campo oculto
    const id = document.getElementById('idCliente').value;
    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();

    // Verificar duplicados
    verificarDuplicados(nombre, apellido, email, telefono, id)
        .then(duplicados => {
            let hayDuplicados = false;

            if (duplicados.nombre) {
                document.getElementById('nombre').classList.add('is-invalid');
                mostrarAlerta('Ya existe un cliente con este nombre', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('nombre').classList.remove('is-invalid');
            }

            if (duplicados.apellido) {
                document.getElementById('apellido').classList.add('is-invalid');
                mostrarAlerta('Ya existe un cliente con este apellido', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('apellido').classList.remove('is-invalid');
            }

            if (duplicados.email) {
                document.getElementById('email').classList.add('is-invalid');
                mostrarAlerta('Este email ya está registrado para otro cliente', 'danger');
                hayDuplicados = true;
            } else {
                document.getElementById('email').classList.remove('is-invalid');
            }

            if (duplicados.telefono) {
                document.getElementById('telefono').classList.add('is-invalid');
                mostrarAlerta('Este teléfono ya está registrado para otro cliente', 'danger');
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
    const formulario = document.getElementById('formEditCliente');
    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').checked;

    // Crear objeto para enviar
    const clienteDto = {
        id: id,
        nombre: nombre,
        apellido: apellido,
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
    fetch(`/api/Cliente/Edit/${id}`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(clienteDto)
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
                text: 'El cliente ha sido actualizado correctamente',
                icon: 'success',
                confirmButtonText: 'Aceptar'
            }).then(() => {
                window.onbeforeunload = null; // Desactivar alerta de navegador
                window.location.href = '/Clientes/Index';
            });
        })
        .catch(error => {
            console.error('Error:', error);
            let errorMsg = 'Ocurrió un error al actualizar el cliente';
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

// Función para limpiar el formulario de clientes
function limpiarFormularioCliente() {
    const formCrearCliente = document.getElementById('formCrearCliente');
    if (formCrearCliente) {
        // Limpiar campos de texto
        formCrearCliente.querySelector('#nombre').value = '';
        formCrearCliente.querySelector('#apellido').value = '';
        formCrearCliente.querySelector('#direccion').value = '';
        formCrearCliente.querySelector('#email').value = '';
        formCrearCliente.querySelector('#telefono').value = '';

        // Restablecer el estado a activo (por defecto)
        const estadoCheckbox = formCrearCliente.querySelector('#estado');
        if (estadoCheckbox) estadoCheckbox.checked = true;

        // Remover clases de validación
        formCrearCliente.querySelectorAll('.is-invalid').forEach(el => {
            el.classList.remove('is-invalid');
        });

        // Restablecer la bandera de cambios
        formHasChanges = false;

        // Mostrar mensaje de confirmación
        mostrarAlerta('Formulario limpiado correctamente', 'info');
    }
}

// Configurar el botón de limpiar con confirmación
function configurarBotonLimpiarCliente() {
    const btnLimpiar = document.getElementById('btnLimpiarCliente');
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
                    limpiarFormularioCliente();
                }
            });
        });
    }
}

// Monitorear cambios en el formulario
function monitorearCambiosFormulario() {
    const elementos = ['nombre', 'apellido', 'email', 'telefono', 'direccion', 'estado'];

    elementos.forEach(id => {
        const elemento = document.getElementById(id);
        if (!elemento) return;

        if (elemento.tagName === 'SELECT') {
            elemento.addEventListener('change', () => {
                if (window.location.pathname.includes('/Clientes/Edit/')) {
                    formHasChanges = detectarCambiosEdicion();
                } else {
                    formHasChanges = detectarCambiosCreacion();
                }
            });
        } else {
            ['input', 'change', 'keyup'].forEach(evento => {
                elemento.addEventListener(evento, () => {
                    if (window.location.pathname.includes('/Clientes/Edit/')) {
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
    if (!clienteOriginal) return false;

    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').checked;

    return nombre !== clienteOriginal.nombre ||
        apellido !== clienteOriginal.apellido ||
        email !== clienteOriginal.email ||
        telefono !== clienteOriginal.telefono ||
        direccion !== clienteOriginal.direccion ||
        estado !== clienteOriginal.estado;
}

// Detectar cambios en el formulario de creación
function detectarCambiosCreacion() {
    const nombre = document.getElementById('nombre').value.trim();
    const apellido = document.getElementById('apellido').value.trim();
    const email = document.getElementById('email').value.trim();
    const telefono = document.getElementById('telefono').value.trim();
    const direccion = document.getElementById('direccion').value.trim();
    const estado = document.getElementById('estado').checked;

    // Verificar si algún campo no está vacío
    return nombre !== '' || apellido !== '' || email !== '' || telefono !== '' || direccion !== '' || estado !== false;
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
    cargarClientes();
    // Configurar botones de eliminar
    document.querySelectorAll('.btn-eliminar-cliente').forEach(boton => {
        boton.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            eliminarCliente(id);
        });
    });
}

// Eliminar cliente
function eliminarCliente(id) {
    Swal.fire({
        title: '¿Está seguro?',
        text: '¿Desea eliminar este cliente? Esta acción no se puede deshacer.',
        icon: 'warning',
        showCancelButton: true,
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6'
    }).then((result) => {
        if (result.isConfirmed) {
            fetch(`/api/Cliente/Delete/${id}`, {
                method: 'DELETE'
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('No se pudo eliminar el cliente');
                    }

                    Swal.fire({
                        title: '¡Eliminado!',
                        text: 'El cliente ha sido eliminado correctamente',
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
                        text: 'Ocurrió un error al eliminar el cliente',
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
    if (window.location.pathname.includes('/Clientes/Index')) {
        buscarClientes(termino);
    } else if (window.location.pathname.includes('/Clientes/Index')) {
        buscarClientes(termino);
    }
}

// Funciones específicas según vista
function buscarClientes(termino) {
    const tabla = document.getElementById('clientesTable');
    const filas = tabla.querySelectorAll('tbody tr');

    filas.forEach(fila => {
        const nombre = fila.querySelector('td:nth-child(2)').textContent.toLowerCase();
        const apellido = fila.querySelector('td:nth-child(3)').textContent.toLowerCase();
        const email = fila.querySelector('td:nth-child(3)').textContent.toLowerCase();

        if (nombre.includes(termino.toLowerCase()) || apellido.includes(termino.toLowerCase()) || email.includes(termino.toLowerCase())) {
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