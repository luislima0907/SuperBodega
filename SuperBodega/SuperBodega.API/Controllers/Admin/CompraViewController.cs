using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para las vistas relacionadas con la gestión de compras.
/// </summary>
/// <remarks>
/// Proporciona acceso a las páginas web para administrar compras,
/// como listado, creación, edición y visualización de detalles.
/// </remarks>
[Route("Compras")]
public class CompraViewController : Controller
{
    private readonly CompraService _compraService;
    private readonly ProveedorService _proveedorService;
    private readonly ProductoService _productoService;

    /// <summary>
    /// Inicializa una nueva instancia del controlador de vistas de compras.
    /// </summary>
    /// <param name="compraService">Servicio de compras a utilizar</param>
    /// <param name="proveedorService">Servicio de proveedores a utilizar</param>
    /// <param name="productoService">Servicio de productos a utilizar</param>
    public CompraViewController
    (
        CompraService compraService, 
        ProveedorService proveedorService,
        ProductoService productoService
        )
    {
        _compraService = compraService;
        _proveedorService = proveedorService;
        _productoService = productoService;
    }

    /// <summary>
    /// Muestra la página de listado de compras.
    /// </summary>
    /// <returns>Vista de listado de compras</returns>
    /// <remarks>
    /// Esta vista carga los datos dinámicamente mediante JavaScript.
    /// </remarks>
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        var compras = await _compraService.GetAllWithDetailsAsync();
        return View(compras);
    }

    /// <summary>
    /// Muestra la página de detalles de una compra específica.
    /// </summary>
    /// <param name="id">ID de la compra a mostrar</param>
    /// <returns>Vista de detalles de la compra</returns>
    /// <remarks>
    /// Si la compra no existe, retorna NotFound (404).
    /// </remarks>
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id)
    {
        var compra = await _compraService.GetWithDetailsAsync(id);
        if (compra == null)
        {
            return NotFound();
        }
        return View(compra);
    }

    /// <summary>
    /// Muestra la página para crear una nueva compra.
    /// </summary>
    /// <returns>Vista de creación de compra</returns>
    /// <remarks>
    /// Esta vista carga la lista de proveedores para el dropdown.
    /// </remarks>
    /// <response code="200">Retorna la vista de creación de compra</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Proveedores = await _proveedorService.GetAllProveedoresAsync();
        return View();
    }

    /// <summary>
    /// Muestra la página para editar una compra existente.
    /// </summary>
    /// <param name="id">ID de la compra a editar</param>
    /// <returns>Vista de edición de compra</returns>
    /// <remarks>
    /// Si la compra no existe, retorna NotFound (404).
    /// </remarks>
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var compra = await _compraService.GetWithDetailsAsync(id);
            if (compra == null)
            {
                TempData["Error"] = $"No se encontró la compra con ID {id}";
                return RedirectToAction(nameof(Index));
            }

            // Obtener lista de proveedores para el dropdown, incluyendo su estado
            var proveedores = await _proveedorService.GetAllProveedoresAsync();
                
            // Verificar si el proveedor actual está inactivo
            var proveedorActual = proveedores.FirstOrDefault(p => p.Id == compra.IdProveedor);
            if (proveedorActual != null && !proveedorActual.Estado)
            {
                ViewBag.ProveedorInactivo = true;
                TempData["Warning"] = "El proveedor de esta compra está inactivo. Se recomienda seleccionar un proveedor activo.";
            }

            ViewBag.Proveedores = proveedores;

            return View(compra);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al cargar la compra: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }
}