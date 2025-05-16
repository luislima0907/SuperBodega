using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperBodega.API.Repositories.Implementations.Admin
{
    public class VentaRepository : IVentaRepository
    {
        private readonly SuperBodegaContext _context;

        public VentaRepository(SuperBodegaContext context)
        {
            _context = context;
        }

        public async Task<Venta> GetWithDetailsAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Venta>> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .Where(v => v.IdCliente == clienteId)
                .OrderByDescending(v => v.FechaDeRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetByEstadoIdAsync(int estadoId)
        {
            return await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                .Where(v => v.IdEstadoDeLaVenta == estadoId)
                .OrderByDescending(v => v.FechaDeRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetAllWithDetailsAsync()
        {
            return await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.EstadoDeLaVenta)
                .Include(v => v.DetallesDeLaVenta)
                    .ThenInclude(d => d.Producto)
                        .ThenInclude(p => p.Categoria)
                .OrderByDescending(v => v.FechaDeRegistro)
                .ToListAsync();
        }

        public async Task<IEnumerable<Venta>> GetAllAsync()
        {
            return await _context.Ventas.ToListAsync();
        }

        public Task<Venta> GetByIdAsync(int id)
        {
            return _context.Ventas.FindAsync(id).AsTask();
        }

        public Task<Venta> AddAsync(Venta venta)
        {
            _context.Ventas.Add(venta);
            return _context.SaveChangesAsync().ContinueWith(t => venta);
        }

        public Task<Venta> UpdateAsync(Venta venta)
        {
            _context.Ventas.Update(venta);
            return _context.SaveChangesAsync().ContinueWith(t => venta);
        }

        public Task<bool> DeleteAsync(int id)
        {
            var venta = _context.Ventas.Find(id);
            if (venta == null)
            {
                return Task.FromResult(false);
            }

            _context.Ventas.Remove(venta);
            return _context.SaveChangesAsync().ContinueWith(t => true);
        }

        public async Task<bool> ClienteTieneVentasActivas(int clienteId)
        {
            return await _context.Ventas
                .AnyAsync(v => v.IdCliente == clienteId && 
                            (v.IdEstadoDeLaVenta == 1 || // Recibida
                            v.IdEstadoDeLaVenta == 2)); // Despachada
        }

        public async Task<bool> ProveedorTieneVentasActivas(int proveedorId)
        {
            return await _context.DetallesDeLaVenta
                .Where(d => d.IdProveedor == proveedorId)
                .Join(_context.Ventas,
                    detalle => detalle.IdVenta,
                    venta => venta.Id,
                    (detalle, venta) => venta)
                .AnyAsync(v => v.IdEstadoDeLaVenta == 1 || v.IdEstadoDeLaVenta == 2);
        }
    }
}
