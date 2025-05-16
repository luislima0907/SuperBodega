using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SuperBodega.API.Services.Admin
{
    public class EstadoDeLaVentaService
    {
        private readonly IEstadoDeLaVentaRepository _estadoDeLaVentaRepository;

        public EstadoDeLaVentaService(IEstadoDeLaVentaRepository estadoDeLaVentaRepository)
        {
            _estadoDeLaVentaRepository = estadoDeLaVentaRepository;
        }

        public async Task<IEnumerable<EstadoDeLaVentaDTO>> GetAllEstadosDeLaVentaAsync()
        {
            var estados = await _estadoDeLaVentaRepository.GetAllAsync();
            return estados.Select(e => new EstadoDeLaVentaDTO
            {
                Id = e.Id,
                Nombre = e.Nombre
            });
        }

        public async Task<EstadoDeLaVentaDTO> GetEstadoDeLaVentaByIdAsync(int id)
        {
            var estado = await _estadoDeLaVentaRepository.GetByIdAsync(id);
            if (estado == null) return null;

            return new EstadoDeLaVentaDTO
            {
                Id = estado.Id,
                Nombre = estado.Nombre
            };
        }

        public async Task<EstadoDeLaVentaDTO> CreateEstadoDeLaVentaAsync(EstadoDeLaVentaDTO estadoDTO)
        {
            var estado = new EstadoDeLaVenta
            {
                Nombre = estadoDTO.Nombre
            };

            var createdEstado = await _estadoDeLaVentaRepository.AddAsync(estado);
            return new EstadoDeLaVentaDTO
            {
                Id = createdEstado.Id,
                Nombre = createdEstado.Nombre
            };
        }

        public async Task<EstadoDeLaVentaDTO> UpdateEstadoDeLaVentaAsync(int id, EstadoDeLaVentaDTO estadoDTO)
        {
            var estado = await _estadoDeLaVentaRepository.GetByIdAsync(id);
            if (estado == null) return null;

            estado.Nombre = estadoDTO.Nombre;

            var updatedEstado = await _estadoDeLaVentaRepository.UpdateAsync(estado);
            return new EstadoDeLaVentaDTO
            {
                Id = updatedEstado.Id,
                Nombre = updatedEstado.Nombre
            };
        }

        public async Task<bool> DeleteEstadoDeLaVentaAsync(int id)
        {
            return await _estadoDeLaVentaRepository.DeleteAsync(id);
        }
    }
}