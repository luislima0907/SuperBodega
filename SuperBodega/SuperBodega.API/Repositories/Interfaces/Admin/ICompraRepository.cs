using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Repositories.Interfaces.Admin;

public interface ICompraRepository : IGenericOperationsRepository<Compra>
{
    Task<Compra> GetWithDetailsAsync(int id);
    Task<IEnumerable<Compra>> GetAllWithDetailsAsync();
    Task<IEnumerable<Compra>> GetByProveedorIdAsync(int proveedorId);
}