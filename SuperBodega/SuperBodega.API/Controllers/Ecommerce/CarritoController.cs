using Microsoft.AspNetCore.Mvc;
using SuperBodega.API.Services.Admin;
using SuperBodega.API.Services.Ecommerce;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperBodega.API.Controllers.Ecommerce
{
    [Route("api/ecommerce/[controller]")]
    [ApiController]
    public class CarritoController : ControllerBase
    {
        private readonly CarritoService _carritoService;
        private readonly ClienteService _clienteService;

        public CarritoController(CarritoService carritoService, ClienteService clienteService)
        {
            _carritoService = carritoService;
            _clienteService = clienteService;
        }

        // GET: api/ecommerce/Carrito/5
        [HttpGet("{clienteId}")]
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

                return Ok(new
                {
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

        // GET: api/ecommerce/Carrito/count/5
        [HttpGet("count/{clienteId}")]
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

        // POST: api/ecommerce/Carrito
        [HttpPost]
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

        // PUT: api/ecommerce/Carrito/Edit/5
        [HttpPut("Edit/{elementoId}")]
        public async Task<IActionResult> UpdateCartItem(int elementoId, [FromBody] UpdateCartItemModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var elemento = await _carritoService.UpdateCartItemAsync(elementoId, model.Cantidad);

                return Ok(new
                {
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

        // DELETE: api/ecommerce/Carrito/Delete/5
        [HttpDelete("Delete/{elementoId}")]
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

        // DELETE: api/ecommerce/Carrito/clear/client/{clienteId}
        [HttpDelete("clear/client/{clienteId}")]
        public async Task<IActionResult> ClearCart(int clienteId)
        {
            try
            {
                // Call the new service method that accepts clienteId
                await _carritoService.ClearCartByClientIdAsync(clienteId);

                return Ok(new { message = "Carrito limpiado." });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al limpiar el carrito: {ex.Message}");
            }
        }

        // GET: api/ecommerce/Carrito/clientes
        [HttpGet("clientes")]
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

    public class AddToCartModel
    {
        public int ClienteId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class UpdateCartItemModel
    {
        public int Cantidad { get; set; }
    }
}