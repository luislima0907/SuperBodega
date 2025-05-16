using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers.Ecommerce
{
    [Route("MisPedidos")]
    public class MisPedidosViewController : Controller
    {
        // GET: /MisPedidos/Index
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
    }
}