using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para las vistas relacionadas con la gestión de productos.
/// Proporciona acceso a las páginas web para administrar productos,
/// como listado, creación, edición y eliminación.
/// </summary>
/// <remarks>
/// Este controlador utiliza servicios para interactuar con la base de datos
/// y manejar la lógica de negocio relacionada con los productos.
/// Las vistas se generan dinámicamente mediante Razor.
/// </remarks>
[Route("Productos")]
public class ProductoViewController : Controller
{
    private readonly ProductoService _productoService;
    private readonly CategoriaService _categoriaService;
    private readonly IWebHostEnvironment _webHostEnvironment;
    
    /// <summary>
    /// Inicializa una nueva instancia del controlador de vistas de productos.
    /// </summary>
    /// <param name="productoService">Servicio de productos a utilizar</param>
    /// <param name="categoriaService">Servicio de categorías a utilizar</param>
    /// <param name="webHostEnvironment">Entorno web para acceder a archivos estáticos</param>
    public ProductoViewController(ProductoService productoService, CategoriaService categoriaService, IWebHostEnvironment webHostEnvironment)
    {
        _productoService = productoService;
        _categoriaService = categoriaService;
        _webHostEnvironment = webHostEnvironment;
    }
    
    /// <summary>
    /// Muestra la página de listado de productos.
    /// </summary>
    /// <returns>Vista de listado de productos</returns>
    /// <remarks>
    /// Esta vista carga los datos dinámicamente mediante JavaScript.
    /// </remarks>
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var productos = await _productoService.GetAllProductosAsync();
        return View(productos);
    }

    /// <summary>
    /// Muestra la página para crear un nuevo producto.
    /// </summary>
    /// <returns>Vista de creación de producto</returns>
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        try
        {
            // Obtener todas las categorías para el dropdown
            var categorias = await _categoriaService.GetAllCategoriasAsync();
            
            // Convertir a SelectListItems con información de estado
            var categoriasSelectList = categorias.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Estado ? c.Nombre : $"{c.Nombre} [INACTIVA]", // Indicar si la categoría está inactiva
                Selected = false,
                Group = new SelectListGroup { Name = c.Estado ? "Categorías Activas" : "Categorías Inactivas" }
            }).ToList();
            
            // Pasar la lista completa de categorías para poder acceder a todas sus propiedades en la vista
            ViewBag.Categorias = categoriasSelectList;
            ViewBag.CategoriasCompletas = categorias; // Las categorías con toda su información

            return View(new CreateProductoDTO { Estado = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Error al cargar las categorías" });
        }
    }

    /// <summary>
    /// Muestra la página para editar un producto existente.
    /// </summary>
    /// <param name="id">ID del producto a editar</param>
    /// <returns>Vista de edición de producto</returns>
    /// <remarks>
    /// Si el producto no existe, retorna NotFound (404).
    /// </remarks>
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _productoService.GetProductoByIdAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        // Obtener todas las categorías para el dropdown
        var categorias = await _categoriaService.GetAllCategoriasAsync();
        
        // Convertir a SelectListItems con información de estado
        var categoriasSelectList = categorias.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Estado ? c.Nombre : $"{c.Nombre} [INACTIVA]",
            Selected = c.Id == producto.CategoriaId,
            Group = new SelectListGroup { Name = c.Estado ? "Categorías Activas" : "Categorías Inactivas" }
        }).ToList();

        // Verificar si la categoría del producto está inactiva
        var categoriaActual = categorias.FirstOrDefault(c => c.Id == producto.CategoriaId);
        if (categoriaActual != null && !categoriaActual.Estado)
        {
            ViewBag.CategoriaInactiva = true;
            TempData["Warning"] = "La categoría de este producto está inactiva. Se recomienda seleccionar una categoría activa.";
        }

        var updateProductoDTO = new UpdateProductoDTO
        {
            Codigo = producto.Codigo,
            Nombre = producto.Nombre,
            Descripcion = producto.Descripcion,
            CategoriaId = producto.CategoriaId,
            PrecioDeCompra = producto.PrecioDeCompra,
            PrecioDeVenta = producto.PrecioDeVenta,
            Estado = producto.Estado,
            ImagenUrl = producto.ImagenUrl
        };

        ViewBag.Categorias = categoriasSelectList;
        ViewBag.CategoriasCompletas = categorias; // Las categorías con toda su información
        ViewBag.Producto = producto;
        ViewBag.ProductoId = id;
        ViewBag.CategoriaOriginalId = producto.CategoriaId; // Para validación en el frontend

        return View(updateProductoDTO);
    }

    // [HttpDelete("Delete/{id}")]
    // public async Task<IActionResult> Delete(int id)
    // {
    //     var producto = await _productoService.GetProductoByIdAsync(id);
    //     if (producto == null)
    //     {
    //         return NotFound();
    //     }
    //
    //     // Eliminar la imagen si existe
    //     if (!string.IsNullOrEmpty(producto.ImagenUrl))
    //     {
    //         var rutaImagen = Path.Combine(_webHostEnvironment.WebRootPath, producto.ImagenUrl.TrimStart('/'));
    //         if (System.IO.File.Exists(rutaImagen))
    //         {
    //             System.IO.File.Delete(rutaImagen);
    //         }
    //     }
    //
    //     var result = await _productoService.DeleteProductoAsync(id);
    //     if (!result)
    //     {
    //         return StatusCode(500, new { success = false, message = "No se pudo eliminar el producto" });
    //     }
    //
    //     return Json(new { success = true, message = "Producto eliminado correctamente" });
    // }
}