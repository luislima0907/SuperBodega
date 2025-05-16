using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperBodega.API.Repositories.Implementations.Admin
{
    public class DetalleDeLaVentaRepository : IDetalleDeLaVentaRepository
    {
        private readonly SuperBodegaContext _context;

        public DetalleDeLaVentaRepository(SuperBodegaContext context)
        {
            _context = context;
        }

        public async Task<DetalleDeLaVenta> AddAsync(DetalleDeLaVenta entity)
        {
            _context.DetallesDeLaVenta.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var detalle = await _context.DetallesDeLaVenta.FindAsync(id);
            if (detalle == null) return false;
            
            _context.DetallesDeLaVenta.Remove(detalle);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<DetalleDeLaVenta>> GetAllAsync()
        {
            return await _context.DetallesDeLaVenta
                .Include(d => d.Producto)
                .ToListAsync();
        }

        public async Task<DetalleDeLaVenta> GetByIdAsync(int id)
        {
            return await _context.DetallesDeLaVenta
                .Include(d => d.Producto)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<DetalleDeLaVenta> UpdateAsync(DetalleDeLaVenta entity)
        {
            _context.DetallesDeLaVenta.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> ProductoTieneVentasActivas(int productoId)
        {
            return await _context.DetallesDeLaVenta
                .Include(d => d.Venta)
                .AnyAsync(d => d.IdProducto == productoId && 
                              (d.Venta.IdEstadoDeLaVenta == 1 || // Recibida
                               d.Venta.IdEstadoDeLaVenta == 2)); // Despachada
        }
        
        public async Task<bool> CategoriaTieneVentasActivas(int categoriaId)
        {
            return await _context.DetallesDeLaVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .AnyAsync(d => d.Producto.CategoriaId == categoriaId && 
                              (d.Venta.IdEstadoDeLaVenta == 1 || // Recibida
                               d.Venta.IdEstadoDeLaVenta == 2)); // Despachada
        }
    }
}