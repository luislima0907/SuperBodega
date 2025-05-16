using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers.Ecommerce;

[Route("Notificaciones")]
public class NotificacionViewController : Controller
{
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }
}