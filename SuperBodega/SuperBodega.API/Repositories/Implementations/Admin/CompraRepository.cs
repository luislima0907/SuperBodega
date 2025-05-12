using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Repositories.Implementations.Admin;

public class CompraRepository : ICompraRepository
{
    private readonly SuperBodegaContext _context;
    
    public CompraRepository(SuperBodegaContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Compra>> GetAllAsync()
    {
        return await _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.DetallesDeLaCompra)
                .ThenInclude(d => d.Producto)
            .ToListAsync();
    }
    
    public async Task<Compra> GetByIdAsync(int id)
    {
        // Incluir el proveedor y los detalles
        return await _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.DetallesDeLaCompra)
            .ThenInclude(d => d.Producto)
            .ThenInclude(p => p.Categoria)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<Compra> AddAsync(Compra entity)
    {
        await _context.Compras.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    
    public async Task<Compra> UpdateAsync(Compra entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return entity;
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var compra = await _context.Compras.FindAsync(id);
        if (compra == null) return false;
        
        _context.Compras.Remove(compra);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Compra> GetWithDetailsAsync(int id)
    {
        return await _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.DetallesDeLaCompra)
                .ThenInclude(d => d.Producto)
                .ThenInclude(p => p.Categoria)
            .FirstOrDefaultAsync(c => c.Id == id);
    }
    
    public async Task<IEnumerable<Compra>> GetAllWithDetailsAsync()
    {
        return await _context.Compras
            .Include(c => c.Proveedor)
            .Include(c => c.DetallesDeLaCompra)
                .ThenInclude(d => d.Producto)
                .ThenInclude(p => p.Categoria)
            .ToListAsync();
    }

    public async Task<IEnumerable<Compra>> GetByProveedorIdAsync(int proveedorId)
    {
        return await _context.Compras
            .Where(c => c.IdProveedor == proveedorId)
            .Include(c => c.DetallesDeLaCompra)
            .ThenInclude(d => d.Producto)
            .ToListAsync();
    }
    
}