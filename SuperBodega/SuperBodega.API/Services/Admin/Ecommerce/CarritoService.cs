using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Models.Ecommerce;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperBodega.API.Services.Ecommerce
{
    public class CarritoService
    {
        private readonly SuperBodegaContext _context;

        public CarritoService(SuperBodegaContext context)
        {
            _context = context;
        }

        // Obtener un carrito activo por cliente o crear uno nuevo si no existe
        public async Task<Carrito> GetOrCreateCarritoAsync(int clienteId)
        {
            var carrito = await _context.Carritos
                .Include(c => c.Elementos)
                .ThenInclude(e => e.Producto)
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaCreacion)
                .FirstOrDefaultAsync();

            if (carrito == null)
            {
                carrito = new Carrito
                {
                    ClienteId = clienteId,
                    FechaCreacion = DateTime.Now
                };

                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            return carrito;
        }

        // Agregar un producto al carrito
        public async Task<ElementoCarrito> AddToCartAsync(int clienteId, int productoId, int cantidad)
        {
            // Validar que el producto exista y tenga stock suficiente
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null)
            {
                throw new Exception("El producto no existe.");
            }

            if (producto.Stock < cantidad)
            {
                throw new Exception("No hay suficiente stock disponible.");
            }

            // Obtener el carrito del cliente
            var carrito = await GetOrCreateCarritoAsync(clienteId);

            // Verificar si el producto ya existe en el carrito
            var elementoCarrito = carrito.Elementos
                .FirstOrDefault(e => e.ProductoId == productoId);

            if (elementoCarrito != null)
            {
                // El producto ya está en el carrito, actualizar la cantidad
                elementoCarrito.Cantidad += cantidad;
            }
            else
            {
                // El producto no está en el carrito, crear nuevo elemento
                elementoCarrito = new ElementoCarrito
                {
                    CarritoId = carrito.Id,
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    PrecioUnitario = producto.PrecioDeVenta
                };

                _context.ElementosCarrito.Add(elementoCarrito);
            }

            await _context.SaveChangesAsync();
            return elementoCarrito;
        }

        // Actualizar la cantidad de un producto en el carrito
        public async Task<ElementoCarrito> UpdateCartItemAsync(int elementoId, int cantidad)
        {
            var elemento = await _context.ElementosCarrito
                .Include(e => e.Producto)
                .FirstOrDefaultAsync(e => e.Id == elementoId);

            if (elemento == null)
            {
                throw new Exception("El elemento no existe en el carrito.");
            }

            if (elemento.Producto.Stock < cantidad)
            {
                throw new Exception("No hay suficiente stock disponible.");
            }

            elemento.Cantidad = cantidad;
            await _context.SaveChangesAsync();
            return elemento;
        }

        // Eliminar un producto del carrito
        public async Task RemoveFromCartAsync(int elementoId)
        {
            var elemento = await _context.ElementosCarrito.FindAsync(elementoId);
            if (elemento != null)
            {
                _context.ElementosCarrito.Remove(elemento);
                await _context.SaveChangesAsync();
            }
        }

        // Limpiar el carrito (eliminar todos los elementos)
        public async Task ClearCartAsync(int carritoId)
        {
            var elementos = await _context.ElementosCarrito
                .Where(e => e.CarritoId == carritoId)
                .ToListAsync();

            _context.ElementosCarrito.RemoveRange(elementos);
            await _context.SaveChangesAsync();
        }

        // Limpiar el carrito por ID de Cliente
        public async Task ClearCartByClientIdAsync(int clienteId)
        {
            // Encuentra el carrito más reciente del cliente
            var carrito = await _context.Carritos
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaCreacion)
                .FirstOrDefaultAsync();

            if (carrito != null)
            {
                // Llama al método existente que usa carritoId
                await ClearCartAsync(carrito.Id);
            }
            // Si no hay carrito, no hay nada que limpiar, no es un error.
        }

        // Obtener el número de elementos en el carrito
        public async Task<int> GetCartItemCountAsync(int clienteId)
        {
            var carrito = await _context.Carritos
                .Include(c => c.Elementos)
                .Where(c => c.ClienteId == clienteId)
                .OrderByDescending(c => c.FechaCreacion)
                .FirstOrDefaultAsync();

            if (carrito == null)
            {
                return 0;
            }

            return carrito.Elementos.Count;
        }

        // Obtener todos los elementos del carrito
        public async Task<List<ElementoCarrito>> GetCartItemsAsync(int clienteId)
        {
            var carrito = await GetOrCreateCarritoAsync(clienteId);

            // Recargar los elementos con información completa del producto
            await _context.Entry(carrito)
                .Collection(c => c.Elementos)
                .Query()
                .Include(e => e.Producto)
                .ThenInclude(p => p.Categoria)
                .LoadAsync();

            return carrito.Elementos.ToList();
        }
    }
}