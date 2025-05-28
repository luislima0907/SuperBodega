using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar categorias en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class CategoriaController : ControllerBase
{
    private readonly CategoriaService _categoriaService;

    public CategoriaController(CategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    /// <summary>
    /// Obtiene todas las categorias registrados en el sistema
    /// </summary>
    /// <returns>Lista de todas las categorias</returns>
    /// <response code="200">Retorna la lista de categorias</response>
    /// <response code="404">No se encontraron categorias.</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CategoriaDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        var categorias = await _categoriaService.GetAllCategoriasAsync();
        return Ok(categorias);
    }

    /// <summary>
    /// Obtiene una categoria por su ID
    /// </summary>
    /// <param name="id">ID de la categoria a buscar</param>
    /// <returns>Datos de la categoria solicitada</returns>
    /// <response code="200">Devuelve la categoria solicitada</response>
    /// <response code="404">Si la categoria no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoriaDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(int id)
    {
        var categoria = await _categoriaService.GetCategoriaByIdAsync(id);
        if (categoria == null)
        {
            return NotFound();
        }
        return Ok(categoria);
    }

    /// <summary>
    /// Crea una nueva categoria en el sistema
    /// </summary>
    /// <param name="categoriaDto">Datos de la categoria a crear</param>
    /// <returns>La categoria recién creada con su ID asignado</returns>
    /// <response code="201">Retorna la nueva categoria creada</response>
    /// <response code="400">Si los datos suministrados son inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CategoriaDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(CreateCategoriaDTO categoriaDto)
    {
        var newCategoria = await _categoriaService.CreateCategoriaAsync(categoriaDto);
        return CreatedAtAction(nameof(GetById), new { id = newCategoria.Id }, newCategoria);
    }

    /// <summary>
    /// Actualiza los datos de una categoria existente
    /// </summary>
    /// <param name="id">ID de la categoria a actualizar</param>
    /// <param name="updateCategoriaDto">Datos actualizados de la categoria</param>
    /// <returns>La categoria con sus datos actualizados</returns>
    /// <response code="200">Si la categoria se actualizó correctamente</response>
    /// <response code="400">Si los datos suministrados son inválidos</response>
    /// <response code="404">Si la categoria no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("Edit/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CategoriaDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(int id, UpdateCategoriaDTO updateCategoriaDto)
    {
        var updatedCategoria = await _categoriaService.UpdateCategoriaAsync(id, updateCategoriaDto);
        if (updatedCategoria == null)
        {
            return NotFound();
        }
        return Ok(updatedCategoria);
    }

    /// <summary>
    /// Elimina una categoria del sistema
    /// </summary>
    /// <param name="id">ID de la categoria a eliminar</param>
    /// <returns>Sin contenido si se eliminó correctamente</returns>
    /// <response code="204">Si la categoria se eliminó correctamente</response>
    /// <response code="404">Si la categoria no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("Delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoriaService.DeleteCategoriaAsync(id);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}