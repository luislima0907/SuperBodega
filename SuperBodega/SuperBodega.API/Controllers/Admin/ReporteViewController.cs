using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

[Route("Reportes")]
public class ReporteViewController : Controller
{

    public ReporteViewController()
    {
    }

    [HttpGet("VentasPorPeriodo")]
    public IActionResult VentasPorPeriodo()
    {
        return View();
    }

    [HttpGet("VentasPorCliente")]
    public IActionResult VentasPorCliente()
    {
        return View();
    }

    [HttpGet("VentasPorProducto")]
    public IActionResult VentasPorProducto()
    {
        return View();
    }

    [HttpGet("VentasPorProveedor")]
    public IActionResult VentasPorProveedor()
    {
        return View();
    }

}