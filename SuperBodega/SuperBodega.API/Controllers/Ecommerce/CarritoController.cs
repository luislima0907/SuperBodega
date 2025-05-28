using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;
using SuperBodega.API.Services.Ecommerce;
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Ecommerce;

namespace SuperBodega.API.Controllers.Ecommerce
{
    /// <summary>
    /// Controlador para gestionar el carrito de compras.
    /// Proporciona métodos para agregar, actualizar y eliminar productos del carrito,
    /// así como para obtener el contenido del carrito y su conteo.
    /// </summary>
    [Route("api/ecommerce/[controller]")]
    [ApiController]
    [Produces("application/json")]
    [ApiConventionType(typeof(DefaultApiConventions))]
    public class CarritoController : ControllerBase
    {
        private readonly CarritoService _carritoService;
        private readonly ClienteService _clienteService;
        
        public CarritoController(CarritoService carritoService, ClienteService clienteService)
        {
            _carritoService = carritoService;
            _clienteService = clienteService;
        }
        
        /// <summary>
        /// Obtiene el contenido del carrito de compras para un cliente específico.
        /// </summary>
        /// <param name="clienteId">ID del cliente.</param>
        /// <returns>Contenido del carrito.</returns>
        /// <response code="200">Retorna el carrito del cliente.</response>
        /// <response code="404">Si el cliente no existe.</response>
        /// <response code="500">Error interno del servidor.</response>
        // GET: api/ecommerce/Carrito/5
        [HttpGet("{clienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Carrito))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCart(int clienteId)
        {
            try
            {
                var cliente = await _clienteService.GetClienteByIdAsync(clienteId);
                if (cliente == null)
                {
                    return NotFound("El cliente no existe.");
                }
                
                var elementos = await _carritoService.GetCartItemsAsync(clienteId);
                
                return Ok(new { 
                    cliente, 
                    elementos = elementos.Select(e => new {
                        id = e.Id,
                        productoId = e.ProductoId,
                        productoNombre = e.Producto.Nombre,
                        productoCategoriaId = e.Producto.CategoriaId,
                        productoCategoriaNombre = e.Producto.Categoria?.Nombre ?? "Sin categoría",
                        precioUnitario = e.PrecioUnitario,
                        cantidad = e.Cantidad,
                        subtotal = e.Subtotal,
                        imagenUrl = e.Producto.ImagenUrl ?? "/images/productos/default.png",
                        productoStock = e.Producto.Stock
                    }),
                    itemCount = elementos.Count,
                    total = elementos.Sum(e => e.Subtotal)
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener el carrito: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Obtiene el conteo de elementos en el carrito de compras para un cliente específico.
        /// </summary>
        /// <param name="clienteId">ID del cliente.</param>
        /// <returns>Conteo de elementos en el carrito.</returns>
        /// <response code="200">Retorna el conteo de elementos.</response>
        /// <response code="404">Si el cliente no existe.</response>
        /// <response code="500">Error interno del servidor.</response>
        // GET: api/ecommerce/Carrito/count/5
        [HttpGet("count/{clienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCartCount(int clienteId)
        {
            try
            {
                var count = await _carritoService.GetCartItemCountAsync(clienteId);
                return Ok(new { count });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener el conteo de elementos: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Agrega un producto al carrito de compras.
        /// </summary>
        /// <param name="model">Modelo que contiene los datos del producto a agregar.</param>
        /// <returns>Resultado de la operación con ese modelo.</returns>
        /// <response code="200">Retorna el elemento agregado.</response>
        /// <response code="400">Si los datos suministrados son inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        // POST: api/ecommerce/Carrito
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Carrito))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var elemento = await _carritoService.AddToCartAsync(
                    model.ClienteId, 
                    model.ProductoId, 
                    model.Cantidad);
                
                return Ok(new { message = "Producto agregado al carrito.", elementoId = elemento.Id });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al agregar al carrito: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Actualiza la cantidad de un producto en el carrito de compras.
        /// </summary>
        /// <param name="elementoId">ID del elemento en el carrito.</param>
        /// <param name="model">Modelo que contiene la nueva cantidad.</param>
        /// <returns>Resultado de la operación con el modelo.</returns>
        /// <response code="200">Retorna la cantidad actualizada.</response>
        /// <response code="400">Si los datos suministrados son inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        // PUT: api/ecommerce/Carrito/Edit/5
        [HttpPut("Edit/{elementoId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Carrito))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCartItem(int elementoId, [FromBody] UpdateCartItemModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            try
            {
                var elemento = await _carritoService.UpdateCartItemAsync(elementoId, model.Cantidad);
                
                return Ok(new { 
                    message = "Cantidad actualizada.", 
                    cantidad = elemento.Cantidad,
                    subtotal = elemento.Subtotal
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al actualizar el elemento: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Elimina un producto del carrito de compras.
        /// </summary>
        /// <param name="elementoId">ID del elemento en el carrito.</param>
        /// <returns>Resultado de la operación de eliminar.</returns>
        /// <response code="200">Retorna un mensaje de éxito.</response>
        /// <response code="400">Si los datos suministrados son inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        // DELETE: api/ecommerce/Carrito/Delete/5
        [HttpDelete("Delete/{elementoId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveFromCart(int elementoId)
        {
            try
            {
                await _carritoService.RemoveFromCartAsync(elementoId);
                
                return Ok(new { message = "Producto eliminado del carrito." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al eliminar del carrito: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Limpia el carrito de compras para un cliente específico.
        /// </summary>
        /// <param name="clienteId">ID del cliente.</param>
        /// <returns>Resultado de la operación de limpiar.</returns>
        /// <response code="200">Retorna un mensaje de éxito.</response>
        /// <response code="400">Si los datos suministrados son inválidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        // DELETE: api/ecommerce/Carrito/clear/client/{clienteId}
        [HttpDelete("clear/client/{clienteId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ClearCart(int clienteId)
        {
            try
            {
                await _carritoService.ClearCartByClientIdAsync(clienteId);

                return Ok(new { message = "Carrito limpiado." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al limpiar el carrito: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Obtiene todos los clientes activos.
        /// </summary>
        /// <returns>Lista de clientes activos.</returns>
        /// <response code="200">Retorna la lista de clientes activos.</response>
        /// <response code="404">No se encontraron clientes.</response>
        /// <response code="500">Error interno del servidor.</response>
        // GET: api/ecommerce/Carrito/clientes
        [HttpGet("clientes")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClienteDTO>))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetClientes()
        {
            try
            {
                var clientes = await _clienteService.GetAllClientesAsync();
                var clientesActivos = clientes.Where(c => c.Estado).ToList();
                
                return Ok(clientesActivos);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al obtener los clientes: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Modelo para agregar un producto al carrito de compras.
    /// </summary>
    public class AddToCartModel
    {
        /// <summary>
        /// ID del cliente.
        /// </summary>
        public int ClienteId { get; set; }
        
        /// <summary>
        /// ID del producto a agregar.
        /// </summary>
        public int ProductoId { get; set; }
        
        /// <summary>
        /// Cantidad del producto a agregar.
        /// </summary>
        public int Cantidad { get; set; }
    }
    
    /// <summary>
    /// Modelo para actualizar la cantidad de un producto en el carrito de compras.
    /// </summary>
    public class UpdateCartItemModel
    {
        /// <summary>
        /// Cantidad del producto a actualizar.
        /// </summary>
        public int Cantidad { get; set; }
    }
}