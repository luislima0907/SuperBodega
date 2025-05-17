using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.DTOs.Admin;
using System.IO;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SuperBodega.API.Controllers.Admin;

[ApiController]
[Route("api/[controller]")]
public class ReporteController : ControllerBase
{
    private readonly SuperBodegaContext _context;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<ReporteController> _logger;

    public ReporteController(
        SuperBodegaContext context,
        IWebHostEnvironment hostingEnvironment,
        ILogger<ReporteController> logger)
    {
        _context = context;
        _hostingEnvironment = hostingEnvironment;
        _logger = logger;
    }

    // Obtener ventas por rango de fechas
    [HttpGet("periodo")]
    public async Task<IActionResult> GetVentasPorPeriodo([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            // Asegurar que la fecha fin tenga hora 23:59:59
            fechaFin = fechaFin.Date.AddDays(1).AddTicks(-1);

            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.FechaDeRegistro >= fechaInicio && v.FechaDeRegistro <= fechaFin)
                .Where(v => v.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .OrderByDescending(v => v.FechaDeRegistro)
                .Select(v => new VentaReporteDTO
                {
                    Id = v.Id,
                    NumeroDeFactura = v.NumeroDeFactura,
                    FechaDeRegistro = v.FechaDeRegistro,
                    IdCliente = v.IdCliente,
                    Cliente = new ClienteReporteDTO
                    {
                        Id = v.Cliente.Id,
                        NombreCompleto = v.Cliente != null ? $"{v.Cliente.Nombre} {v.Cliente.Apellido}" : "Cliente no encontrado",
                        Email = v.Cliente.Email,
                        Telefono = v.Cliente.Telefono,
                        Direccion = v.Cliente.Direccion
                    },
                    MontoTotal = v.MontoTotal,
                    MontoDePago = v.MontoDePago,
                    MontoDeCambio = v.MontoDeCambio,
                    NombreEstadoDeLaVenta = v.EstadoDeLaVenta.Nombre,
                    Detalles = v.DetallesDeLaVenta.Select(d => new DetalleVentaReporteDTO
                    {
                        IdProducto = d.IdProducto,
                        NombreDelProducto = d.Producto.Nombre,
                        CodigoDelProducto = d.Producto.Codigo,
                        ImagenDelProducto = d.Producto.ImagenUrl,
                        PrecioDeVenta = d.PrecioDeVenta,
                        Cantidad = d.Cantidad,
                        Subtotal = d.PrecioDeVenta * d.Cantidad
                    }).ToList()
                })
                .ToListAsync();

            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener las ventas por per�odo: {ex.Message}" });
        }
    }

    // Obtener ventas por cliente
    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> GetVentasPorCliente(int clienteId)
    {
        try
        {
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.IdCliente == clienteId)
                .Where(v => v.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .OrderByDescending(v => v.FechaDeRegistro)
                .Select(v => new VentaReporteDTO
                {
                    Id = v.Id,
                    NumeroDeFactura = v.NumeroDeFactura,
                    FechaDeRegistro = v.FechaDeRegistro,
                    IdCliente = v.IdCliente,
                    Cliente = new ClienteReporteDTO
                    {
                        Id = v.Cliente.Id,
                        NombreCompleto = v.Cliente != null ? $"{v.Cliente.Nombre} {v.Cliente.Apellido}" : "Cliente no encontrado",
                        Email = v.Cliente.Email,
                        Telefono = v.Cliente.Telefono,
                        Direccion = v.Cliente.Direccion
                    },
                    MontoTotal = v.MontoTotal,
                    MontoDePago = v.MontoDePago,
                    MontoDeCambio = v.MontoDeCambio,
                    NombreEstadoDeLaVenta = v.EstadoDeLaVenta.Nombre,
                    Detalles = v.DetallesDeLaVenta.Select(d => new DetalleVentaReporteDTO
                    {
                        IdProducto = d.IdProducto,
                        NombreDelProducto = d.Producto.Nombre,
                        CodigoDelProducto = d.Producto.Codigo,
                        ImagenDelProducto = d.Producto.ImagenUrl,
                        PrecioDeVenta = d.PrecioDeVenta,
                        Cantidad = d.Cantidad,
                        Subtotal = d.PrecioDeVenta * d.Cantidad
                    }).ToList()
                })
                .ToListAsync();

            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener las ventas por cliente: {ex.Message}" });
        }
    }

    // Obtener ventas por producto
    [HttpGet("producto/{productoId}")]
    public async Task<IActionResult> GetVentasPorProducto(int productoId)
    {
        try
        {
            // First check if product exists
            var producto = await _context.Productos
                .Include(p => p.Categoria)  // Make sure to include the category
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null)
            {
                return NotFound(new { mensaje = "El producto no fue encontrado" });
            }

            // Store the category name for later use
            var nombreCategoria = producto.Categoria?.Nombre ?? "Sin categor�a";

            // Get all sales IDs that have details with this product and are "entregada"
            var ventasIds = await _context.DetallesDeLaVenta
                .Where(d => d.IdProducto == productoId)
                .Where(d => d.Venta.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .Select(d => d.IdVenta)
                .Distinct()
                .ToListAsync();

            if (!ventasIds.Any())
            {
                return NotFound(new { mensaje = "No se encontraron ventas para el producto especificado" });
            }

            // Now query the Ventas table directly like in the client method
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .Where(v => ventasIds.Contains(v.Id))
                .OrderByDescending(v => v.FechaDeRegistro)
                .Select(v => new VentaReporteDTO
                {
                    Id = v.Id,
                    NumeroDeFactura = v.NumeroDeFactura,
                    FechaDeRegistro = v.FechaDeRegistro,
                    IdCliente = v.IdCliente,
                    Cliente = new ClienteReporteDTO
                    {
                        Id = v.Cliente != null ? v.Cliente.Id : 0,
                        NombreCompleto = v.Cliente != null ? $"{v.Cliente.Nombre} {v.Cliente.Apellido}" : "Cliente no encontrado",
                        Email = v.Cliente != null ? v.Cliente.Email : "",
                        Telefono = v.Cliente != null ? v.Cliente.Telefono : "",
                        Direccion = v.Cliente != null ? v.Cliente.Direccion : ""
                    },
                    MontoTotal = v.MontoTotal,
                    MontoDePago = v.MontoDePago,
                    MontoDeCambio = v.MontoDeCambio,
                    NombreEstadoDeLaVenta = v.EstadoDeLaVenta.Nombre,
                    Detalles = v.DetallesDeLaVenta
                        .Where(d => d.IdProducto == productoId)
                        .Select(d => new DetalleVentaReporteDTO
                        {
                            IdProducto = d.IdProducto,
                            NombreDelProducto = d.Producto != null ? d.Producto.Nombre : "Producto desconocido",
                            CodigoDelProducto = d.Producto != null ? d.Producto.Codigo : "Sin c�digo",
                            ImagenDelProducto = d.Producto != null ? d.Producto.ImagenUrl : "",
                            PrecioDeVenta = d.PrecioDeVenta,
                            Cantidad = d.Cantidad,
                            Subtotal = d.PrecioDeVenta * d.Cantidad,
                            NombreCategoria = d.Producto != null && d.Producto.Categoria != null ? d.Producto.Categoria.Nombre : nombreCategoria // Fixed category name access
                        }).ToList()
                })
                .ToListAsync();

            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener las ventas por producto: {ex.Message}" });
        }
    }

    // Obtener ventas por proveedor
    [HttpGet("proveedor/{proveedorId}")]
    public async Task<IActionResult> GetVentasPorProveedor(int proveedorId)
    {
        try
        {
            // First check if provider exists
            var proveedor = await _context.Proveedores.FindAsync(proveedorId);
            if (proveedor == null)
            {
                return NotFound(new { mensaje = "El proveedor no fue encontrado" });
            }

            // Get all sales IDs that have details with this provider
            var ventasIds = await _context.DetallesDeLaVenta
                .Where(d => d.IdProveedor == proveedorId)
                .Where(d => d.Venta.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .Select(d => d.IdVenta)
                .Distinct()
                .ToListAsync();

            if (!ventasIds.Any())
            {
                return NotFound(new { mensaje = "No se encontraron ventas para el proveedor especificado" });
            }

            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                .Where(v => ventasIds.Contains(v.Id))
                .OrderByDescending(v => v.FechaDeRegistro)
                .Select(v => new VentaReporteDTO
                {
                    Id = v.Id,
                    NumeroDeFactura = v.NumeroDeFactura,
                    FechaDeRegistro = v.FechaDeRegistro,
                    IdCliente = v.IdCliente,
                    Cliente = new ClienteReporteDTO
                    {
                        Id = v.Cliente != null ? v.Cliente.Id : 0,
                        NombreCompleto = v.Cliente != null ? $"{v.Cliente.Nombre} {v.Cliente.Apellido}" : "Cliente no encontrado",
                        Email = v.Cliente != null ? v.Cliente.Email : "",
                        Telefono = v.Cliente != null ? v.Cliente.Telefono : "",
                        Direccion = v.Cliente != null ? v.Cliente.Direccion : ""
                    },
                    MontoTotal = v.MontoTotal,
                    MontoDePago = v.MontoDePago,
                    MontoDeCambio = v.MontoDeCambio,
                    NombreEstadoDeLaVenta = v.EstadoDeLaVenta.Nombre,
                    Proveedor = new ProveedorReporteDTO
                    {
                        Id = proveedor.Id,
                        Nombre = proveedor.Nombre,
                        Email = proveedor.Email,
                        Telefono = proveedor.Telefono,
                        Direccion = proveedor.Direccion
                    },
                    Detalles = v.DetallesDeLaVenta
                        .Where(d => d.IdProveedor == proveedorId)
                        .Select(d => new DetalleVentaReporteDTO
                        {
                            IdProducto = d.IdProducto,
                            NombreDelProducto = d.Producto != null ? d.Producto.Nombre : "Producto desconocido",
                            CodigoDelProducto = d.Producto != null ? d.Producto.Codigo : "Sin c�digo",
                            ImagenDelProducto = d.Producto != null ? d.Producto.ImagenUrl : "/images/productos/default.png",
                            PrecioDeVenta = d.PrecioDeVenta,
                            Cantidad = d.Cantidad,
                            Subtotal = d.PrecioDeVenta * d.Cantidad,
                            Proveedor = new ProveedorReporteDTO
                            {
                                Id = proveedor.Id,
                                Nombre = proveedor.Nombre,
                                Email = proveedor.Email,
                                Telefono = proveedor.Telefono,
                                Direccion = proveedor.Direccion
                            }
                        }).ToList()
                })
                .ToListAsync();

            return Ok(ventas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = $"Error al obtener las ventas por proveedor: {ex.Message}" });
        }
    }

    // Exportar ventas a Excel por per�odo
    [HttpGet("exportar/periodo")]
    public async Task<IActionResult> ExportarVentasPorPeriodo([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            _logger.LogInformation("Exportando ventas para el periodo: {FechaInicio} a {FechaFin}", fechaInicio, fechaFin);

            // Asegurar que la fecha fin tenga hora 23:59:59
            fechaFin = fechaFin.Date.AddDays(1).AddTicks(-1);

            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .Where(v => v.FechaDeRegistro >= fechaInicio && v.FechaDeRegistro <= fechaFin)
                .Where(v => v.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .OrderByDescending(v => v.FechaDeRegistro)
                .ToListAsync();

            if (!ventas.Any())
            {
                _logger.LogWarning("No se encontraron ventas para el per�odo especificado");
                return NotFound(new { mensaje = "No se encontraron ventas para el per�odo especificado" });
            }

            _logger.LogInformation("Ventas encontradas: {VentasCount}", ventas.Count);

            // Crear el archivo Excel
            var fileName = $"Ventas_Periodo_{fechaInicio:yyyyMMdd}_a_{fechaFin:yyyyMMdd}_{DateTime.Now:HHmmss}.xlsx";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "reportes", fileName);

            // Asegurarse de que el directorio exista
            Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));

            using (var package = new ExcelPackage())
            {
                _logger.LogInformation("Creando archivo Excel en: {FilePath}", filePath);
                var worksheet = package.Workbook.Worksheets.Add("Ventas");

                // T�tulo principal del reporte
                worksheet.Cells[3, 1].Value = "Reporte de Ventas por Periodo";
                worksheet.Cells[3, 1, 3, 9].Merge = true;
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Style.Font.Size = 16;
                worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[3, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(180, 180, 180));

                // Manejo de zona horaria para Guatemala
                TimeZoneInfo guatemalaZone;
                try
                {
                    // Intentar formato Windows
                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                }
                catch
                {
                    try
                    {
                        // Intentar formato Linux
                        guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                    }
                    catch
                    {
                        // Crear zona manualmente con UTC-6 para Guatemala
                        guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                    }
                }

                // Fecha y hora del reporte
                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
                worksheet.Cells[4, 1].Value = $"Fecha del Reporte: {localDateTime:dd/MM/yyyy hh:mm tt}";
                worksheet.Cells[4, 1, 4, 9].Merge = true;
                worksheet.Cells[4, 1].Style.Font.Italic = true;
                worksheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[4, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 220, 220));

                // Informaci�n del periodo
                worksheet.Cells[6, 1].Value = "Informaci�n del Periodo";
                worksheet.Cells[6, 1, 6, 6].Merge = true;
                worksheet.Cells[6, 1].Style.Font.Bold = true;
                worksheet.Cells[6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[6, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[6, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                worksheet.Cells[7, 1].Value = "Fecha Inicio";
                worksheet.Cells[7, 2].Value = fechaInicio.ToString("dd/MM/yyyy");
                worksheet.Cells[8, 1].Value = "Fecha Fin";
                worksheet.Cells[8, 2].Value = fechaFin.ToString("dd/MM/yyyy");
                worksheet.Cells[9, 1].Value = "Total Ventas";
                worksheet.Cells[9, 2].Value = ventas.Count;
                worksheet.Cells[9, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheet.Cells[10, 1].Value = "Monto Total";
                worksheet.Cells[10, 2].Value = $"Q {ventas.Sum(v => v.MontoTotal):F2}";

                // Estilo para informaci�n del periodo
                using (var range = worksheet.Cells[7, 1, 10, 1])
                {
                    _logger.LogInformation("Estableciendo estilos para informaci�n del periodo");
                    range.Style.Font.Bold = true;
                }

                _logger.LogInformation("Estableciendo encabezados de ventas en la hoja de Excel");
                // Encabezados de ventas
                worksheet.Cells[12, 1].Value = "Factura #";
                worksheet.Cells[12, 2].Value = "Cliente";
                worksheet.Cells[12, 3].Value = "Fecha";
                worksheet.Cells[12, 4].Value = "Producto";
                worksheet.Cells[12, 5].Value = "Categor�a";
                worksheet.Cells[12, 6].Value = "Cantidad";
                worksheet.Cells[12, 7].Value = "Precio";
                worksheet.Cells[12, 8].Value = "Subtotal";
                worksheet.Cells[12, 9].Value = "Total";

                // Estilo para encabezados
                using (var range = worksheet.Cells[12, 1, 12, 9])
                {
                    _logger.LogInformation("Estableciendo estilos para encabezados de ventas");
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                int row = 13;
                decimal total = 0;
                foreach (var venta in ventas)
                {
                    foreach (var detalle in venta.DetallesDeLaVenta)
                    {
                        try
                        {
                            _logger.LogInformation("Procesando detalle de venta ID: {DetalleId}", detalle.Id);

                            worksheet.Cells[row, 1].Value = venta.NumeroDeFactura;
                            worksheet.Cells[row, 2].Value = venta.Cliente != null ?
                                $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}".Trim() :
                                "Cliente no encontrado";
                            worksheet.Cells[row, 3].Value = venta.FechaDeRegistro;
                            // Formato de fecha con AM/PM
                            worksheet.Cells[row, 3].Style.Numberformat.Format = "dd/mm/yyyy hh:mm AM/PM";
                            worksheet.Cells[row, 4].Value = detalle.Producto != null ?
                                $"{detalle.Producto.Codigo ?? "Sin c�digo"} - {detalle.Producto.Nombre ?? "Sin nombre"}" :
                                "Producto no disponible";
                            // Columna para categor�a
                            worksheet.Cells[row, 5].Value = detalle.Producto?.Categoria?.Nombre ?? "Sin categor�a";
                            worksheet.Cells[row, 6].Value = detalle.Cantidad;
                            worksheet.Cells[row, 7].Value = $"Q{detalle.PrecioDeVenta}";
                            worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            decimal subtotal = detalle.Cantidad * detalle.PrecioDeVenta;
                            total += subtotal;
                            worksheet.Cells[row, 8].Value = $"Q{subtotal:F2}";
                            worksheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            worksheet.Cells[row, 9].Value = $"Q{venta.MontoTotal:F2}";
                            worksheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                            row++;
                            _logger.LogInformation("Processed detail for Venta ID {VentaId}", venta.Id);
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue with next detail
                            _logger.LogError(ex, "Error processing detail for Venta ID {VentaId}", venta.Id);
                            System.Diagnostics.Debug.WriteLine($"Error processing detail: {ex.Message}");
                        }
                    }
                }

                // Add Total row
                worksheet.Cells[row + 1, 8].Value = "Total General:";
                worksheet.Cells[row + 1, 8].Style.Font.Bold = true;
                worksheet.Cells[row + 1, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row + 1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row + 1, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                worksheet.Cells[row + 1, 9].Value = $"Q{total:F2}";
                worksheet.Cells[row + 1, 9].Style.Font.Bold = true;
                worksheet.Cells[row + 1, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row + 1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row + 1, 9].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                // Establecer anchos fijos para las columnas:
                worksheet.Column(1).Width = 12; // Factura #
                worksheet.Column(2).Width = 25; // Cliente
                worksheet.Column(3).Width = 20; // Fecha
                worksheet.Column(4).Width = 30; // Producto
                worksheet.Column(5).Width = 15; // Categor�a
                worksheet.Column(6).Width = 10; // Cantidad
                worksheet.Column(7).Width = 12; // Precio
                worksheet.Column(8).Width = 12; // Subtotal
                worksheet.Column(9).Width = 12; // Total

                // Guardar el archivo Excel con explicit error handling
                try
                {
                    _logger.LogInformation("Guardando archivo Excel en: {FilePath}", filePath);
                    FileInfo file = new FileInfo(filePath);
                    package.SaveAs(file);
                    _logger.LogInformation("Excel file created successfully: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving Excel file: {FilePath}", filePath);
                    return StatusCode(500, new
                    {
                        mensaje = $"Error saving Excel file: {ex.Message}",
                        path = filePath,
                        exception = ex.ToString()
                    });
                }
            }

            // Devolver la URL del archivo
            var fileUrl = $"/reportes/{fileName}";

            return Ok(new
            {
                mensaje = "Reporte por periodo generado correctamente",
                url = fileUrl,
                nombreArchivo = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting sales for period {FechaInicio} to {FechaFin}", fechaInicio, fechaFin);
            return StatusCode(500, new
            {
                mensaje = $"Error general al exportar las ventas por periodo: {ex.Message}",
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet("exportar/periodo/pdf")]
    public async Task<IActionResult> ExportarVentasPorPeriodoPDF([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            _logger.LogInformation("Exportando ventas para el periodo (PDF): {FechaInicio} a {FechaFin}", fechaInicio, fechaFin);

            // Asegurar que la fecha fin tenga hora 23:59:59
            fechaFin = fechaFin.Date.AddDays(1).AddTicks(-1);

            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .Where(v => v.FechaDeRegistro >= fechaInicio && v.FechaDeRegistro <= fechaFin)
                .Where(v => v.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .OrderByDescending(v => v.FechaDeRegistro)
                .ToListAsync();

            if (!ventas.Any())
            {
                _logger.LogWarning("No se encontraron ventas para el per�odo especificado");
                return NotFound(new { mensaje = "No se encontraron ventas para el per�odo especificado" });
            }

            _logger.LogInformation("Ventas encontradas: {VentasCount}", ventas.Count);

            // Crear el archivo PDF
            var fileName = $"Ventas_Periodo_{fechaInicio:yyyyMMdd}_a_{fechaFin:yyyyMMdd}_{DateTime.Now:HHmmss}.pdf";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "reportes", fileName);

            // Asegurarse de que el directorio exista
            Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));

            // Create PDF
            var document = Document.Create(container =>
            {
                _logger.LogInformation("Creando documento PDF en: {FilePath}", filePath);
                container.Page(page =>
                {
                    _logger.LogInformation("Configurando p�gina PDF");
                    // Set page settings
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header
                    page.Header().Column(column => {
                        _logger.LogInformation("Configurando encabezado PDF");
                        // Title row
                        column.Item().Row(row => {
                            _logger.LogInformation("Configurando fila de t�tulo PDF");
                            row.RelativeItem().Column(col => {
                                col.Item().AlignCenter().Text("Reporte de Ventas por Periodo").FontSize(20).Bold();

                                // Get time zone for Guatemala
                                TimeZoneInfo guatemalaZone;
                                try
                                {
                                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                                }
                                catch
                                {
                                    try
                                    {
                                        guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                                    }
                                    catch
                                    {
                                        guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                                    }
                                }
                                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
                                col.Item().Text($"Fecha del Reporte: {localDateTime:dd/MM/yyyy hh:mm tt}").FontSize(10);
                            });
                        });

                        _logger.LogInformation("Configurando informaci�n del periodo en el encabezado PDF");
                        // Period info section with border
                        column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                        column.Item().PaddingTop(10).Text("Informaci�n del Periodo").Bold();
                        column.Item().Table(table => {
                            _logger.LogInformation("Definiendo columnas para la tabla de informaci�n del periodo PDF");
                            // Define columns
                            table.ColumnsDefinition(columns => {
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                            });

                            _logger.LogInformation("Agregando filas a la tabla de informaci�n del periodo PDF");
                            // Add rows
                            table.Cell().Text("Fecha Inicio:").Bold();
                            table.Cell().Text($"{fechaInicio:dd/MM/yyyy}");

                            table.Cell().Text("Fecha Fin:").Bold();
                            table.Cell().Text($"{fechaFin:dd/MM/yyyy}");

                            table.Cell().Text("Total Ventas:").Bold();
                            table.Cell().Text($"{ventas.Count}");

                            table.Cell().Text("Monto Total:").Bold();
                            table.Cell().Text($"Q {ventas.Sum(v => v.MontoTotal):F2}");
                        });
                        _logger.LogInformation("Configurando separaci�n entre encabezado y contenido PDF");
                        column.Item().PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                    });

                    _logger.LogInformation("Configurando contenido de la p�gina PDF");
                    // Content
                    page.Content().Column(column => {
                        // Sales table header as an item in the column
                        column.Item().PaddingTop(10).Text("Detalle de Ventas").FontSize(14).Bold();

                        // Table as another item in the column
                        column.Item().PaddingTop(10).Table(table => {
                            // Define columns
                            table.ColumnsDefinition(columns => {
                                columns.ConstantColumn(50);     // Factura # - reducido
                                columns.RelativeColumn(2);      // Cliente - relativo
                                columns.RelativeColumn(2);      // Fecha - relativo
                                columns.RelativeColumn(3);      // Producto - relativo
                                columns.RelativeColumn(2);      // Categor�a - relativo 
                                columns.ConstantColumn(50);     // Cantidad - reducido
                                columns.ConstantColumn(50);     // Precio - reducido
                                columns.ConstantColumn(50);     // Subtotal - reducido
                                columns.ConstantColumn(50);     // Total - reducido
                            });

                            // Add header
                            table.Header(header => {
                                _logger.LogInformation("Agregando encabezado a la tabla de ventas PDF");
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Factura #").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cliente").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Fecha").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Producto").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Categor�a").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cantidad").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Precio").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Subtotal").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Total").Bold();
                            });

                            // Add rows
                            decimal totalGeneral = 0;

                            foreach (var venta in ventas)
                            {
                                foreach (var detalle in venta.DetallesDeLaVenta)
                                {
                                    _logger.LogInformation("Agregando fila a la tabla de ventas PDF para detalle ID: {DetalleId}", detalle.Id);

                                    string clienteNombre = venta.Cliente != null
                                        ? $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}"
                                        : "Cliente no encontrado";

                                    string categoria = detalle.Producto?.Categoria?.Nombre ?? "Sin categor�a";
                                    string fechaFormateada = venta.FechaDeRegistro.ToString("dd/MM/yyyy hh:mm tt");
                                    decimal subtotal = detalle.Cantidad * detalle.PrecioDeVenta;
                                    totalGeneral += subtotal;

                                    table.Cell().Text(venta.NumeroDeFactura);
                                    table.Cell().Text(clienteNombre);
                                    table.Cell().Text(fechaFormateada);

                                    // Producto con imagen
                                    table.Cell().Element(cell => {
                                        cell.Row(row => {
                                            // Agregar imagen del producto si est� disponible
                                            if (detalle.Producto != null && !string.IsNullOrEmpty(detalle.Producto.ImagenUrl))
                                            {
                                                string fullPath = Path.Combine(_hostingEnvironment.WebRootPath, detalle.Producto.ImagenUrl.TrimStart('/'));
                                                if (System.IO.File.Exists(fullPath))
                                                {
                                                    try
                                                    {
                                                        row.ConstantItem(30).Image(fullPath)
                                                            .FitArea();
                                                    }
                                                    catch
                                                    {
                                                        _logger.LogWarning("No se pudo cargar la imagen: {ImagePath}", fullPath);
                                                        // Continuar sin imagen
                                                    }
                                                }
                                            }

                                            // Mostrar nombre del producto
                                            string productoNombre = detalle.Producto != null
                                                ? $"{detalle.Producto.Codigo} - {detalle.Producto.Nombre}"
                                                : "Producto no disponible";

                                            row.RelativeItem().Text(productoNombre)
                                                .WrapAnywhere();
                                        });
                                    });
                                    table.Cell().AlignCenter().Text(categoria);
                                    table.Cell().Text(detalle.Cantidad.ToString());
                                    table.Cell().Text($"Q {detalle.PrecioDeVenta:F2}");
                                    table.Cell().Text($"Q {subtotal:F2}");
                                    table.Cell().Text($"Q {venta.MontoTotal:F2}");
                                }
                            }

                            _logger.LogInformation("Agregando fila total a la tabla de ventas PDF");
                            // Add total row
                            table.Cell().ColumnSpan(8).AlignRight().Text(text => text.Span("Total General:").Bold());
                            table.Cell().Text($"Q {totalGeneral:F2}").Bold();
                        });
                    });

                    _logger.LogInformation("Configurando pie de p�gina PDF");
                    // Footer
                    page.Footer().AlignCenter().Text(text => {
                        text.Span("P�gina ").FontSize(10);
                        text.CurrentPageNumber().FontSize(10);
                        text.Span(" de ").FontSize(10);
                        text.TotalPages().FontSize(10);
                    });
                });
            });

            _logger.LogInformation("Generando PDF en: {FilePath}", filePath);
            // Save the PDF
            document.GeneratePdf(filePath);

            // Return the file URL
            var fileUrl = $"/reportes/{fileName}";

            return Ok(new
            {
                mensaje = "Reporte PDF por periodo generado correctamente",
                url = fileUrl,
                nombreArchivo = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte PDF para periodo {FechaInicio} a {FechaFin}", fechaInicio, fechaFin);
            return StatusCode(500, new
            {
                mensaje = $"Error al exportar las ventas por periodo en PDF: {ex.Message}",
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet("exportar/cliente/{clienteId}")]
    public async Task<IActionResult> ExportarVentasPorCliente(int clienteId)
    {
        try
        {
            _logger.LogInformation("Exportando ventas para el cliente con ID: {ClienteId}", clienteId);
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente con ID {ClienteId} no encontrado", clienteId);
                return NotFound(new { mensaje = "Cliente no encontrado" });
            }

            _logger.LogInformation("Cliente encontrado: {ClienteNombre} {ClienteApellido}", cliente.Nombre, cliente.Apellido);
            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria) // Incluir la categor�a
                .Where(v => v.IdCliente == clienteId)
                .Where(v => v.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .OrderByDescending(v => v.FechaDeRegistro)
                .ToListAsync();

            if (!ventas.Any())
            {
                _logger.LogWarning("No se encontraron ventas para el cliente con ID {ClienteId}", clienteId);
                return NotFound(new { mensaje = "No se encontraron ventas para el cliente especificado" });
            }

            _logger.LogInformation("Ventas encontradas: {VentasCount}", ventas.Count);

            // Crear el archivo Excel
            var fileName = $"Ventas_Cliente_{cliente.Nombre.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "reportes", fileName);

            // Create directory with explicit error handling
            try
            {
                _logger.LogInformation("Creando directorio para reportes en: {ReportesPath}", Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));
                Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));
                _logger.LogInformation("Directorio creado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando directorio para reportes: {ReportesPath}", Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));
                return StatusCode(500, new
                {
                    mensaje = $"Error creating report directory: {ex.Message}",
                    path = Path.Combine(_hostingEnvironment.WebRootPath, "reportes"),
                    exception = ex.ToString()
                });
            }

            using (var package = new ExcelPackage())
            {
                _logger.LogInformation("Creando archivo Excel en: {FilePath}", filePath);
                var worksheet = package.Workbook.Worksheets.Add("Ventas");

                // T�tulo principal del reporte
                worksheet.Cells[3, 1].Value = "Reporte de Ventas por Cliente";
                worksheet.Cells[3, 1, 3, 9].Merge = true;
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Style.Font.Size = 16;
                worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[3, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(180, 180, 180));

                // Manejo de zona horaria para Guatemala
                TimeZoneInfo guatemalaZone;
                try
                {
                    // Intentar formato Windows
                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                }
                catch
                {
                    try
                    {
                        // Intentar formato Linux
                        guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                    }
                    catch
                    {
                        // Crear zona manualmente con UTC-6 para Guatemala
                        guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                    }
                }

                // Fecha y hora del reporte
                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
                worksheet.Cells[4, 1].Value = $"Fecha del Reporte: {localDateTime:dd/MM/yyyy hh:mm tt}";
                worksheet.Cells[4, 1, 4, 9].Merge = true;
                worksheet.Cells[4, 1].Style.Font.Italic = true;
                worksheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[4, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 220, 220));

                // Informaci�n del cliente
                worksheet.Cells[6, 1].Value = "Informaci�n del Cliente";
                worksheet.Cells[6, 1, 6, 6].Merge = true;
                worksheet.Cells[6, 1].Style.Font.Bold = true;
                worksheet.Cells[6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[6, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[6, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                worksheet.Cells[7, 1].Value = "Nombre";
                worksheet.Cells[7, 2].Value = $"{cliente.Nombre} {cliente.Apellido}".Trim();
                worksheet.Cells[8, 1].Value = "Email";
                worksheet.Cells[8, 2].Value = cliente.Email ?? "No disponible";
                worksheet.Cells[9, 1].Value = "Tel�fono";
                worksheet.Cells[9, 2].Value = cliente.Telefono ?? "No disponible";
                worksheet.Cells[10, 1].Value = "Direcci�n";
                worksheet.Cells[10, 2].Value = cliente.Direccion ?? "No disponible";

                // Estilo para informaci�n del cliente
                using (var range = worksheet.Cells[7, 1, 10, 1])
                {
                    _logger.LogInformation("Estableciendo estilos para informaci�n del cliente");
                    range.Style.Font.Bold = true;
                }

                _logger.LogInformation("Estableciendo encabezados de ventas en la hoja de Excel");
                // Encabezados de ventas
                worksheet.Cells[12, 1].Value = "Factura #";
                worksheet.Cells[12, 2].Value = "Fecha";
                worksheet.Cells[12, 3].Value = "Producto";
                worksheet.Cells[12, 4].Value = "Categor�a";
                worksheet.Cells[12, 5].Value = "Cantidad";
                worksheet.Cells[12, 6].Value = "Precio";
                worksheet.Cells[12, 7].Value = "Subtotal";
                worksheet.Cells[12, 8].Value = "Pago";
                worksheet.Cells[12, 9].Value = "Cambio";

                // Estilo para encabezados
                using (var range = worksheet.Cells[12, 1, 12, 9])
                {
                    _logger.LogInformation("Estableciendo estilos para encabezados de ventas");
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                int row = 13;
                foreach (var venta in ventas)
                {
                    foreach (var detalle in venta.DetallesDeLaVenta)
                    {
                        try
                        {
                            _logger.LogInformation("Procesando detalle de venta ID: {DetalleId}", detalle.Id);

                            worksheet.Cells[row, 1].Value = venta.NumeroDeFactura;
                            worksheet.Cells[row, 2].Value = venta.FechaDeRegistro;
                            // Formato de fecha con AM/PM
                            worksheet.Cells[row, 2].Style.Numberformat.Format = "dd/mm/yyyy hh:mm AM/PM";
                            worksheet.Cells[row, 3].Value = detalle.Producto != null ?
                                $"{detalle.Producto.Codigo ?? "Sin c�digo"} - {detalle.Producto.Nombre ?? "Sin nombre"}" :
                                "Producto no disponible";
                            // Columna para categor�a
                            worksheet.Cells[row, 4].Value = detalle.Producto?.Categoria?.Nombre ?? "Sin categor�a";
                            worksheet.Cells[row, 5].Value = detalle.Cantidad;
                            worksheet.Cells[row, 6].Value = detalle.PrecioDeVenta;
                            worksheet.Cells[row, 7].Value = $"Q{detalle.Cantidad * detalle.PrecioDeVenta}";
                            worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            // Columnas para pago y cambio
                            worksheet.Cells[row, 8].Value = $"Q{venta.MontoDePago}";
                            worksheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            worksheet.Cells[row, 9].Value = $"Q{venta.MontoDeCambio}";
                            worksheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                            row++;
                            _logger.LogInformation("Processed detail for Venta ID {VentaId}", venta.Id);
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue with next detail
                            _logger.LogError(ex, "Error processing detail for Venta ID {VentaId}", venta.Id);
                            System.Diagnostics.Debug.WriteLine($"Error processing detail: {ex.Message}");
                        }
                    }
                }

                // Calculate total
                decimal total = ventas.Sum(v => v.MontoTotal);

                // Add Total row
                worksheet.Cells[row + 1, 8].Value = "Total General:";
                worksheet.Cells[row + 1, 8].Style.Font.Bold = true;
                worksheet.Cells[row + 1, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row + 1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row + 1, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                worksheet.Cells[row + 1, 9].Value = $"Q{total}";
                worksheet.Cells[row + 1, 9].Style.Font.Bold = true;
                worksheet.Cells[row + 1, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row + 1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row + 1, 9].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                // Establecer anchos fijos para las columnas:
                worksheet.Column(1).Width = 12; // Factura #
                worksheet.Column(2).Width = 20; // Fecha
                worksheet.Column(3).Width = 30; // Producto
                worksheet.Column(4).Width = 15; // Categor�a
                worksheet.Column(5).Width = 10; // Cantidad
                worksheet.Column(6).Width = 12; // Precio
                worksheet.Column(7).Width = 12; // Subtotal
                worksheet.Column(8).Width = 12; // Pago
                worksheet.Column(9).Width = 12; // Cambio

                // Guardar el archivo Excel con explicit error handling
                try
                {
                    _logger.LogInformation("Guardando archivo Excel en: {FilePath}", filePath);
                    FileInfo file = new FileInfo(filePath);
                    package.SaveAs(file);
                    _logger.LogInformation("Excel file created successfully: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving Excel file: {FilePath}", filePath);
                    return StatusCode(500, new
                    {
                        mensaje = $"Error saving Excel file: {ex.Message}",
                        path = filePath,
                        exception = ex.ToString()
                    });
                }
            }

            // Devolver la URL del archivo
            var fileUrl = $"/reportes/{fileName}";

            return Ok(new
            {
                mensaje = "Reporte por cliente generado correctamente",
                url = fileUrl,
                nombreArchivo = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting sales for client ID {ClienteId}", clienteId);
            return StatusCode(500, new
            {
                mensaje = $"Error general al exportar las ventas por cliente: {ex.Message}",
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet("exportar/cliente/{clienteId}/pdf")]
    public async Task<IActionResult> ExportarVentasPorClientePDF(int clienteId)
    {
        try
        {
            _logger.LogInformation("Exportando ventas para el cliente con ID (PDF): {ClienteId}", clienteId);
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                _logger.LogWarning("Cliente con ID {ClienteId} no encontrado", clienteId);
                return NotFound(new { mensaje = "Cliente no encontrado" });
            }

            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .Where(v => v.IdCliente == clienteId)
                .Where(v => v.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .OrderByDescending(v => v.FechaDeRegistro)
                .ToListAsync();

            if (!ventas.Any())
            {
                _logger.LogWarning("No se encontraron ventas para el cliente con ID {ClienteId}", clienteId);
                return NotFound(new { mensaje = "No se encontraron ventas para el cliente especificado" });
            }

            _logger.LogInformation("Ventas encontradas: {VentasCount}", ventas.Count);

            // Crear el archivo PDF
            var fileName = $"Ventas_Cliente_{cliente.Nombre.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "reportes", fileName);

            // Asegurarse de que el directorio exista
            Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));

            // Generate PDF document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    // Set page settings
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header - Fixed by using Column instead of multiple Element calls
                    page.Header().Column(column => {
                        // Title row
                        column.Item().Row(row => {
                            row.RelativeItem().Column(col => {
                                col.Item().AlignCenter().Text("Reporte de Ventas por Cliente").FontSize(20).Bold();

                                TimeZoneInfo guatemalaZone;
                                try
                                {
                                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                                }
                                catch
                                {
                                    try
                                    {
                                        guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                                    }
                                    catch
                                    {
                                        guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                                    }
                                }
                                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
                                col.Item().Text($"Fecha del Reporte: {localDateTime:dd/MM/yyyy hh:mm tt}").FontSize(10);
                            });
                        });

                        // Client info section with border
                        column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                        column.Item().PaddingTop(10).Text("Informaci�n del Cliente").Bold();
                        column.Item().Table(table => {
                            // Define columns
                            table.ColumnsDefinition(columns => {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            // Add rows
                            table.Cell().Text("Nombre:").Bold();
                            table.Cell().Text($"{cliente.Nombre} {cliente.Apellido}".Trim());

                            table.Cell().Text("Email:").Bold();
                            table.Cell().Text(cliente.Email ?? "No disponible");

                            table.Cell().Text("Tel�fono:").Bold();
                            table.Cell().Text(cliente.Telefono ?? "No disponible");

                            table.Cell().Text("Direcci�n:").Bold();
                            table.Cell().Text(cliente.Direccion ?? "No disponible");
                        });
                        column.Item().PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                    });

                    // Content
                    page.Content().Column(column => {
                        // Sales table header
                        column.Item().PaddingTop(10).Text("Detalle de Ventas").FontSize(14).Bold();

                        column.Item().PaddingTop(10).Table(table => {
                            // Define columns
                            table.ColumnsDefinition(columns => {
                                columns.ConstantColumn(50);     // Factura # - reducido
                                columns.RelativeColumn(2);      // Fecha - relativo
                                columns.RelativeColumn(4);      // Producto - relativo
                                columns.RelativeColumn(3);      // Categor�a - relativo 
                                columns.ConstantColumn(50);     // Cantidad - reducido
                                columns.ConstantColumn(50);     // Subtotal - reducido
                                columns.ConstantColumn(50);     // Pago - reducido
                                columns.ConstantColumn(50);     // Cambio - reducido
                            });

                            // Add header
                            table.Header(header => {
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Factura #").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Fecha").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Producto").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Categor�a").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cantidad").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Subtotal").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Pago").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cambio").Bold();
                            });

                            // Add rows for each sale
                            foreach (var venta in ventas)
                            {
                                // Sale header
                                string fechaFormateada = venta.FechaDeRegistro.ToString("dd/MM/yyyy hh:mm tt");

                                // Detalles de cada venta
                                foreach (var detalle in venta.DetallesDeLaVenta)
                                {
                                    string productoNombre = detalle.Producto != null
                                        ? $"{detalle.Producto.Codigo} - {detalle.Producto.Nombre}"
                                        : "Producto no disponible";

                                    string categoria = detalle.Producto?.Categoria?.Nombre ?? "Sin categor�a";
                                    decimal subtotal = detalle.Cantidad * detalle.PrecioDeVenta;

                                    table.Cell().Text(venta.NumeroDeFactura);
                                    table.Cell().Text(fechaFormateada);

                                    // Product with image
                                    table.Cell().Element(cell => {
                                        cell.Row(row => {
                                            string imagePath = detalle.Producto?.ImagenUrl ?? "/images/productos/default.png";
                                            if (!string.IsNullOrEmpty(imagePath))
                                            {
                                                string fullPath = Path.Combine(_hostingEnvironment.WebRootPath, imagePath.TrimStart('/'));
                                                if (System.IO.File.Exists(fullPath))
                                                {
                                                    try
                                                    {
                                                        row.ConstantItem(30).Image(fullPath)
                                                            .FitArea();
                                                    }
                                                    catch
                                                    {
                                                        // If image loading fails, just show the product name
                                                    }
                                                }
                                            }
                                            row.RelativeItem().Text(productoNombre)
                                                .WrapAnywhere();
                                        });
                                    });

                                    table.Cell().Text(categoria);
                                    table.Cell().Text(detalle.Cantidad.ToString());
                                    table.Cell().Text($"Q {subtotal:F2}");
                                    table.Cell().Text($"Q {venta.MontoDePago:F2}");
                                    table.Cell().Text($"Q {venta.MontoDeCambio:F2}");
                                }
                            }

                            // Add total row
                            decimal total = ventas.Sum(v => v.MontoTotal);
                            table.Cell().ColumnSpan(7).AlignRight().Text(text => text.Span("Total General:").Bold());
                            table.Cell().Text($"Q {total:F2}").Bold();
                            table.Cell().ColumnSpan(2);
                        });
                    });

                    // Footer
                    page.Footer().AlignCenter().Text(text => {
                        text.Span("P�gina ").FontSize(10);
                        text.CurrentPageNumber().FontSize(10);
                        text.Span(" de ").FontSize(10);
                        text.TotalPages().FontSize(10);
                    });
                });
            });

            document.GeneratePdf(filePath);

            // Devolver la URL del archivo
            var fileUrl = $"/reportes/{fileName}";

            return Ok(new
            {
                mensaje = "Reporte PDF por cliente generado correctamente",
                url = fileUrl,
                nombreArchivo = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte PDF para cliente ID {ClienteId}", clienteId);
            return StatusCode(500, new
            {
                mensaje = $"Error al exportar las ventas por cliente en PDF: {ex.Message}",
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet("exportar/producto/{productoId}")]
    public async Task<IActionResult> ExportarVentasPorProducto(int productoId)
    {
        try
        {
            _logger.LogInformation("Exportando ventas para el producto con ID: {ProductoId}", productoId);
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null)
            {
                _logger.LogWarning("Producto con ID {ProductoId} no encontrado", productoId);
                return NotFound(new { mensaje = "Producto no encontrado" });
            }

            _logger.LogInformation("Producto encontrado: {ProductoCodigo} - {ProductoNombre}", producto.Codigo, producto.Nombre);

            // Get all sales IDs that have details with this product
            var ventasIds = await _context.DetallesDeLaVenta
                .Where(d => d.IdProducto == productoId)
                .Where(d => d.Venta.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .Select(d => d.IdVenta)
                .Distinct()
                .ToListAsync();

            if (!ventasIds.Any())
            {
                _logger.LogWarning("No se encontraron ventas para el producto con ID {ProductoId}", productoId);
                return NotFound(new { mensaje = "No se encontraron ventas para el producto especificado" });
            }

            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                .Where(v => ventasIds.Contains(v.Id))
                .OrderByDescending(v => v.FechaDeRegistro)
                .ToListAsync();

            _logger.LogInformation("Ventas encontradas: {VentasCount}", ventas.Count);

            // Crear el archivo Excel
            var fileName = $"Ventas_Producto_{producto.Codigo.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "reportes", fileName);

            // Asegurarse de que el directorio exista
            try
            {
                _logger.LogInformation("Creando directorio para reportes en: {ReportesPath}", Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));
                Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));
                _logger.LogInformation("Directorio creado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando directorio para reportes: {ReportesPath}", Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));
                return StatusCode(500, new
                {
                    mensaje = $"Error creating report directory: {ex.Message}",
                    path = Path.Combine(_hostingEnvironment.WebRootPath, "reportes"),
                    exception = ex.ToString()
                });
            }

            using (var package = new ExcelPackage())
            {
                _logger.LogInformation("Creando archivo Excel en: {FilePath}", filePath);
                var worksheet = package.Workbook.Worksheets.Add("Ventas");

                // T�tulo principal del reporte
                worksheet.Cells[3, 1].Value = "Reporte de Ventas por Producto";
                worksheet.Cells[3, 1, 3, 9].Merge = true;
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Style.Font.Size = 16;
                worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[3, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(180, 180, 180));

                // Manejo de zona horaria para Guatemala
                TimeZoneInfo guatemalaZone;
                try
                {
                    // Intentar formato Windows
                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                }
                catch
                {
                    try
                    {
                        // Intentar formato Linux
                        guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                    }
                    catch
                    {
                        // Crear zona manualmente con UTC-6 para Guatemala
                        guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                    }
                }

                // Fecha y hora del reporte
                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
                worksheet.Cells[4, 1].Value = $"Fecha del Reporte: {localDateTime:dd/MM/yyyy hh:mm tt}";
                worksheet.Cells[4, 1, 4, 9].Merge = true;
                worksheet.Cells[4, 1].Style.Font.Italic = true;
                worksheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[4, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 220, 220));

                // Informaci�n del producto
                worksheet.Cells[6, 1].Value = "Informaci�n del Producto";
                worksheet.Cells[6, 1, 6, 6].Merge = true;
                worksheet.Cells[6, 1].Style.Font.Bold = true;
                worksheet.Cells[6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[6, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[6, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                worksheet.Cells[7, 1].Value = "C�digo";
                worksheet.Cells[7, 2].Value = producto.Codigo;
                worksheet.Cells[8, 1].Value = "Nombre";
                worksheet.Cells[8, 2].Value = producto.Nombre;
                worksheet.Cells[9, 1].Value = "Categor�a";
                worksheet.Cells[9, 2].Value = producto.Categoria?.Nombre ?? "Sin categor�a";
                worksheet.Cells[10, 1].Value = "Precio";
                worksheet.Cells[10, 2].Value = $"Q {producto.PrecioDeVenta:F2}";

                // Estilo para informaci�n del producto
                using (var range = worksheet.Cells[7, 1, 10, 1])
                {
                    _logger.LogInformation("Estableciendo estilos para informaci�n del producto");
                    range.Style.Font.Bold = true;
                }

                _logger.LogInformation("Estableciendo encabezados de ventas en la hoja de Excel");
                // Encabezados de ventas
                worksheet.Cells[12, 1].Value = "Factura #";
                worksheet.Cells[12, 2].Value = "Cliente";
                worksheet.Cells[12, 3].Value = "Fecha";
                worksheet.Cells[12, 4].Value = "Cantidad";
                worksheet.Cells[12, 5].Value = "Precio";
                worksheet.Cells[12, 6].Value = "Subtotal";
                worksheet.Cells[12, 7].Value = "Pago";
                worksheet.Cells[12, 8].Value = "Cambio";

                // Estilo para encabezados
                using (var range = worksheet.Cells[12, 1, 12, 8])
                {
                    _logger.LogInformation("Estableciendo estilos para encabezados de ventas");
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                int row = 13;
                decimal total = 0;
                foreach (var venta in ventas)
                {
                    var detalle = venta.DetallesDeLaVenta.FirstOrDefault(d => d.IdProducto == productoId);
                    if (detalle != null)
                    {
                        try
                        {
                            _logger.LogInformation("Procesando detalle de venta ID: {DetalleId} para factura: {Factura}", detalle.Id, venta.NumeroDeFactura);

                            decimal subtotal = detalle.Cantidad * detalle.PrecioDeVenta;
                            total += subtotal;

                            worksheet.Cells[row, 1].Value = venta.NumeroDeFactura;
                            worksheet.Cells[row, 2].Value = venta.Cliente != null ?
                                $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}".Trim() :
                                "Cliente no encontrado";
                            worksheet.Cells[row, 3].Value = venta.FechaDeRegistro;
                            // Formato de fecha con AM/PM
                            worksheet.Cells[row, 3].Style.Numberformat.Format = "dd/mm/yyyy hh:mm AM/PM";
                            worksheet.Cells[row, 4].Value = detalle.Cantidad;
                            worksheet.Cells[row, 5].Value = detalle.PrecioDeVenta;
                            worksheet.Cells[row, 6].Value = $"Q{subtotal:F2}";
                            worksheet.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            // Columnas para pago y cambio
                            worksheet.Cells[row, 7].Value = $"Q{venta.MontoDePago:F2}";
                            worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                            worksheet.Cells[row, 8].Value = $"Q{venta.MontoDeCambio:F2}";
                            worksheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                            row++;
                            _logger.LogInformation("Processed detail for Venta ID {VentaId}", venta.Id);
                        }
                        catch (Exception ex)
                        {
                            // Log error but continue with next detail
                            _logger.LogError(ex, "Error processing detail for Venta ID {VentaId}", venta.Id);
                            System.Diagnostics.Debug.WriteLine($"Error processing detail: {ex.Message}");
                        }
                    }
                }

                // Add Total row
                worksheet.Cells[row + 1, 7].Value = "Total General:";
                worksheet.Cells[row + 1, 7].Style.Font.Bold = true;
                worksheet.Cells[row + 1, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row + 1, 7].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row + 1, 7].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                worksheet.Cells[row + 1, 8].Value = $"Q{total:F2}";
                worksheet.Cells[row + 1, 8].Style.Font.Bold = true;
                worksheet.Cells[row + 1, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row + 1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row + 1, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                // Establecer anchos fijos para las columnas:
                worksheet.Column(1).Width = 12; // Factura #
                worksheet.Column(2).Width = 25; // Cliente
                worksheet.Column(3).Width = 20; // Fecha
                worksheet.Column(4).Width = 10; // Cantidad
                worksheet.Column(5).Width = 12; // Precio
                worksheet.Column(6).Width = 12; // Subtotal
                worksheet.Column(7).Width = 12; // Pago
                worksheet.Column(8).Width = 12; // Cambio

                // Guardar el archivo Excel con explicit error handling
                try
                {
                    _logger.LogInformation("Guardando archivo Excel en: {FilePath}", filePath);
                    FileInfo file = new FileInfo(filePath);
                    package.SaveAs(file);
                    _logger.LogInformation("Excel file created successfully: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving Excel file: {FilePath}", filePath);
                    return StatusCode(500, new
                    {
                        mensaje = $"Error saving Excel file: {ex.Message}",
                        path = filePath,
                        exception = ex.ToString()
                    });
                }
            }

            // Devolver la URL del archivo
            var fileUrl = $"/reportes/{fileName}";

            return Ok(new
            {
                mensaje = "Reporte por producto generado correctamente",
                url = fileUrl,
                nombreArchivo = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting sales for product ID {ProductoId}", productoId);
            return StatusCode(500, new
            {
                mensaje = $"Error general al exportar las ventas por producto: {ex.Message}",
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet("exportar/producto/{productoId}/pdf")]
    public async Task<IActionResult> ExportarVentasPorProductoPDF(int productoId)
    {
        try
        {
            _logger.LogInformation("Exportando ventas para el producto con ID (PDF): {ProductoId}", productoId);
            var producto = await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null)
            {
                _logger.LogWarning("Producto con ID {ProductoId} no encontrado", productoId);
                return NotFound(new { mensaje = "Producto no encontrado" });
            }

            // Get all sales IDs that have details with this product
            var ventasIds = await _context.DetallesDeLaVenta
                .Where(d => d.IdProducto == productoId)
                .Where(d => d.Venta.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .Select(d => d.IdVenta)
                .Distinct()
                .ToListAsync();

            if (!ventasIds.Any())
            {
                _logger.LogWarning("No se encontraron ventas para el producto con ID {ProductoId}", productoId);
                return NotFound(new { mensaje = "No se encontraron ventas para el producto especificado" });
            }

            var ventas = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                .Where(v => ventasIds.Contains(v.Id))
                .OrderByDescending(v => v.FechaDeRegistro)
                .ToListAsync();

            _logger.LogInformation("Ventas encontradas: {VentasCount}", ventas.Count);
            _logger.LogInformation("Creando archivo PDF para el producto: {ProductoNombre}", producto.Nombre);

            // Create PDF
            var fileName = $"Ventas_Producto_{producto.Codigo.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "reportes", fileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));

            // Generate PDF document
            var document = Document.Create(container =>
            {
                _logger.LogInformation("Creando documento PDF en: {FilePath}", filePath);
                container.Page(page =>
                {
                    _logger.LogInformation("Configurando p�gina PDF");
                    // Set page settings
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    // Header - Modified to remove image
                    page.Header().Column(column => {
                        _logger.LogInformation("Configurando encabezado PDF");
                        // Title row - without product image
                        column.Item().Row(row => {
                            _logger.LogInformation("Configurando fila de t�tulo PDF");
                            row.RelativeItem().Column(col => {
                                col.Item().AlignCenter().Text("Reporte de Ventas por Producto").FontSize(20).Bold();

                                // Get time zone for Guatemala
                                TimeZoneInfo guatemalaZone;
                                try
                                {
                                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                                }
                                catch
                                {
                                    try
                                    {
                                        guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                                    }
                                    catch
                                    {
                                        guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                                    }
                                }
                                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
                                col.Item().Text($"Fecha del Reporte: {localDateTime:dd/MM/yyyy hh:mm tt}").FontSize(10);
                            });
                        });

                        _logger.LogInformation("Configurando informaci�n del producto en el encabezado PDF");
                        // Product info section with border
                        column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                        column.Item().PaddingTop(10).Text("Informaci�n del Producto").Bold();

                        // Add a row for product image and info
                        column.Item().Row(row => {
                            // Product image on the left
                            if (!string.IsNullOrEmpty(producto.ImagenUrl))
                            {
                                string fullPath = Path.Combine(_hostingEnvironment.WebRootPath, producto.ImagenUrl.TrimStart('/'));
                                if (System.IO.File.Exists(fullPath))
                                {
                                    try
                                    {
                                        row.ConstantItem(100).Padding(5).Image(fullPath)
                                            .FitArea();
                                    }
                                    catch
                                    {
                                        _logger.LogWarning("Error loading product image from path: {ImagePath}", fullPath);
                                    }
                                }
                            }

                            // Product details table on the right
                            row.RelativeItem().Padding(5).Table(table => {
                                // Define columns
                                table.ColumnsDefinition(columns => {
                                    columns.RelativeColumn();
                                    columns.RelativeColumn(2);
                                });

                                // Add rows
                                table.Cell().Text("C�digo:").Bold();
                                table.Cell().Text(producto.Codigo);

                                table.Cell().Text("Nombre:").Bold();
                                table.Cell().Text(producto.Nombre);

                                table.Cell().Text("Categor�a:").Bold();
                                table.Cell().Text(producto.Categoria?.Nombre ?? "Sin categor�a");

                                table.Cell().Text("Precio:").Bold();
                                table.Cell().Text($"Q {producto.PrecioDeVenta:F2}");
                            });
                        });

                        column.Item().PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                    });

                    _logger.LogInformation("Configurando contenido de la p�gina PDF");
                    // Content
                    page.Content().Column(column => {
                        // Sales table header as an item in the column
                        column.Item().PaddingTop(10).Text("Detalle de Ventas").FontSize(14).Bold();

                        // Table as another item in the column
                        column.Item().PaddingTop(10).Table(table => {
                            // Define columns
                            table.ColumnsDefinition(columns => {
                                columns.ConstantColumn(60);     // Factura # - reducido
                                columns.RelativeColumn(3);      // Cliente - relativo
                                columns.RelativeColumn(2);      // Fecha - relativo
                                columns.ConstantColumn(60);     // Cantidad - reducido
                                columns.ConstantColumn(60);     // Precio - reducido
                                columns.ConstantColumn(60);     // Subtotal - reducido
                                columns.ConstantColumn(60);     // Pago - reducido
                                columns.ConstantColumn(60);     // Cambio - reducido
                            });

                            // Add header
                            table.Header(header => {
                                _logger.LogInformation("Agregando encabezado a la tabla de ventas PDF");
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Factura #").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cliente").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Fecha").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cantidad").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Precio").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Subtotal").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Pago").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cambio").Bold();
                            });

                            // Add rows
                            decimal total = 0;

                            foreach (var venta in ventas)
                            {
                                var detalle = venta.DetallesDeLaVenta.FirstOrDefault(d => d.IdProducto == productoId);
                                if (detalle != null)
                                {
                                    _logger.LogInformation("Agregando fila a la tabla de ventas PDF para detalle ID: {DetalleId}", detalle.Id);

                                    string clienteNombre = venta.Cliente != null
                                        ? $"{venta.Cliente.Nombre} {venta.Cliente.Apellido}"
                                        : "Cliente no encontrado";

                                    string fechaFormateada = venta.FechaDeRegistro.ToString("dd/MM/yyyy hh:mm tt");
                                    decimal subtotal = detalle.Cantidad * detalle.PrecioDeVenta;
                                    total += subtotal;

                                    table.Cell().Text(venta.NumeroDeFactura);
                                    table.Cell().Text(clienteNombre);
                                    table.Cell().Text(fechaFormateada);
                                    table.Cell().Text(detalle.Cantidad.ToString());
                                    table.Cell().Text($"Q {detalle.PrecioDeVenta:F2}");
                                    table.Cell().Text($"Q {subtotal:F2}");
                                    table.Cell().Text($"Q {venta.MontoDePago:F2}");
                                    table.Cell().Text($"Q {venta.MontoDeCambio:F2}");
                                }
                            }

                            _logger.LogInformation("Agregando fila total a la tabla de ventas PDF");
                            // Add total row
                            table.Cell().ColumnSpan(7).AlignRight().Text(text => text.Span("Total General:").Bold());
                            table.Cell().Text($"Q {total:F2}").Bold();
                            table.Cell().ColumnSpan(2);
                        });
                    });

                    _logger.LogInformation("Configurando pie de p�gina PDF");
                    // Footer
                    page.Footer().AlignCenter().Text(text => {
                        text.Span("P�gina ").FontSize(10);
                        text.CurrentPageNumber().FontSize(10);
                        text.Span(" de ").FontSize(10);
                        text.TotalPages().FontSize(10);
                    });
                });
            });

            _logger.LogInformation("Generando PDF en: {FilePath}", filePath);
            // Save the PDF
            document.GeneratePdf(filePath);

            // Return the file URL
            var fileUrl = $"/reportes/{fileName}";

            return Ok(new
            {
                mensaje = "Reporte PDF por producto generado correctamente",
                url = fileUrl,
                nombreArchivo = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte PDF para producto ID {ProductoId}", productoId);
            return StatusCode(500, new
            {
                mensaje = $"Error al exportar las ventas por producto en PDF: {ex.Message}",
                stackTrace = ex.StackTrace
            });
        }
    }

    // Agregar este m�todo al final del controlador
    [HttpGet("exportar/proveedor/{proveedorId}")]
    public async Task<IActionResult> ExportarVentasPorProveedor(int proveedorId)
    {
        try
        {
            _logger.LogInformation("Exportando ventas para el proveedor con ID: {ProveedorId}", proveedorId);
            var proveedor = await _context.Proveedores.FindAsync(proveedorId);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor con ID {ProveedorId} no encontrado", proveedorId);
                return NotFound(new { mensaje = "Proveedor no encontrado" });
            }

            _logger.LogInformation("Proveedor encontrado: {ProveedorNombre}", proveedor.Nombre);
            var detalles = await _context.DetallesDeLaVenta
                .Include(d => d.Venta)
                    .ThenInclude(v => v.Cliente)
                .Include(d => d.Venta)
                    .ThenInclude(v => v.EstadoDeLaVenta)
                .Include(d => d.Producto)
                    .ThenInclude(p => p.Categoria) // Incluir la categor�a
                .Where(d => d.IdProveedor == proveedorId)
                .Where(d => d.Venta != null && d.Venta.EstadoDeLaVenta != null &&
                    d.Venta.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .OrderByDescending(d => d.Venta.FechaDeRegistro)
                .ToListAsync();

            if (!detalles.Any())
            {
                _logger.LogWarning("No se encontraron ventas para el proveedor con ID {ProveedorId}", proveedorId);
                return NotFound(new { mensaje = "No se encontraron ventas para el proveedor especificado" });
            }

            // Crear el archivo Excel
            _logger.LogInformation("Creando archivo Excel para el proveedor: {ProveedorNombre}", proveedor.Nombre);
            _logger.LogInformation("Detalles de ventas encontrados: {DetallesCount}", detalles.Count);
            var fileName = $"Ventas_Proveedor_{proveedor.Nombre.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "reportes", fileName);

            // Create directory with explicit error handling
            try
            {
                _logger.LogInformation("Creando directorio para reportes en: {ReportesPath}", Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));
                Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));
                // Debug line to help troubleshoot Docker permissions
                System.IO.File.WriteAllText(Path.Combine(_hostingEnvironment.WebRootPath, "reportes", "test.txt"), "Test write access");
                _logger.LogInformation("Directorio creado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando directorio para reportes: {ReportesPath}", Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));
                return StatusCode(500, new
                {
                    mensaje = $"Error creating report directory: {ex.Message}",
                    path = Path.Combine(_hostingEnvironment.WebRootPath, "reportes"),
                    exception = ex.ToString()
                });
            }

            using (var package = new ExcelPackage())
            {
                _logger.LogInformation("Creando archivo Excel en: {FilePath}", filePath);
                var worksheet = package.Workbook.Worksheets.Add("Ventas");

                // T�tulo principal del reporte
                worksheet.Cells[3, 1].Value = "Reporte de Ventas por Proveedor";
                worksheet.Cells[3, 1, 3, 9].Merge = true;
                worksheet.Cells[3, 1].Style.Font.Bold = true;
                worksheet.Cells[3, 1].Style.Font.Size = 16;
                worksheet.Cells[3, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[3, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                worksheet.Cells[3, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[3, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(180, 180, 180));

                TimeZoneInfo guatemalaZone;
                try
                {
                    // Intentar formato Windows
                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                }
                catch
                {
                    try
                    {
                        // Intentar formato Linux
                        guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                    }
                    catch
                    {
                        // Crear zona manualmente con UTC-6 para Guatemala
                        guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                    }
                }

                // Fecha y hora del reporte
                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
                worksheet.Cells[4, 1].Value = $"Fecha del Reporte: {localDateTime:dd/MM/yyyy hh:mm tt}";
                worksheet.Cells[4, 1, 4, 9].Merge = true;
                worksheet.Cells[4, 1].Style.Font.Italic = true;
                worksheet.Cells[4, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[4, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[4, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(220, 220, 220));

                // Informaci�n del proveedor (ajustado a nuevas filas)
                worksheet.Cells[6, 1].Value = "Informaci�n del Proveedor";
                worksheet.Cells[6, 1, 6, 6].Merge = true;
                worksheet.Cells[6, 1].Style.Font.Bold = true;
                worksheet.Cells[6, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[6, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[6, 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                worksheet.Cells[7, 1].Value = "Nombre";
                worksheet.Cells[7, 2].Value = proveedor.Nombre;
                worksheet.Cells[8, 1].Value = "Email";
                worksheet.Cells[8, 2].Value = proveedor.Email;
                worksheet.Cells[9, 1].Value = "Tel�fono";
                worksheet.Cells[9, 2].Value = proveedor.Telefono;
                worksheet.Cells[10, 1].Value = "Direcci�n";
                worksheet.Cells[10, 2].Value = proveedor.Direccion;

                // Estilo para informaci�n del proveedor
                using (var range = worksheet.Cells[7, 1, 10, 1])
                {
                    _logger.LogInformation("Estableciendo estilos para informaci�n del proveedor");
                    range.Style.Font.Bold = true;
                }

                _logger.LogInformation("Estableciendo encabezados de ventas en la hoja de Excel");
                // Encabezados de ventas
                worksheet.Cells[12, 1].Value = "Factura #";
                worksheet.Cells[12, 2].Value = "Cliente";
                worksheet.Cells[12, 3].Value = "Fecha";
                worksheet.Cells[12, 4].Value = "Producto";
                worksheet.Cells[12, 5].Value = "Categor�a";
                worksheet.Cells[12, 6].Value = "Cantidad";
                worksheet.Cells[12, 7].Value = "Subtotal";
                worksheet.Cells[12, 8].Value = "Pago";
                worksheet.Cells[12, 9].Value = "Cambio";

                // Estilo para encabezados
                using (var range = worksheet.Cells[12, 1, 12, 9])
                {
                    _logger.LogInformation("Estableciendo estilos para encabezados de ventas");
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                int row = 13;
                foreach (var detalle in detalles)
                {
                    try
                    {
                        _logger.LogInformation("Procesando detalle de venta ID: {DetalleId}", detalle.Id);
                        if (detalle.Venta == null) continue;

                        worksheet.Cells[row, 1].Value = detalle.Venta.NumeroDeFactura;
                        worksheet.Cells[row, 2].Value = detalle.Venta.Cliente != null ?
                            $"{detalle.Venta.Cliente.Nombre ?? ""} {detalle.Venta.Cliente.Apellido ?? ""}".Trim() :
                            "Cliente no encontrado";
                        worksheet.Cells[row, 3].Value = detalle.Venta.FechaDeRegistro;
                        // Formato de fecha con AM/PM
                        worksheet.Cells[row, 3].Style.Numberformat.Format = "dd/mm/yyyy hh:mm AM/PM";
                        worksheet.Cells[row, 4].Value = detalle.Producto != null ?
                            $"{detalle.Producto.Codigo ?? "Sin c�digo"} - {detalle.Producto.Nombre ?? "Sin nombre"}" :
                            "Producto no disponible";
                        // Nueva columna para categor�a
                        worksheet.Cells[row, 5].Value = detalle.Producto?.Categoria?.Nombre ?? "Sin categor�a";
                        worksheet.Cells[row, 6].Value = detalle.Cantidad;
                        worksheet.Cells[row, 7].Value = $"Q{detalle.Cantidad * detalle.PrecioDeVenta}";
                        worksheet.Cells[row, 7].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        // Nuevas columnas para pago y cambio
                        worksheet.Cells[row, 8].Value = $"Q{detalle.Venta.MontoDePago}";
                        worksheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheet.Cells[row, 9].Value = $"Q{detalle.Venta.MontoDeCambio}";
                        worksheet.Cells[row, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;

                        row++;
                        _logger.LogInformation("Processed detail for Venta ID {VentaId}", detalle.IdVenta);
                    }
                    catch (Exception ex)
                    {
                        // Log error but continue with next detail
                        _logger.LogError(ex, "Error processing detail for Venta ID {VentaId}", detalle.IdVenta);
                        System.Diagnostics.Debug.WriteLine($"Error processing detail: {ex.Message}");
                    }
                }

                // Calculate total
                decimal total = 0;
                foreach (var d in detalles)
                {
                    if (d != null)
                    {
                        total += d.Cantidad * d.PrecioDeVenta;
                    }
                }

                // Update the Total Ventas styling in ExportarVentasPorProveedor method
                worksheet.Cells[row + 1, 8].Value = "Total General:";
                worksheet.Cells[row + 1, 8].Style.Font.Bold = true;
                worksheet.Cells[row + 1, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row + 1, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row + 1, 8].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                worksheet.Cells[row + 1, 9].Value = $"Q{total}";
                worksheet.Cells[row + 1, 9].Style.Font.Bold = true;
                worksheet.Cells[row + 1, 9].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row + 1, 9].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells[row + 1, 9].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(200, 200, 200));

                // Establecer anchos fijos para las columnas:
                worksheet.Column(1).Width = 12; // Factura #
                worksheet.Column(2).Width = 25; // Cliente
                worksheet.Column(3).Width = 20; // Fecha
                worksheet.Column(4).Width = 30; // Producto
                worksheet.Column(5).Width = 15; // Categor�a
                worksheet.Column(6).Width = 10; // Cantidad
                worksheet.Column(7).Width = 12; // Subtotal
                worksheet.Column(8).Width = 12; // Pago
                worksheet.Column(9).Width = 12; // Cambio
                // Guardar el archivo Excel con explicit error handling
                try
                {
                    _logger.LogInformation("Guardando archivo Excel en: {FilePath}", filePath);
                    FileInfo file = new FileInfo(filePath);
                    package.SaveAs(file);
                    _logger.LogInformation("Excel file created successfully: {FilePath}", filePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving Excel file: {FilePath}", filePath);
                    return StatusCode(500, new
                    {
                        mensaje = $"Error saving Excel file: {ex.Message}",
                        path = filePath,
                        exception = ex.ToString()
                    });
                }
            }

            // Devolver la URL del archivo
            var fileUrl = $"/reportes/{fileName}";

            _logger.LogInformation("Excel file created successfully: {FilePath}", filePath);
            return Ok(new
            {
                mensaje = "Reporte por proveedor generado correctamente",
                url = fileUrl,
                nombreArchivo = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting sales for provider ID {ProveedorId}", proveedorId);
            return StatusCode(500, new
            {
                mensaje = $"Error general al exportar las ventas por proveedor: {ex.Message}",
                stackTrace = ex.StackTrace
            });
        }
    }

    [HttpGet("exportar/proveedor/{proveedorId}/pdf")]
    public async Task<IActionResult> ExportarVentasPorProveedorPDF(int proveedorId)
    {
        try
        {
            QuestPDF.Settings.EnableDebugging = true;
            _logger.LogInformation("Exportando ventas para el proveedor con ID (PDF): {ProveedorId}", proveedorId);
            var proveedor = await _context.Proveedores.FindAsync(proveedorId);
            if (proveedor == null)
            {
                _logger.LogWarning("Proveedor con ID {ProveedorId} no encontrado", proveedorId);
                return NotFound(new { mensaje = "Proveedor no encontrado" });
            }

            var detalles = await _context.DetallesDeLaVenta
                .Include(d => d.Venta)
                    .ThenInclude(v => v.Cliente)
                .Include(d => d.Venta)
                    .ThenInclude(v => v.EstadoDeLaVenta)
                .Include(d => d.Producto)
                    .ThenInclude(p => p.Categoria)
                .Where(d => d.IdProveedor == proveedorId)
                .Where(d => d.Venta != null && d.Venta.EstadoDeLaVenta != null &&
                    d.Venta.EstadoDeLaVenta.Nombre.ToLower() == "entregada")
                .OrderByDescending(d => d.Venta.FechaDeRegistro)
                .ToListAsync();

            if (!detalles.Any())
            {
                _logger.LogWarning("No se encontraron ventas para el proveedor con ID {ProveedorId}", proveedorId);
                return NotFound(new { mensaje = "No se encontraron ventas para el proveedor especificado" });
            }

            _logger.LogInformation("Detalles de ventas encontrados: {DetallesCount}", detalles.Count);
            _logger.LogInformation("Creando archivo PDF para el proveedor: {ProveedorNombre}", proveedor.Nombre);
            // Create PDF
            var fileName = $"Ventas_Proveedor_{proveedor.Nombre.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "reportes", fileName);

            // Ensure directory exists
            Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, "reportes"));

            // Generate PDF document
            var document = Document.Create(container =>
            {
                _logger.LogInformation("Creando documento PDF en: {FilePath}", filePath);
                container.Page(page =>
                {
                    _logger.LogInformation("Configurando p�gina PDF");
                    // Set page settings
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    _logger.LogInformation("Configurando contenido de la p�gina PDF");
                    // Header - Fixed by using Column instead of multiple Element calls
                    page.Header().Column(column => {
                        _logger.LogInformation("Configurando encabezado PDF");
                        // Title row
                        column.Item().Row(row => {
                            _logger.LogInformation("Configurando fila de t�tulo PDF");
                            row.RelativeItem().Column(col => {
                                col.Item().AlignCenter().Text("Reporte de Ventas por Proveedor").FontSize(20).Bold();

                                TimeZoneInfo guatemalaZone;
                                try
                                {
                                    guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
                                }
                                catch
                                {
                                    try
                                    {
                                        guatemalaZone = TimeZoneInfo.FindSystemTimeZoneById("America/Guatemala");
                                    }
                                    catch
                                    {
                                        guatemalaZone = TimeZoneInfo.CreateCustomTimeZone("Guatemala", new TimeSpan(-6, 0, 0), "Guatemala", "GT");
                                    }
                                }
                                DateTime localDateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, guatemalaZone);
                                col.Item().Text($"Fecha del Reporte: {localDateTime:dd/MM/yyyy hh:mm tt}").FontSize(10);
                            });
                        });

                        _logger.LogInformation("Configurando informaci�n del proveedor en el encabezado PDF");
                        // Provider info section with border
                        column.Item().PaddingTop(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                        column.Item().PaddingTop(10).Text("Informaci�n del Proveedor").Bold();
                        column.Item().Table(table => {
                            // Define columns
                            table.ColumnsDefinition(columns => {
                                _logger.LogInformation("Definiendo columnas para la tabla de informaci�n del proveedor PDF");
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            _logger.LogInformation("Agregando filas a la tabla de informaci�n del proveedor PDF");
                            // Add rows
                            table.Cell().Text("Nombre:").Bold();
                            table.Cell().Text(proveedor.Nombre);

                            table.Cell().Text("Email:").Bold();
                            table.Cell().Text(proveedor.Email);

                            table.Cell().Text("Tel�fono:").Bold();
                            table.Cell().Text(proveedor.Telefono);

                            table.Cell().Text("Direcci�n:").Bold();
                            table.Cell().Text(proveedor.Direccion);
                        });
                        _logger.LogInformation("Configurando separaci�n entre encabezado y contenido PDF");
                        column.Item().PaddingBottom(10).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                    });

                    _logger.LogInformation("Configurando contenido de la p�gina PDF");
                    // Content
                    page.Content().Column(column => {
                        // Sales table header as an item in the column
                        column.Item().PaddingTop(10).Text("Detalle de Ventas").FontSize(14).Bold();

                        // Table as another item in the column
                        column.Item().PaddingTop(10).Table(table => {
                            // Define columns
                            table.ColumnsDefinition(columns => {
                                columns.ConstantColumn(50);     // Factura # - reducido
                                columns.RelativeColumn(3);      // Cliente - relativo
                                columns.RelativeColumn(3);      // Fecha - relativo
                                columns.RelativeColumn(4);      // Producto - relativo
                                columns.RelativeColumn(2);      // Categor�a - relativo 
                                columns.ConstantColumn(50);     // Cantidad - reducido
                                columns.ConstantColumn(50);     // Subtotal - reducido
                                columns.ConstantColumn(50);     // Pago - reducido
                                columns.ConstantColumn(50);     // Cambio - reducido
                            });

                            // Add header
                            table.Header(header => {
                                _logger.LogInformation("Agregando encabezado a la tabla de ventas PDF");
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Factura #").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cliente").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Fecha").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Producto").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Categor�a").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cantidad").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Subtotal").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Pago").Bold();
                                header.Cell().Background(Colors.Grey.Lighten2).Text("Cambio").Bold();
                            });

                            // Add rows
                            foreach (var detalle in detalles)
                            {
                                _logger.LogInformation("Agregando fila a la tabla de ventas PDF para detalle ID: {DetalleId}", detalle.Id);
                                string clienteNombre = detalle.Venta?.Cliente != null
                                    ? $"{detalle.Venta.Cliente.Nombre} {detalle.Venta.Cliente.Apellido}"
                                    : "Cliente no encontrado";

                                string productoNombre = detalle.Producto != null
                                    ? $"{detalle.Producto.Codigo} - {detalle.Producto.Nombre}"
                                    : "Producto no disponible";

                                string categoria = detalle.Producto?.Categoria?.Nombre ?? "Sin categor�a";
                                string fechaFormateada = detalle.Venta.FechaDeRegistro.ToString("dd/MM/yyyy hh:mm tt");
                                decimal subtotal = detalle.Cantidad * detalle.PrecioDeVenta;

                                table.Cell().Text(detalle.Venta.NumeroDeFactura);
                                table.Cell().Text(clienteNombre);
                                table.Cell().Text(fechaFormateada);

                                // Product with image
                                table.Cell().Element(cell => {
                                    cell.Row(row => {
                                        string imagePath = detalle.Producto?.ImagenUrl ?? "/images/productos/default.png";
                                        if (!string.IsNullOrEmpty(imagePath))
                                        {
                                            string fullPath = Path.Combine(_hostingEnvironment.WebRootPath, imagePath.TrimStart('/'));
                                            if (System.IO.File.Exists(fullPath))
                                            {
                                                try
                                                {
                                                    // Limita el tama�o de la imagen
                                                    row.ConstantItem(30).Image(fullPath)
                                                        .FitArea();
                                                }
                                                catch
                                                {
                                                    _logger.LogWarning("Error loading image from path: {ImagePath}", fullPath);
                                                }
                                            }
                                        }
                                        // Texto con recorte
                                        row.RelativeItem().Text(productoNombre)
                                            .WrapAnywhere();
                                    });
                                });

                                _logger.LogInformation("Agregando categor�a y cantidad a la tabla de ventas PDF");
                                table.Cell().AlignCenter().Text(categoria);
                                table.Cell().Text(detalle.Cantidad.ToString());
                                table.Cell().Text($"Q {subtotal:F2}");
                                table.Cell().Text($"Q {detalle.Venta.MontoDePago:F2}");
                                table.Cell().Text($"Q {detalle.Venta.MontoDeCambio:F2}");
                            }

                            _logger.LogInformation("Agregando fila total a la tabla de ventas PDF");
                            // Add total row
                            decimal total = detalles.Sum(d => d.Cantidad * d.PrecioDeVenta);
                            table.Cell().ColumnSpan(8).AlignRight().Text(text => text.Span("Total General:").Bold());
                            table.Cell().Text($"Q {total:F2}").Bold();
                            table.Cell().ColumnSpan(2);
                        });
                    });

                    _logger.LogInformation("Configurando pie de p�gina PDF");
                    // Footer
                    page.Footer().AlignCenter().Text(text => {
                        text.Span("P�gina ").FontSize(10);
                        text.CurrentPageNumber().FontSize(10);
                        text.Span(" de ").FontSize(10);
                        text.TotalPages().FontSize(10);
                    });
                });
            });

            _logger.LogInformation("Generando PDF en: {FilePath}", filePath);
            // Save the PDF
            document.GeneratePdf(filePath);

            // Return the file URL
            var fileUrl = $"/reportes/{fileName}";

            return Ok(new
            {
                mensaje = "Reporte PDF por proveedor generado correctamente",
                url = fileUrl,
                nombreArchivo = fileName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando reporte PDF para proveedor ID {ProveedorId}", proveedorId);
            return StatusCode(500, new
            {
                mensaje = $"Error al exportar las ventas por proveedor en PDF: {ex.Message}",
                stackTrace = ex.StackTrace
            });
        }
    }
}