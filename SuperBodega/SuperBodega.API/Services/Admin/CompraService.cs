using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;
using SuperBodega.API.Repositories.Interfaces.Admin;

namespace SuperBodega.API.Services.Admin;

public class CompraService
{
    private readonly ICompraRepository _compraRepository;
    private readonly IProductoRepository _productoRepository;
    
    public CompraService(ICompraRepository compraRepository, IProductoRepository productoRepository)
    {
        _compraRepository = compraRepository;
        _productoRepository = productoRepository;
    }

    public async Task<IEnumerable<CompraDTO>> GetAllAsync()
    {
        var compra = await _compraRepository.GetAllAsync();
        return compra.Select(c => new CompraDTO
        {
            Id = c.Id,
            NumeroDeFactura = c.NumeroDeFactura,
            IdProveedor = c.IdProveedor,
            NombreDelProveedor = c.Proveedor?.Nombre ?? "Proveedor no encontrado",
            FechaDeRegistro = c.FechaDeRegistro,
            MontoTotal = c.MontoTotal,
            DetallesDeLaCompra = c.DetallesDeLaCompra.Select(d => new DetalleDeLaCompraDTO
            {
                Id = d.Id,
                IdCompra = d.IdCompra,
                IdProducto = d.IdProducto,
                NombreDelProducto = d.Producto?.Nombre ?? "Producto no encontrado",
                CodigoDelProducto = d.Producto?.Codigo ?? "N/A",
                ImagenDelProducto = d.Producto?.ImagenUrl ?? "/images/productos/default.png",
                CategoriaDelProducto = d.Producto?.Categoria?.Nombre ?? "Categoría no encontrada",
                PrecioDeCompra = d.PrecioDeCompra,
                PrecioDeVenta = d.PrecioDeVenta,
                Cantidad = d.Cantidad,
                Montototal = d.Montototal,
                FechaDeRegistro = d.FechaDeRegistro
            }).ToList() ?? new List<DetalleDeLaCompraDTO>()
        });
    }

    public async Task<IEnumerable<CompraDTO>> GetAllWithDetailsAsync()
    {
        var compras = await _compraRepository.GetAllWithDetailsAsync();
        return compras.Select(c => new CompraDTO
        {
            Id = c.Id,
            NumeroDeFactura = c.NumeroDeFactura,
            IdProveedor = c.IdProveedor,
            NombreDelProveedor = c.Proveedor?.Nombre ?? "Proveedor no encontrado",
            FechaDeRegistro = c.FechaDeRegistro,
            MontoTotal = c.MontoTotal,
            DetallesDeLaCompra = c.DetallesDeLaCompra.Select(d => new DetalleDeLaCompraDTO
            {
                Id = d.Id,
                IdCompra = d.IdCompra,
                IdProducto = d.IdProducto,
                NombreDelProducto = d.Producto?.Nombre ?? "Producto no encontrado",
                CodigoDelProducto = d.Producto?.Codigo ?? "N/A",
                ImagenDelProducto = d.Producto?.ImagenUrl ?? "/images/productos/default.png",
                CategoriaDelProducto = d.Producto?.Categoria?.Nombre ?? "Categoría no encontrada",
                PrecioDeCompra = d.PrecioDeCompra,
                PrecioDeVenta = d.PrecioDeVenta,
                Cantidad = d.Cantidad,
                Montototal = d.Montototal,
                FechaDeRegistro = d.FechaDeRegistro
            }).ToList()
        });
    }
    
    public async Task<CompraDTO> GetByIdAsync(int id)
    {
        var compra = await _compraRepository.GetByIdAsync(id);
        if (compra == null) return null;
    
        // Asegurar que se cargue la información completa incluyendo el proveedor
        if (compra.Proveedor == null)
        {
            // Si el proveedor no se cargó adecuadamente, intentar obtener la compra con detalles completos
            compra = await _compraRepository.GetWithDetailsAsync(id);
            
            // Si aún así es null, usar un valor predeterminado
            if (compra == null) return null;
        }
    
        return new CompraDTO
        {
            Id = compra.Id,
            NumeroDeFactura = compra.NumeroDeFactura,
            IdProveedor = compra.IdProveedor,
            NombreDelProveedor = compra.Proveedor?.Nombre ?? "Proveedor no encontrado",
            FechaDeRegistro = compra.FechaDeRegistro,
            MontoTotal = compra.MontoTotal,
            DetallesDeLaCompra = compra.DetallesDeLaCompra?.Select(d => new DetalleDeLaCompraDTO
            {
                Id = d.Id,
                IdCompra = d.IdCompra,
                IdProducto = d.IdProducto,
                NombreDelProducto = d.Producto?.Nombre ?? "Producto no encontrado",
                CodigoDelProducto = d.Producto?.Codigo ?? "N/A",
                ImagenDelProducto = d.Producto?.ImagenUrl ?? "/images/productos/default.png",
                CategoriaDelProducto = d.Producto?.Categoria?.Nombre ?? "Categoría no encontrada",
                PrecioDeCompra = d.PrecioDeCompra,
                PrecioDeVenta = d.PrecioDeVenta,
                Cantidad = d.Cantidad,
                Montototal = d.Montototal,
                FechaDeRegistro = d.FechaDeRegistro
            }).ToList() ?? new List<DetalleDeLaCompraDTO>()
        };
    }

    public async Task<CompraDTO> GetWithDetailsAsync(int id)
    {
        var compra = await _compraRepository.GetWithDetailsAsync(id);
        if (compra == null) return null;
        return new CompraDTO
        {
            Id = compra.Id,
            NumeroDeFactura = compra.NumeroDeFactura,
            IdProveedor = compra.IdProveedor,
            NombreDelProveedor = compra.Proveedor.Nombre,
            FechaDeRegistro = compra.FechaDeRegistro,
            MontoTotal = compra.MontoTotal,
            DetallesDeLaCompra = compra.DetallesDeLaCompra.Select(d => new DetalleDeLaCompraDTO
            {
                Id = d.Id,
                IdCompra = d.IdCompra,
                IdProducto = d.IdProducto,
                NombreDelProducto = d.Producto?.Nombre ?? "Producto no encontrado",
                CodigoDelProducto = d.Producto?.Codigo ?? "N/A",
                ImagenDelProducto = d.Producto?.ImagenUrl ?? "/images/productos/default.png",
                CategoriaDelProducto = d.Producto?.Categoria?.Nombre ?? "Categoría no encontrada",
                PrecioDeCompra = d.PrecioDeCompra,
                PrecioDeVenta = d.PrecioDeVenta,
                Cantidad = d.Cantidad,
                Montototal = d.Montototal,
                FechaDeRegistro = d.FechaDeRegistro
            }).ToList()
        };
    }

    public async Task<CompraDTO> CreateAsync(CreateCompraDTO createCompraDto)
    {
        var compra = new Compra
        {
            NumeroDeFactura = createCompraDto.NumeroDeFactura,
            IdProveedor = createCompraDto.IdProveedor,
            FechaDeRegistro = createCompraDto.FechaDeRegistro,
            MontoTotal = 0 // Inicialmente 0, se actualizará después
        };
        
        // procesar cada detalle de la compra
        foreach (var detalleDTO in createCompraDto.DetallesDeLaCompra)
        {
            var producto = await _productoRepository.GetByIdAsync(detalleDTO.IdProducto);
            if (producto == null)
            {
                throw new Exception($"El producto con ID {detalleDTO.IdProducto} no existe.");
            }
            
            var detalle = new DetalleDeLaCompra
            {
                IdProducto = detalleDTO.IdProducto,
                PrecioDeCompra = detalleDTO.PrecioDeCompra,
                PrecioDeVenta = detalleDTO.PrecioDeVenta,
                Cantidad = detalleDTO.Cantidad,
                Montototal = detalleDTO.PrecioDeCompra * detalleDTO.Cantidad,
                FechaDeRegistro = detalleDTO.FechaDeRegistro
            };
            
            // Actualizar el stock y los precios del producto
            producto.Stock += detalle.Cantidad;
            producto.PrecioDeCompra = detalle.PrecioDeCompra;
            producto.PrecioDeVenta = detalle.PrecioDeVenta;
            await _productoRepository.UpdateAsync(producto);
            // Agregar el detalle a la compra
            compra.DetallesDeLaCompra.Add(detalle);
            compra.MontoTotal += detalle.Montototal;
        }
        
        // Guardar la compra con todos sus detalles
        var createdCompra = await _compraRepository.AddAsync(compra);
        return await GetWithDetailsAsync(createdCompra.Id);
    }
    
    public async Task<CompraDTO> UpdateAsync(int id, UpdateCompraDTO updateCompraDTO)
    {
        var compra = await _compraRepository.GetWithDetailsAsync(id);
        if (compra == null)
            throw new Exception($"Compra con ID {id} no encontrada");
    
        // Actualizar datos básicos de la compra
        compra.NumeroDeFactura = updateCompraDTO.NumeroDeFactura;
        compra.IdProveedor = updateCompraDTO.IdProveedor;
    
        // Manejar detalles de la compra si están incluidos
        if (updateCompraDTO.DetallesDeLaCompra != null && updateCompraDTO.DetallesDeLaCompra.Count > 0)
        {
            var detallesExistentesIds = compra.DetallesDeLaCompra.Select(d => d.Id).ToList();
            var detallesNuevosIds = updateCompraDTO.DetallesDeLaCompra
                .Where(d => d.Id > 0)
                .Select(d => d.Id)
                .ToList();
    
            // Identificar detalles a eliminar (los que están en la BD pero no vienen en el DTO)
            var detallesAEliminar = compra.DetallesDeLaCompra
                .Where(d => !detallesNuevosIds.Contains(d.Id))
                .ToList();
    
            // Revertir el stock para los detalles que se eliminarán
            foreach (var detalle in detallesAEliminar)
            {
                var producto = await _productoRepository.GetByIdAsync(detalle.IdProducto);
                if (producto != null)
                {
                    producto.Stock -= detalle.Cantidad;
                    await _productoRepository.UpdateAsync(producto);
                }
                
                // Eliminar el detalle
                compra.DetallesDeLaCompra.Remove(detalle);
            }
    
            // Actualizar detalles existentes y agregar nuevos
            foreach (var detalleDTO in updateCompraDTO.DetallesDeLaCompra)
            {
                if (detalleDTO.Id > 0)
                {
                    // Actualizar detalle existente
                    var detalleExistente = compra.DetallesDeLaCompra.FirstOrDefault(d => d.Id == detalleDTO.Id);
                    if (detalleExistente != null)
                    {
                        // Ajustar stock del producto
                        var producto = await _productoRepository.GetByIdAsync(detalleExistente.IdProducto);
                        if (producto != null)
                        {
                            // Revertir cantidad anterior y agregar nueva cantidad
                            producto.Stock = producto.Stock - detalleExistente.Cantidad + detalleDTO.Cantidad;
                            producto.PrecioDeCompra = detalleDTO.PrecioDeCompra;
                            producto.PrecioDeVenta = detalleDTO.PrecioDeVenta;
                            await _productoRepository.UpdateAsync(producto);
                        }
                        
                        // Actualizar el detalle
                        detalleExistente.PrecioDeCompra = detalleDTO.PrecioDeCompra;
                        detalleExistente.PrecioDeVenta = detalleDTO.PrecioDeVenta;
                        detalleExistente.Cantidad = detalleDTO.Cantidad;
                        detalleExistente.Montototal = detalleDTO.PrecioDeCompra * detalleDTO.Cantidad;
                    }
                }
                else
                {
                    // Agregar nuevo detalle
                    var producto = await _productoRepository.GetByIdAsync(detalleDTO.IdProducto);
                    if (producto == null)
                        throw new Exception($"El producto con ID {detalleDTO.IdProducto} no existe.");
                    
                    var nuevoDetalle = new DetalleDeLaCompra
                    {
                        IdCompra = id,
                        IdProducto = detalleDTO.IdProducto,
                        PrecioDeCompra = detalleDTO.PrecioDeCompra,
                        PrecioDeVenta = detalleDTO.PrecioDeVenta,
                        Cantidad = detalleDTO.Cantidad,
                        Montototal = detalleDTO.PrecioDeCompra * detalleDTO.Cantidad,
                        FechaDeRegistro = DateTime.UtcNow
                    };
                    
                    // Actualizar stock del producto
                    producto.Stock += nuevoDetalle.Cantidad;
                    producto.PrecioDeCompra = detalleDTO.PrecioDeCompra;
                    producto.PrecioDeVenta = detalleDTO.PrecioDeVenta;
                    await _productoRepository.UpdateAsync(producto);
                    
                    // Agregar el detalle a la compra
                    compra.DetallesDeLaCompra.Add(nuevoDetalle);
                }
            }
    
            // Recalcular monto total de la compra
            compra.MontoTotal = compra.DetallesDeLaCompra.Sum(d => d.Montototal);
        }
    
        await _compraRepository.UpdateAsync(compra);
        return await GetWithDetailsAsync(id);
    }
    
    public async Task<bool> DeleteAsync(int id)
    {
        var compra = await _compraRepository.GetWithDetailsAsync(id);
        if (compra == null) return false;

        // Eliminar todos los detalles de la compra
        foreach (var detalle in compra.DetallesDeLaCompra)
        {
            var producto = await _productoRepository.GetByIdAsync(detalle.IdProducto);
            if (producto != null)
            {
                producto.Stock -= detalle.Cantidad;
                await _productoRepository.UpdateAsync(producto);
            }
        }

        return await _compraRepository.DeleteAsync(id);
    }
    
    public async Task<IEnumerable<CompraDTO>> GetByProveedorIdAsync(int proveedorId)
    {
        var compras = await _compraRepository.GetByProveedorIdAsync(proveedorId);
        return compras.Select(c => new CompraDTO
        {
            Id = c.Id,
            NumeroDeFactura = c.NumeroDeFactura,
            IdProveedor = c.IdProveedor,
            NombreDelProveedor = c.Proveedor.Nombre,
            FechaDeRegistro = c.FechaDeRegistro,
            MontoTotal = c.MontoTotal
        });
    }
}