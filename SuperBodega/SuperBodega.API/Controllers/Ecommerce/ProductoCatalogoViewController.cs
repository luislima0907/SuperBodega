using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Ecommerce
{
    [Route("Catalogo")]
    public class ProductoCatalogoViewController : Controller
    {
        private readonly ILogger<ProductoCatalogoController> _logger;
        private readonly ProductoService _productoService;
        private readonly CategoriaService _categoriaService;

        public ProductoCatalogoViewController(
            ILogger<ProductoCatalogoController> logger,
            ProductoService productoService,
            CategoriaService categoriaService)
        {
            _logger = logger;
            _productoService = productoService;
            _categoriaService = categoriaService;
        }

        // GET: /ProductoCatalogo
        [HttpGet("Productos")]
        public IActionResult Index()
        {
            _logger.LogInformation("Accediendo al catálogo de productos");
            return View();
        }

        // GET: /ProductoCatalogo/Categorias
        [HttpGet("Categorias")]
        public async Task<IActionResult> Categorias()
        {
            var categorias = await _categoriaService.GetAllCategoriasAsync();
            var activeCategorias = categorias.Where(c => c.Estado);
            
            return View(activeCategorias);
        }

        // GET: /ProductoCatalogo/Categoria/5
        [HttpGet("Categoria/{id}")]
        public async Task<IActionResult> Categoria(int id)
        {
            var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
            if (categoria == null || !categoria.Estado)
            {
                _logger.LogWarning("Intento de acceder a categoría inválida: {CategoriaId}", id);
                return RedirectToAction(nameof(Index));
            }
            
            ViewData["CategoriaId"] = id;
            ViewData["CategoriaNombre"] = categoria.Nombre;
            
            return View();
        }
    }
}