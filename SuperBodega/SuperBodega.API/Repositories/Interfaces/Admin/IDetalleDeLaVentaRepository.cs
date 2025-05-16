using SuperBodega.API.Models.Admin;

namespace SuperBodega.API.Repositories.Interfaces.Admin
{
    public interface IDetalleDeLaVentaRepository : IGenericOperationsRepository<DetalleDeLaVenta>
    {
        Task<bool> ProductoTieneVentasActivas(int productoId);
        Task<bool> CategoriaTieneVentasActivas(int categoriaId);
    }
}