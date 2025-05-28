using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Ecommerce
{
    /// <summary>
    /// Controlador para gestionar las vistas relacionadas con el catálogo de productos.
    /// </summary>
    [Route("Catalogo")]
    public class ProductoCatalogoViewController : Controller
    {
        private readonly ILogger<ProductoCatalogoController> _logger;
        private readonly ProductoService _productoService;
        private readonly CategoriaService _categoriaService;

        /// <summary>
        /// Inicializa una nueva instancia del controlador de vistas del catálogo de productos.
        /// </summary>
        /// <param name="logger">Instancia del logger para registrar eventos</param>
        /// <param name="productoService">Servicio de productos a utilizar</param>
        /// <param name="categoriaService">Servicio de categorías a utilizar</param>
        public ProductoCatalogoViewController(
            ILogger<ProductoCatalogoController> logger,
            ProductoService productoService,
            CategoriaService categoriaService)
        {
            _logger = logger;
            _productoService = productoService;
            _categoriaService = categoriaService;
        }

        /// <summary>
        /// Muestra la página de listado de productos del catálogo.
        /// </summary>
        /// <returns>Vista de listado con el catalogo de productos</returns>
        // GET: /ProductoCatalogo
        [HttpGet("Productos")]
        public IActionResult Index()
        {
            _logger.LogInformation("Accediendo al catálogo de productos");
            return View();
        }

        /// <summary>
        /// Muestra la página de listado de categorías del catálogo.
        /// </summary>
        /// <returns>Vista de listado con el catalogo de categorías</returns>
        // GET: /ProductoCatalogo/Categorias
        [HttpGet("Categorias")]
        public async Task<IActionResult> Categorias()
        {
            var categorias = await _categoriaService.GetAllCategoriasAsync();
            var activeCategorias = categorias.Where(c => c.Estado);
            
            return View(activeCategorias);
        }

        /// <summary>
        /// Muestra la página de listado de productos por categoría.
        /// </summary>
        /// <param name="id">ID de la categoría a filtrar</param>
        /// <returns>Vista de listado con los productos de la categoría</returns>
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