/**
 * reportes.js - Maneja todas las funcionalidades de reportes
 */
document.addEventListener('DOMContentLoaded', () => {
    const currentPath = window.location.pathname;

    // Determinar qu� vista de reportes est� activa
    if (currentPath.includes('VentasPorPeriodo')) {
        initReportePorPeriodo();
    } else if (currentPath.includes('VentasPorCliente')) {
        initReportePorCliente();
    } else if (currentPath.includes('VentasPorProducto')) {
        initReportePorProducto();
    } else if (currentPath.includes('VentasPorProveedor')) {
        initReportePorProveedor();
    }
});

// Reporte por Per�odo de Tiempo
function initReportePorPeriodo() {
    console.log('Inicializando reporte por per�odo');

    // Resetear las variables de fechas de b�squeda
    ultimaFechaInicioBusqueda = null;
    ultimaFechaFinBusqueda = null;

    // Configurar selector de fecha de inicio
    const fechaInicio = document.getElementById('fechaInicio');
    if (fechaInicio) {
        // Establecer como valor predeterminado el primer d�a del mes actual
        const primerDiaDelMes = new Date();
        primerDiaDelMes.setDate(1);
        fechaInicio.valueAsDate = primerDiaDelMes;
    }

    // Configurar selector de fecha fin
    const fechaFin = document.getElementById('fechaFin');
    if (fechaFin) {
        // Establecer como valor predeterminado hoy
        fechaFin.valueAsDate = new Date();
    }

    // Configurar bot�n de b�squeda
    const btnBuscar = document.getElementById('btnBuscarPorPeriodo');
    if (btnBuscar) {
        btnBuscar.addEventListener('click', buscarVentasPorPeriodo);
    }

    // Configurar bot�n de exportaci�n
    const btnExportar = document.getElementById('btnExportarPorPeriodo');
    if (btnExportar) {
        btnExportar.addEventListener('click', exportarVentasPorPeriodo);

        // Deshabilitar bot�n de exportaci�n inicialmente
        btnExportar.disabled = true;
    }

    // Configurar bot�n de exportaci�n PDF
    const btnExportarPDF = document.getElementById('btnExportarPorPeriodoPDF');
    if (btnExportarPDF) {
        btnExportarPDF.addEventListener('click', exportarVentasPorPeriodoPDF);

        // Deshabilitar bot�n de exportaci�n inicialmente
        btnExportarPDF.disabled = true;
    }
}

// Reporte por Cliente
function initReportePorCliente() {
    console.log('Inicializando reporte por cliente');

    // Cargar lista de clientes
    fetch('/api/Cliente')
        .then(response => response.json())
        .then(clientes => {
            const clienteSelect = document.getElementById('clienteSelect');
            if (clienteSelect) {
                clienteSelect.innerHTML = '<option value="">Seleccione un cliente</option>';

                clientes.forEach(cliente => {
                    const option = document.createElement('option');
                    option.value = cliente.id;
                    option.textContent = `${cliente.nombre || ''} ${cliente.apellido || ''}`.trim();
                    clienteSelect.appendChild(option);
                });
            }
        })
        .catch(error => {
            console.error('Error al cargar clientes:', error);
            mostrarError('No se pudieron cargar los clientes');
        });

    // Configurar evento de cambio de cliente
    const clienteSelect = document.getElementById('clienteSelect');
    if (clienteSelect) {
        clienteSelect.addEventListener('change', buscarVentasPorCliente);
    }

    // Configurar bot�n de exportaci�n
    const btnExportar = document.getElementById('btnExportarPorCliente');
    if (btnExportar) {
        btnExportar.addEventListener('click', exportarVentasPorCliente);

        // Deshabilitar bot�n de exportaci�n inicialmente
        btnExportar.disabled = true;
    }

    // Configurar bot�n de exportaci�n PDF
    const btnExportarPDF = document.getElementById('btnExportarPorClientePDF');
    if (btnExportarPDF) {
        btnExportarPDF.addEventListener('click', exportarVentasPorClientePDF);

        // Deshabilitar bot�n de exportaci�n inicialmente
        btnExportarPDF.disabled = true;
    }
}

// Reporte por Producto
function initReportePorProducto() {
    console.log('Inicializando reporte por producto');

    // Cargar lista de productos
    fetch('/api/Producto/GetAll')
        .then(response => response.json())
        .then(productos => {
            const productoSelect = document.getElementById('productoSelect');
            if (productoSelect) {
                productoSelect.innerHTML = '<option value="">Seleccione un producto</option>';

                productos.forEach(producto => {
                    const option = document.createElement('option');
                    option.value = producto.id;
                    option.textContent = `${producto.codigo} - ${producto.nombre}`;
                    productoSelect.appendChild(option);
                });
            }
        })
        .catch(error => {
            console.error('Error al cargar productos:', error);
            mostrarError('No se pudieron cargar los productos');
        });

    // Configurar evento de cambio de producto
    const productoSelect = document.getElementById('productoSelect');
    if (productoSelect) {
        productoSelect.addEventListener('change', buscarVentasPorProducto);
    }

    // Configurar bot�n de exportaci�n
    const btnExportar = document.getElementById('btnExportarPorProducto');
    if (btnExportar) {
        btnExportar.addEventListener('click', exportarVentasPorProducto);

        // Deshabilitar bot�n de exportaci�n inicialmente
        btnExportar.disabled = true;
    }

    // Configurar bot�n de exportaci�n PDF
    const btnExportarPDF = document.getElementById('btnExportarPorProductoPDF');
    if (btnExportarPDF) {
        btnExportarPDF.addEventListener('click', exportarVentasPorProductoPDF);

        // Deshabilitar bot�n de exportaci�n inicialmente
        btnExportarPDF.disabled = true;
    }
}

// Reporte por Proveedor
function initReportePorProveedor() {
    console.log('Inicializando reporte por proveedor');

    // Cargar lista de proveedores
    fetch('/api/Proveedor')
        .then(response => response.json())
        .then(proveedores => {
            const proveedorSelect = document.getElementById('proveedorSelect');
            if (proveedorSelect) {
                proveedorSelect.innerHTML = '<option value="">Seleccione un proveedor</option>';

                proveedores.forEach(proveedor => {
                    const option = document.createElement('option');
                    option.value = proveedor.id;
                    option.textContent = proveedor.nombre;
                    proveedorSelect.appendChild(option);
                });
            }
        })
        .catch(error => {
            console.error('Error al cargar proveedores:', error);
            mostrarError('No se pudieron cargar los proveedores');
        });

    // Configurar evento de cambio de proveedor
    const proveedorSelect = document.getElementById('proveedorSelect');
    if (proveedorSelect) {
        proveedorSelect.addEventListener('change', buscarVentasPorProveedor);
    }

    // Configurar bot�n de exportaci�n
    const btnExportar = document.getElementById('btnExportarPorProveedor');
    if (btnExportar) {
        btnExportar.addEventListener('click', exportarVentasPorProveedor);

        // Deshabilitar bot�n de exportaci�n inicialmente
        btnExportar.disabled = true;
    }

    // Configurar bot�n de exportaci�n PDF
    const btnExportarPDF = document.getElementById('btnExportarPorProveedorPDF');
    if (btnExportarPDF) {
        btnExportarPDF.addEventListener('click', exportarVentasPorProveedorPDF);

        // Deshabilitar bot�n de exportaci�n inicialmente
        btnExportarPDF.disabled = true;
    }
}

// Variables para almacenar las fechas de la �ltima b�squeda exitosa
let ultimaFechaInicioBusqueda = null;
let ultimaFechaFinBusqueda = null;

// Funciones para buscar ventas
function buscarVentasPorPeriodo() {
    const fechaInicio = document.getElementById('fechaInicio').value;
    const fechaFin = document.getElementById('fechaFin').value;

    if (!fechaInicio || !fechaFin) {
        mostrarError('Debe seleccionar ambas fechas');
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando();

    // Deshabilitar los botones de exportaci�n al iniciar una nueva b�squeda
    const btnExportar = document.getElementById('btnExportarPorPeriodo');
    const btnExportarPDF = document.getElementById('btnExportarPorPeriodoPDF');
    if (btnExportar) btnExportar.disabled = true;
    if (btnExportarPDF) btnExportarPDF.disabled = true;

    fetch(`/api/Reporte/periodo?fechaInicio=${fechaInicio}&fechaFin=${fechaFin}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('No se encontraron ventas para el per�odo seleccionado');
            }
            return response.json();
        })
        .then(ventas => {
            ocultarCargando();

            // Verificar si hay ventas en el resultado
            if (ventas.length === 0) {
                document.getElementById('ventasTableBody').innerHTML = `
                    <tr>
                        <td colspan="5" class="text-center">No se encontraron ventas para este periodo</td>
                    </tr>
                `;
                // Mantener los botones deshabilitados
                return;
            }

            // Renderizar tabla y actualizar variables solo si hay ventas
            renderizarTablaVentas(ventas, 'fecha');

            // Guardar las fechas de la b�squeda exitosa
            ultimaFechaInicioBusqueda = fechaInicio;
            ultimaFechaFinBusqueda = fechaFin;

            // Habilitar botones de exportaci�n solo si hay ventas
            if (btnExportar) btnExportar.disabled = false;
            if (btnExportarPDF) btnExportarPDF.disabled = false;
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al buscar ventas');

            // Asegurarse de que los botones queden deshabilitados en caso de error
            document.getElementById('ventasTableBody').innerHTML = `
                <tr>
                    <td colspan="5" class="text-center">No se encontraron ventas para este periodo</td>
                </tr>
            `;
        });
}

function buscarVentasPorCliente() {
    const clienteId = document.getElementById('clienteSelect').value;

    if (!clienteId) {
        // Limpiar tabla y deshabilitar bot�n de exportaci�n
        document.getElementById('ventasTableBody').innerHTML = '';
        document.getElementById('btnExportarPorCliente').disabled = true;
        document.getElementById('btnExportarPorClientePDF').disabled = true;
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando();

    fetch(`/api/Reporte/cliente/${clienteId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('No se encontraron ventas para el cliente seleccionado');
            }
            return response.json();
        })
        .then(ventas => {
            ocultarCargando();
            renderizarTablaVentas(ventas, 'default');

            // Habilitar bot�n de exportaci�n
            const btnExportar = document.getElementById('btnExportarPorCliente');
            if (btnExportar) {
                btnExportar.disabled = false;
            }

            // Habilitar bot�n de exportaci�n PDF
            const btnExportarPDF = document.getElementById('btnExportarPorClientePDF');
            if (btnExportarPDF) {
                btnExportarPDF.disabled = false;
            }
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al buscar ventas para el cliente');
        });
}

function buscarVentasPorProducto() {
    const productoId = document.getElementById('productoSelect').value;

    if (!productoId) {
        // Limpiar tabla y deshabilitar bot�n de exportaci�n
        document.getElementById('ventasTableBody').innerHTML = '';
        document.getElementById('btnExportarPorProducto').disabled = true;
        document.getElementById('btnExportarPorProductoPDF').disabled = true;
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando();

    fetch(`/api/Reporte/producto/${productoId}`)
        .then(response => {
            if (response.status === 404 || response.status === 204) {
                // If no data, show friendly message instead of error
                ocultarCargando();
                document.getElementById('ventasTableBody').innerHTML = `
                    <tr>
                        <td colspan="5" class="text-center">No se encontraron ventas para este producto</td>
                    </tr>
                `;
                document.getElementById('btnExportarPorProducto').disabled = true;
                document.getElementById('btnExportarPorProductoPDF').disabled = true;
                return Promise.reject(new Error('No hay datos'));
            }

            if (!response.ok) {
                return response.text().then(text => {
                    try {
                        const errorData = JSON.parse(text);
                        throw new Error(errorData.mensaje || `Error ${response.status}`);
                    } catch (e) {
                        throw new Error(`Error ${response.status}: ${response.statusText}`);
                    }
                });
            }
            return response.json();
        })
        .then(ventas => {
            ocultarCargando();
            renderizarTablaVentas(ventas, 'producto');

            // Enable/disable export buttons based on data presence
            const hasData = ventas.length > 0;

            // Excel button
            const btnExportar = document.getElementById('btnExportarPorProducto');
            if (btnExportar) {
                btnExportar.disabled = !hasData;
            }

            // PDF button
            const btnExportarPDF = document.getElementById('btnExportarPorProductoPDF');
            if (btnExportarPDF) {
                btnExportarPDF.disabled = !hasData;
            }
        })
        .catch(error => {
            if (error.message !== 'No hay datos') {
                ocultarCargando();
                console.error('Error:', error);
                mostrarError(error.message || 'Error al buscar ventas para el producto');
            }
        });
}

function buscarVentasPorProveedor() {
    const proveedorId = document.getElementById('proveedorSelect').value;

    if (!proveedorId) {
        // Limpiar tabla y deshabilitar bot�n de exportaci�n
        document.getElementById('ventasTableBody').innerHTML = '';
        document.getElementById('btnExportarPorProveedor').disabled = true;
        document.getElementById('btnExportarPorProveedorPDF').disabled = true;
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando();

    fetch(`/api/Reporte/proveedor/${proveedorId}`)
        .then(response => {
            if (response.status === 404 || response.status === 204) {
                // If no data, show friendly message instead of error
                ocultarCargando();
                document.getElementById('ventasTableBody').innerHTML = `
                    <tr>
                        <td colspan="5" class="text-center">No se encontraron ventas para este proveedor</td>
                    </tr>
                `;
                document.getElementById('btnExportarPorProveedor').disabled = true;
                document.getElementById('btnExportarPorProveedorPDF').disabled = true;
                return Promise.reject(new Error('No hay datos'));
            }

            if (!response.ok) {
                return response.text().then(text => {
                    try {
                        const errorData = JSON.parse(text);
                        throw new Error(errorData.mensaje || `Error ${response.status}`);
                    } catch (e) {
                        throw new Error(`Error ${response.status}: ${response.statusText}`);
                    }
                });
            }
            return response.json();
        })
        .then(ventas => {
            ocultarCargando();
            renderizarTablaVentas(ventas, 'proveedor');

            // Enable/disable both export buttons based on data presence
            const hasData = ventas.length > 0;

            // Excel button
            const btnExportar = document.getElementById('btnExportarPorProveedor');
            if (btnExportar) {
                btnExportar.disabled = !hasData;
            }

            // PDF button
            const btnExportarPDF = document.getElementById('btnExportarPorProveedorPDF');
            if (btnExportarPDF) {
                btnExportarPDF.disabled = !hasData;
            }
        })
        .catch(error => {
            if (error.message !== 'No hay datos') {
                ocultarCargando();
                console.error('Error:', error);
                mostrarError(error.message || 'Error al buscar ventas para el proveedor');
            }
        });
}

// Funciones para exportar a Excel
function exportarVentasPorPeriodo() {
    // Usar las fechas almacenadas de la �ltima b�squeda exitosa
    if (!ultimaFechaInicioBusqueda || !ultimaFechaFinBusqueda) {
        mostrarError('Debe realizar una b�squeda primero');
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando('Generando reporte Excel...');

    fetch(`/api/Reporte/exportar/periodo?fechaInicio=${ultimaFechaInicioBusqueda}&fechaFin=${ultimaFechaFinBusqueda}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al generar el reporte');
            }
            return response.json();
        })
        .then(data => {
            ocultarCargando();

            // Mostrar mensaje de �xito con enlace de descarga
            Swal.fire({
                title: 'Reporte generado',
                html: `El reporte ha sido generado correctamente.<br><a href="${data.url}" target="_blank" class="btn btn-primary mt-3">Descargar Excel</a>`,
                icon: 'success',
                confirmButtonText: 'Aceptar'
            });
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al exportar reporte');
        });
}

// A�adir la funci�n para exportar a PDF por periodo
function exportarVentasPorPeriodoPDF() {
    // Usar las fechas almacenadas de la �ltima b�squeda exitosa
    if (!ultimaFechaInicioBusqueda || !ultimaFechaFinBusqueda) {
        mostrarError('Debe realizar una b�squeda primero');
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando('Generando reporte PDF...');

    fetch(`/api/Reporte/exportar/periodo/pdf?fechaInicio=${ultimaFechaInicioBusqueda}&fechaFin=${ultimaFechaFinBusqueda}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al generar el reporte PDF');
            }
            return response.json();
        })
        .then(data => {
            ocultarCargando();

            // Mostrar mensaje de �xito con enlace de descarga
            Swal.fire({
                title: 'Reporte PDF generado',
                html: `El reporte PDF ha sido generado correctamente.<br><a href="${data.url}" target="_blank" class="btn btn-primary mt-3">Descargar PDF</a>`,
                icon: 'success',
                confirmButtonText: 'Aceptar'
            });
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al exportar reporte PDF');
        });
}

function exportarVentasPorCliente() {
    const clienteId = document.getElementById('clienteSelect').value;

    if (!clienteId) {
        mostrarError('Debe seleccionar un cliente');
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando('Generando reporte Excel...');

    fetch(`/api/Reporte/exportar/cliente/${clienteId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al generar el reporte');
            }
            return response.json();
        })
        .then(data => {
            ocultarCargando();

            // Mostrar mensaje de �xito con enlace de descarga
            Swal.fire({
                title: 'Reporte generado',
                html: `El reporte ha sido generado correctamente.<br><a href="${data.url}" target="_blank" class="btn btn-primary mt-3">Descargar Excel</a>`,
                icon: 'success',
                confirmButtonText: 'Aceptar'
            });
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al exportar reporte');
        });
}

// Agregar la funci�n de exportaci�n a PDF
function exportarVentasPorClientePDF() {
    const clienteId = document.getElementById('clienteSelect').value;

    if (!clienteId) {
        mostrarError('Debe seleccionar un cliente');
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando('Generando reporte PDF...');

    fetch(`/api/Reporte/exportar/cliente/${clienteId}/pdf`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al generar el reporte PDF');
            }
            return response.json();
        })
        .then(data => {
            ocultarCargando();

            // Mostrar mensaje de �xito con enlace de descarga
            Swal.fire({
                title: 'Reporte PDF generado',
                html: `El reporte PDF ha sido generado correctamente.<br><a href="${data.url}" target="_blank" class="btn btn-primary mt-3">Descargar PDF</a>`,
                icon: 'success',
                confirmButtonText: 'Aceptar'
            });
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al exportar reporte PDF');
        });
}

// Implementar funciones similares para exportar por producto y proveedor
function exportarVentasPorProducto() {
    const productoId = document.getElementById('productoSelect').value;

    if (!productoId) {
        mostrarError('Debe seleccionar un producto');
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando('Generando reporte Excel...');

    fetch(`/api/Reporte/exportar/producto/${productoId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al generar el reporte');
            }
            return response.json();
        })
        .then(data => {
            ocultarCargando();

            // Mostrar mensaje de �xito con enlace de descarga
            Swal.fire({
                title: 'Reporte generado',
                html: `El reporte ha sido generado correctamente.<br><a href="${data.url}" target="_blank" class="btn btn-primary mt-3">Descargar Excel</a>`,
                icon: 'success',
                confirmButtonText: 'Aceptar'
            });
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al exportar reporte');
        });
}

function exportarVentasPorProductoPDF() {
    const productoId = document.getElementById('productoSelect').value;

    if (!productoId) {
        mostrarError('Debe seleccionar un producto');
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando('Generando reporte PDF...');

    fetch(`/api/Reporte/exportar/producto/${productoId}/pdf`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al generar el reporte PDF');
            }
            return response.json();
        })
        .then(data => {
            ocultarCargando();

            // Mostrar mensaje de �xito con enlace de descarga
            Swal.fire({
                title: 'Reporte PDF generado',
                html: `El reporte PDF ha sido generado correctamente.<br><a href="${data.url}" target="_blank" class="btn btn-primary mt-3">Descargar PDF</a>`,
                icon: 'success',
                confirmButtonText: 'Aceptar'
            });
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al exportar reporte PDF');
        });
}

function exportarVentasPorProveedor() {
    const proveedorId = document.getElementById('proveedorSelect').value;

    if (!proveedorId) {
        mostrarError('Debe seleccionar un proveedor');
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando('Generando reporte Excel...');

    fetch(`/api/Reporte/exportar/proveedor/${proveedorId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al generar el reporte');
            }
            return response.json();
        })
        .then(data => {
            ocultarCargando();

            // Mostrar mensaje de �xito con enlace de descarga
            Swal.fire({
                title: 'Reporte generado',
                html: `El reporte ha sido generado correctamente.<br><a href="${data.url}" target="_blank" class="btn btn-primary mt-3">Descargar Excel</a>`,
                icon: 'success',
                confirmButtonText: 'Aceptar'
            });
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al exportar reporte');
        });
}

function exportarVentasPorProveedorPDF() {
    const proveedorId = document.getElementById('proveedorSelect').value;

    if (!proveedorId) {
        mostrarError('Debe seleccionar un proveedor');
        return;
    }

    // Mostrar indicador de carga
    mostrarCargando('Generando reporte PDF...');

    fetch(`/api/Reporte/exportar/proveedor/${proveedorId}/pdf`)
        .then(response => {
            if (!response.ok) {
                throw new Error('Error al generar el reporte PDF');
            }
            return response.json();
        })
        .then(data => {
            ocultarCargando();

            // Mostrar mensaje de �xito con enlace de descarga
            Swal.fire({
                title: 'Reporte PDF generado',
                html: `El reporte PDF ha sido generado correctamente.<br><a href="${data.url}" target="_blank" class="btn btn-primary mt-3">Descargar PDF</a>`,
                icon: 'success',
                confirmButtonText: 'Aceptar'
            });
        })
        .catch(error => {
            ocultarCargando();
            console.error('Error:', error);
            mostrarError(error.message || 'Error al exportar reporte PDF');
        });
}

// Funci�n para renderizar la tabla de ventas
function renderizarTablaVentas(ventas, caseTable = 'default') {
    const tableBody = document.getElementById('ventasTableBody');

    if (!tableBody) return;

    tableBody.innerHTML = '';

    if (ventas.length === 0) {
        tableBody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center">No se encontraron ventas</td>
            </tr>
        `;
        return;
    }

    if (caseTable === 'default') {
        ventas.forEach(venta => {
            // Formatear fecha
            const fechaFormateada = formatearFechaAmPm(venta.fechaDeRegistro);

            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${venta.numeroDeFactura}</td>
                <td>${venta.cliente.nombreCompleto}</td>
                <td>${fechaFormateada}</td>
                <td>Q ${venta.montoTotal.toFixed(2)}</td>
                <td>
                    <button class="btn btn-sm btn-secondary" onclick="mostrarDetallesVenta(${venta.id})">
                        <i class="fas fa-eye"></i> Ver detalles
                    </button>
                </td>
            `;

            tableBody.appendChild(row);
        });
    }
    else if (caseTable === 'producto') {
        ventas.forEach(venta => {
            // Find the first detail containing product info (should be filtered by the API to only contain the selected product)
            const productoDetalle = venta.detalles && venta.detalles.length > 0 ? venta.detalles[0] : null;

            // Formatear fecha
            const fechaFormateada = formatearFechaAmPm(venta.fechaDeRegistro);

            // Extract category name if available
            const categoria = productoDetalle?.nombreCategoria || 'Sin categor�a';

            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${venta.numeroDeFactura}</td>
                <td>
                    <div class="d-flex align-items-center">
                        <img src="${productoDetalle?.imagenDelProducto || '/images/productos/default.png'}" 
                             class="me-2 img-thumbnail" 
                             style="width: 50px; height: 50px; object-fit: cover;" 
                             alt="${productoDetalle?.nombreDelProducto || 'Producto'}">
                        <div>
                            <span class="fw-bold">${productoDetalle ? `${productoDetalle.codigoDelProducto} - ${productoDetalle.nombreDelProducto}` : 'Producto no disponible'}</span>
                            <br>
                            <small class="text-muted">${categoria}</small>
                        </div>
                    </div>
                </td>
                <td>${fechaFormateada}</td>
                <td>Q ${venta.montoTotal.toFixed(2)}</td>
                <td>
                    <button class="btn btn-sm btn-secondary" onclick="mostrarDetallesVenta(${venta.id})">
                        <i class="fas fa-eye"></i> Ver detalles
                    </button>
                </td>
            `;

            tableBody.appendChild(row);
        });
    }
    else if (caseTable === 'proveedor') {
        ventas.forEach(venta => {
            // Formatear fecha
            const fechaFormateada = formatearFechaAmPm(venta.fechaDeRegistro);
            const proveedor = venta.proveedor?.nombre || 'No disponible';

            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${venta.numeroDeFactura}</td>
                <td>${proveedor}</td>
                <td>${fechaFormateada}</td>
                <td>Q ${venta.montoTotal.toFixed(2)}</td>
                <td>
                    <button class="btn btn-sm btn-secondary" onclick="mostrarDetallesVenta(${venta.id})">
                        <i class="fas fa-eye"></i> Ver detalles
                    </button>
                </td>
            `;

            tableBody.appendChild(row);
        });
    }
    else if (caseTable === 'fecha') {
        ventas.forEach(venta => {
            // Formatear fecha
            const fechaFormateada = formatearFechaAmPm(venta.fechaDeRegistro);

            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${venta.numeroDeFactura}</td>
                <td>${fechaFormateada}</td>
                <td>Q ${venta.montoTotal.toFixed(2)}</td>
                <td>
                    <button class="btn btn-sm btn-secondary" onclick="mostrarDetallesVenta(${venta.id})">
                        <i class="fas fa-eye"></i> Ver detalles
                    </button>
                </td>
            `;

            tableBody.appendChild(row);
        });
    }
}

// Funci�n para mostrar detalles de venta
function mostrarDetallesVenta(ventaId) {
    // Mostrar cargando
    mostrarCargando('Cargando detalles...');

    fetch(`/api/Venta/${ventaId}`)
        .then(response => {
            if (!response.ok) {
                throw new Error('No se pudieron cargar los detalles');
            }
            return response.json();
        })
        .then(venta => {
            ocultarCargando();
            console.log("Detalles de venta:", venta);

            const montoTotal = venta.montoTotal || venta.MontoTotal || 0;
            const montoPago = venta.montoDePago || venta.MontoDePago || venta.montoPago || venta.MontoPago || 0;
            const montoCambio = venta.montoDeCambio || venta.MontoDeCambio || venta.montoCambio || venta.MontoCambio || 0;
            const clienteNombre = venta.nombreCompletoCliente || venta.NombreCompletoCliente || venta.cliente?.nombreCompleto || 'No disponible';
            const clienteEmail = venta.emailCliente || venta.EmailCliente || venta.cliente?.email || 'No disponible';

            let detallesHTML = `
                <div class="mb-3">
                    <p><strong>Cliente:</strong> ${clienteNombre}</p>
                    <p><strong>Email:</strong> ${clienteEmail}</p>
                    <p><strong>Fecha:</strong> ${formatearFechaAmPm(venta.fechaDeRegistro || venta.FechaDeRegistro)}</p>
                    <p><strong>Estado:</strong> ${venta.nombreEstadoDeLaVenta || venta.NombreEstadoDeLaVenta || 'No disponible'}</p>
                </div>
                <table class="table table-striped">
                    <thead>
                        <tr>
                            <th>C�digo</th>
                            <th>Producto</th>
                            <th>Categor�a</th>
                            <th>Precio</th>
                            <th>Cantidad</th>
                            <th>Subtotal</th>
                        </tr>
                    </thead>
                    <tbody>
            `;

            // Check which property contains the details
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
                            <td>${detalle.nombreCategoria || detalle.NombreCategoria || detalle.producto?.categoria?.nombre || 'Sin categor�a'}</td>
                            <td>Q ${(detalle.precioDeVenta || detalle.PrecioDeVenta || 0).toFixed(2)}</td>
                            <td>${detalle.cantidad || detalle.Cantidad || 0}</td>
                            <td>Q ${((detalle.precioDeVenta || detalle.PrecioDeVenta || 0) * (detalle.cantidad || detalle.Cantidad || 0)).toFixed(2)}</td>
                        </tr>
                    `;
                });
            } else {
                detallesHTML += `
                    <tr>
                        <td colspan="6" class="text-center">No hay detalles disponibles para esta venta</td>
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
            `;

            // Mostrar modal con los detalles
            Swal.fire({
                title: `Detalles de venta #${venta.numeroDeFactura || venta.NumeroDeFactura}`,
                html: detallesHTML,
                width: '800px',
                confirmButtonText: 'Cerrar'
            });
        })
        .catch(error => {
            ocultarCargando();
            mostrarError('No se pudieron cargar los detalles de la venta');
            console.error('Error:', error);
        });
}

// Formatear fecha con formato AM/PM
function formatearFechaAmPm(fechaStr) {
    // Si no hay fecha, devolver cadena vac�a
    if (!fechaStr) return '';

    // Crear objeto Date a partir de la cadena
    const fecha = new Date(fechaStr);

    // Verificar si la fecha es v�lida
    if (isNaN(fecha.getTime())) return 'Fecha inv�lida';

    // Ajustamos la fecha a la zona horaria local expl�citamente
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

// Funciones auxiliares
function mostrarError(mensaje) {
    Swal.fire({
        title: 'Error',
        text: mensaje,
        icon: 'error',
        confirmButtonText: 'Aceptar'
    });
}

function mostrarCargando(mensaje = 'Cargando...') {
    Swal.fire({
        title: mensaje,
        allowOutsideClick: false,
        didOpen: () => {
            Swal.showLoading();
        }
    });
}

function ocultarCargando() {
    Swal.close();
}