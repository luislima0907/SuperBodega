using SuperBodega.API.Models.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SuperBodega.API.Repositories.Interfaces.Admin
{
    public interface IVentaRepository : IGenericOperationsRepository<Venta>
    {
        Task<Venta> GetWithDetailsAsync(int id);
        Task<IEnumerable<Venta>> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<Venta>> GetByEstadoIdAsync(int estadoId);
        Task<IEnumerable<Venta>> GetAllWithDetailsAsync();
        Task<bool> ClienteTieneVentasActivas(int clienteId);
        Task<bool> ProveedorTieneVentasActivas(int proveedorId);
    }
}