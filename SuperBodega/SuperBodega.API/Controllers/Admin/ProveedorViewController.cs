using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para las vistas relacionadas con la gestión de proveedores.
/// </summary>
/// <remarks>
/// Proporciona acceso a las páginas web para administrar proveedores,
/// como listado, creación, edición y eliminación.
/// </remarks>
[Route("Proveedores")]
public class ProveedorViewController : Controller
{
    private readonly ProveedorService _proveedorService;
    
    /// <summary>
    /// Inicializa una nueva instancia del controlador de vistas de proveedores.
    /// </summary>
    public ProveedorViewController(ProveedorService proveedorService)
    {
        _proveedorService = proveedorService;
    }
    
    /// <summary>
    /// Muestra la página de listado de proveedores.
    /// </summary>
    /// <returns>Vista de listado de proveedores</returns>
    /// <remarks>
    /// Esta vista carga los datos dinámicamente mediante JavaScript.
    /// </remarks>
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }
    
    /// <summary>
    /// Muestra la página para crear un nuevo proveedor.
    /// </summary>
    /// <returns>Vista de creación de proveedor</returns>
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Muestra la página para editar un proveedor existente.
    /// </summary>
    /// <param name="id">ID del proveedor a editar</param>
    /// <returns>Vista de edición de proveedor</returns>
    /// <remarks>
    /// Si el proveedor no existe, retorna NotFound (404).
    /// </remarks>
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var proveedor = await _proveedorService.GetProveedorByIdAsync(id);
        if (proveedor == null)
        {
            return NotFound();
        }
        return View(proveedor);
    }
}