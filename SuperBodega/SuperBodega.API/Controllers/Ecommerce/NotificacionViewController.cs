using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers.Ecommerce;

/// <summary>
/// Controlador para gestionar las vistas relacionadas con las notificaciones.
/// </summary>
[Route("Notificaciones")]
public class NotificacionViewController : Controller
{
    /// <summary>
    /// Muestra la página de listado de las notificaciones.
    /// </summary>
    /// <returns>Vista de listado de notificaciones</returns>
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }
}