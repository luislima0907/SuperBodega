using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para las vistas relacionadas con los reportes.
/// </summary>
/// <remarks>
/// Proporciona acceso a las páginas web para generar reportes
/// como ventas por periodo, cliente, producto y proveedor.
/// </remarks>
[Route("Reportes")]
public class ReporteViewController : Controller
{
    
    public ReporteViewController()
    {
    }

    /// <summary>
    /// Muestra la página de reportes por periodo.
    /// </summary>
    /// <returns>Vista de reportes por periodo</returns>
    [HttpGet("VentasPorPeriodo")]
    public IActionResult VentasPorPeriodo()
    {
        return View();
    }

    /// <summary>
    /// Muestra la página de reportes por cliente.
    /// </summary>
    /// <returns>Vista de reportes por cliente</returns>
    [HttpGet("VentasPorCliente")]
    public IActionResult VentasPorCliente()
    {
        return View();
    }

    /// <summary>
    /// Muestra la página de reportes por producto.
    /// </summary>
    /// <returns>Vista de reportes por producto</returns>
    [HttpGet("VentasPorProducto")]
    public IActionResult VentasPorProducto()
    {
        return View();
    }

    /// <summary>
    /// Muestra la página de reportes por proveedor.
    /// </summary>
    /// <returns>Vista de reportes por proveedor</returns>
    [HttpGet("VentasPorProveedor")]
    public IActionResult VentasPorProveedor()
    {
        return View();
    }
}