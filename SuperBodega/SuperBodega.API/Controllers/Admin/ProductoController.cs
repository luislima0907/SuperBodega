using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Services.Admin;

namespace SuperBodega.API.Controllers.Admin;

/// <summary>
/// Controlador para gestionar productos en el sistema
/// </summary>
[Route("api/[controller]")]
[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
public class ProductoController : ControllerBase
{
    private readonly ProductoService _productoService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ProductoController(ProductoService productoService, IWebHostEnvironment webHostEnvironment)
    {
        _productoService = productoService;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// Obtiene todos los productos registrados en el sistema
    /// </summary>
    /// <returns>Lista de todos los productos</returns>
    /// <response code="200">Retorna la lista de productos</response>
    /// <response code="404">No se encontraron productos.</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("GetAll")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductoDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductoDTO>>> GetProductos()
    {
        var productos = await _productoService.GetAllProductosAsync();
        return Ok(productos);
    }

    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    /// <param name="id">ID del producto a buscar</param>
    /// <returns>Datos del producto solicitado</returns>
    /// <response code="200">Devuelve el producto solicitado</response>
    /// <response code="404">Si el producto no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductoDTO))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductoDTO>> GetProducto(int id)
    {
        var producto = await _productoService.GetProductoByIdAsync(id);
        if (producto == null)
        {
            return NotFound();
        }

        return Ok(producto);
    }

    /// <summary>
    /// Obtiene todos los productos de una categoría específica
    /// </summary>
    /// <param name="id">ID de la categoría</param>
    /// <returns>Lista de productos de la categoría</returns>
    /// <response code="200">Retorna la lista de productos de la categoría</response>
    /// <response code="404">Si la categoría no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpGet("categoria/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductoDTO>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ProductoDTO>>> GetProductosByCategoria(int id)
    {
        var productos = await _productoService.GetProductosByCategoriaIdAsync(id);
        return Ok(productos);
    }

    /// <summary>
    /// Crea un nuevo producto en el sistema
    /// </summary>
    /// <param name="productoDto">Datos del producto a crear</param>
    /// <param name="Imagen">Imagen del producto</param>
    /// <returns>Datos del producto creado</returns>
    /// <response code="201">Retorna el nuevo producto creado</response>
    /// <response code="400">Si los datos suministrados son inválidos</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("Create")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductoDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ProductoDTO>> CreateProducto([FromForm] CreateProductoDTO productoDto, IFormFile? Imagen)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Manejar la imagen si existe
        if (Imagen != null && Imagen.Length > 0)
        {
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");
            
            // Crear directorio si no existe
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
            
            // Generar un nombre único para la imagen
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Imagen.FileName);
            var filePath = Path.Combine(uploadPath, fileName);
            
            // Guardar la imagen
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Imagen.CopyToAsync(stream);
            }
            
            // Guardar la ruta relativa en el DTO
            productoDto.ImagenUrl = $"/images/productos/{fileName}";
        }
        else
        {
            productoDto.ImagenUrl = "/images/productos/default.png"; // Ruta por defecto
        }

        var producto = await _productoService.CreateProductoAsync(productoDto);
        return CreatedAtAction(nameof(GetProducto), new { id = producto.Id }, producto);
    }

    /// <summary>
    /// Actualiza un producto existente en el sistema
    /// </summary>
    /// <param name="id">ID del producto a actualizar</param>
    /// <param name="updateProductoDTO">Datos del producto a actualizar</param>
    /// <returns>El producto con sus datos actualizados</returns>
    /// <response code="204">Actualización exitosa</response>
    /// <response code="400">Si los datos suministrados son inválidos</response>
    /// <response code="404">Si el producto no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPut("Edit/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProducto(int id, [FromBody] UpdateProductoDTO updateProductoDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _productoService.UpdateProductoAsync(id, updateProductoDTO);
        if (result == null)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Actualiza un producto existente en el sistema con una nueva imagen
    /// </summary>
    /// <param name="id">ID del producto a actualizar</param>
    /// <param name="updateProductoDTO">Datos del producto a actualizar</param>
    /// <param name="Imagen">Nueva imagen del producto</param>
    /// <returns>El producto con sus datos actualizados</returns>
    /// <response code="204">Actualización exitosa</response>
    /// <response code="400">Si los datos suministrados son inválidos</response>
    /// <response code="404">Si el producto no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPost("EditImage/{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateProductoWithImage(int id, [FromForm] UpdateProductoDTO updateProductoDTO, IFormFile Imagen)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
    
        var productoExistente = await _productoService.GetProductoByIdAsync(id);
        if (productoExistente == null)
        {
            return NotFound();
        }
    
        // Process the image if one is provided
        if (Imagen != null && Imagen.Length > 0)
        {
            // If replacing an existing image, delete the old image file
            if (!string.IsNullOrEmpty(productoExistente.ImagenUrl))
            {
                var rutaImagenAntigua = Path.Combine(_webHostEnvironment.WebRootPath, productoExistente.ImagenUrl.TrimStart('/'));
                if (System.IO.File.Exists(rutaImagenAntigua))
                {
                    System.IO.File.Delete(rutaImagenAntigua);
                }
            }
    
            var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "productos");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);
    
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Imagen.FileName);
            var filePath = Path.Combine(uploadPath, fileName);
    
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await Imagen.CopyToAsync(stream);
            }
    
            updateProductoDTO.ImagenUrl = $"/images/productos/{fileName}";
        }
        else
        {
            // If no new image provided, retain the existing one
            updateProductoDTO.ImagenUrl = productoExistente.ImagenUrl;
        }
    
        var result = await _productoService.UpdateProductoAsync(id, updateProductoDTO);
        if (result == null)
        {
            return NotFound();
        }
        return NoContent();
    }

    /// <summary>
    /// Elimina un producto del sistema
    /// </summary>
    /// <param name="id">ID del producto a eliminar</param>
    /// <returns>Sin contenido si se eliminó correctamente</returns>
    /// <response code="204">Si el producto se eliminó correctamente</response>
    /// <response code="404">Si el producto no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpDelete("Delete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteProducto(int id)
    {
        var result = await _productoService.DeleteProductoAsync(id);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Actualiza el stock de un producto
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <param name="cantidad">Cantidad a actualizar</param>
    /// <returns>Sin contenido si se actualizó correctamente</returns>
    /// <response code="204">Si la actualización fue exitosa</response>
    /// <response code="404">Si el producto no existe</response>
    /// <response code="500">Error interno del servidor</response>
    [HttpPatch("{id}/stock/{cantidad}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateStock(int id, int cantidad)
    {
        var result = await _productoService.UpdateStockAsync(id, cantidad);
        if (!result)
        {
            return NotFound();
        }
        return NoContent();
    }
}