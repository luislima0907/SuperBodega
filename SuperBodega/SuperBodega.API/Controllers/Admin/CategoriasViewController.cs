using Microsoft.AspNetCore.Mvc;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para las vistas relacionadas con la gestión de categorias.
/// </summary>
/// <remarks>
/// Proporciona acceso a las páginas web para administrar categorias,
/// como listado, creación, edición y visualización de detalles.
/// </remarks>
[Route("Categorias")]
public class CategoriaViewController : Controller
{
    private readonly CategoriaService _categoriaService;
    
    /// <summary>
    /// Inicializa una nueva instancia del controlador de vistas de categorias.
    /// </summary>
    /// <param name="categoriaService">Servicio de categorias a utilizar</param>
    public CategoriaViewController(CategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }
    
    /// <summary>
    /// Muestra la página de listado de las categorias.
    /// </summary>
    /// <returns>Vista de listado de categorias</returns>
    /// <remarks>
    /// Esta vista carga los datos dinámicamente mediante JavaScript.
    /// </remarks>
    [HttpGet("Index")]
    public IActionResult Index()
    {
        return View();
    }

    /// <summary>
    /// Muestra la página para crear un nueva categoria.
    /// </summary>
    /// <returns>Vista de creación de categorias</returns>
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }
    
    /// <summary>
    /// Muestra la página para editar una categoria existente.
    /// </summary>
    /// <param name="id">ID de la categoria a editar</param>
    /// <returns>Vista de edición de categorias</returns>
    /// <remarks>
    /// Si la categoria no existe, retorna NotFound (404).
    /// </remarks>
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }
        return View(categoria);
    }
}