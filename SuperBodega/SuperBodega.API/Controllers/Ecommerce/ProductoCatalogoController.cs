using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Ecommerce
{
    /// <summary>
    /// Controlador para gestionar el catálogo de productos.
    /// Proporciona métodos para obtener productos y categorías,
    /// así como para filtrar y paginar los resultados.
    /// </summary>
    [Route("api/ecommerce/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class ProductoCatalogoController : ControllerBase
    {
        private readonly ProductoService _productoService;
        private readonly CategoriaService _categoriaService;

        public ProductoCatalogoController(ProductoService productoService, CategoriaService categoriaService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
        }

        /// <summary>
        /// Obtiene todos los productos del catálogo.
        /// Incluye opciones de paginación y búsqueda.
        /// </summary>
        /// <param name="page">Número de página para la paginación (default: 1)</param>
        /// <param name="pageSize">Número de elementos por página (default: 12)</param>
        /// <param name="search">Texto para filtrar productos por nombre, descripción o categoría</param>
        /// <returns>Lista de productos paginada y filtrada</returns>
        /// <response code="200">Retorna la lista de productos</response>
        /// <response code="404">No se encontraron productos.</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductoDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 12, [FromQuery] string search = null)
        {
            var allProductos = await _productoService.GetAllProductosAsync();
            var categorias = await _categoriaService.GetAllCategoriasAsync();
            
            // Crear un diccionario para acceso rápido a las categorías por ID
            var categoriaDict = categorias.ToDictionary(c => c.Id, c => c);
            
            // Enriquecer productos con información de categoría
            var enrichedProductos = allProductos
                .Where(p => p.Estado) // Solo productos activos
                .Select(p => new {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Stock = p.Stock,
                    PrecioVenta = p.PrecioDeVenta,
                    Estado = p.Estado,
                    FechaDeRegistro = p.FechaDeRegistro,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = p.CategoriaId > 0 && categoriaDict.ContainsKey(p.CategoriaId) ? 
                        categoriaDict[p.CategoriaId].Nombre : "Sin categoría",
                    ImagenUrl = p.ImagenUrl ?? "/images/productos/default.png"
                });
                
            // Aplicar búsqueda si se proporciona
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                enrichedProductos = enrichedProductos
                    .Where(p => 
                        p.Nombre.ToLower().Contains(search) || 
                        (p.Descripcion != null && p.Descripcion.ToLower().Contains(search)) ||
                        p.CategoriaNombre.ToLower().Contains(search)
                    );
            }
            
            // Ordenar por fecha de registro descendente
            enrichedProductos = enrichedProductos.OrderByDescending(p => p.FechaDeRegistro);
                
            var totalItems = enrichedProductos.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            var paginatedProductos = enrichedProductos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
                
            return Ok(new {
                productos = paginatedProductos,
                currentPage = page,
                pageSize = pageSize,
                totalItems = totalItems,
                totalPages = totalPages
            });
        }

        /// <summary>
        /// Obtiene productos por categoría.
        /// Incluye opciones de paginación y búsqueda.
        /// </summary>
        /// <param name="categoriaId">ID de la categoría</param>
        /// <param name="page">Número de página para la paginación (default: 1)</param>
        /// <param name="pageSize">Número de elementos por página (default: 12)</param>
        /// <param name="search">Texto para filtrar productos por nombre o descripción</param>
        /// <returns>Lista de productos paginada y filtrada</returns>
        /// <response code="200">Retorna la lista de productos</response>
        /// <response code="404">No se encontraron productos o la categoría no existe.</response>
        /// <response code="500">Error interno del servidor</response>
        [HttpGet("categoria/{categoriaId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductoDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByCategoria(int categoriaId, [FromQuery] int page = 1, [FromQuery] int pageSize = 12, [FromQuery] string search = null)
        {
            // Verificar primero si existe la categoría
            var categoria = await _categoriaService.GetCategoriaByIdAsync(categoriaId);
            if (categoria == null)
            {
                return NotFound("La categoría no existe");
            }
            
            // Solo mostrar categorías activas
            if (!categoria.Estado)
            {
                return BadRequest("La categoría no está activa");
            }
            
            var allProductos = await _productoService.GetProductosByCategoriaIdAsync(categoriaId);
            
            // Enriquecer productos con información de categoría
            var enrichedProductos = allProductos
                .Where(p => p.Estado) // Solo productos activos
                .Select(p => new {
                    Id = p.Id,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Stock = p.Stock,
                    PrecioVenta = p.PrecioDeVenta,
                    Estado = p.Estado,
                    FechaDeRegistro = p.FechaDeRegistro,
                    CategoriaId = p.CategoriaId,
                    CategoriaNombre = categoria.Nombre,
                    ImagenUrl = p.ImagenUrl ?? "/images/productos/default.png"
                });
                
            // Aplicar búsqueda si se proporciona
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                enrichedProductos = enrichedProductos
                    .Where(p => 
                        p.Nombre.ToLower().Contains(search) || 
                        (p.Descripcion != null && p.Descripcion.ToLower().Contains(search))
                    );
            }
            
            // Ordenar por fecha de registro descendente
            enrichedProductos = enrichedProductos.OrderByDescending(p => p.FechaDeRegistro);
                
            var totalItems = enrichedProductos.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            
            var paginatedProductos = enrichedProductos
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
                
            return Ok(new {
                productos = paginatedProductos,
                currentPage = page,
                pageSize = pageSize,
                totalItems = totalItems,
                totalPages = totalPages,
                categoria = categoria
            });
        }
        
        // GET: api/ecommerce/ProductoCatalogo/categorias
        [HttpGet("categorias")]
        public async Task<IActionResult> GetCategorias()
        {
            var allCategorias = await _categoriaService.GetAllCategoriasAsync();
            var activeCategorias = allCategorias.Where(c => c.Estado);
            return Ok(activeCategorias);
        }
    }
}