using Microsoft.EntityFrameworkCore;
using SuperBodega.API.Data;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperBodega.API.Repositories.Implementations.Admin
{
    public class EstadoDeLaVentaRepository : IEstadoDeLaVentaRepository
    {
        private readonly SuperBodegaContext _context;

        public EstadoDeLaVentaRepository(SuperBodegaContext context)
        {
            _context = context;
        }

        public async Task<EstadoDeLaVenta> AddAsync(EstadoDeLaVenta entity)
        {
            _context.EstadosDeLaVenta.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var estado = await _context.EstadosDeLaVenta.FindAsync(id);
            if (estado == null) return false;
            
            _context.EstadosDeLaVenta.Remove(estado);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<EstadoDeLaVenta>> GetAllAsync()
        {
            return await _context.EstadosDeLaVenta.ToListAsync();
        }

        public async Task<EstadoDeLaVenta> GetByIdAsync(int id)
        {
            return await _context.EstadosDeLaVenta.FindAsync(id);
        }

        public async Task<EstadoDeLaVenta> UpdateAsync(EstadoDeLaVenta entity)
        {
            _context.EstadosDeLaVenta.Update(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}