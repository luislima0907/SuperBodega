using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers.Ecommerce
{
    [Route("RealizarCompra")]
    public class RealizarCompraViewController : Controller
    {
        // GET: /RealizarCompra/Index
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}