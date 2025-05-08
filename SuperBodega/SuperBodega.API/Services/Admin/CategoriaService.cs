
using SuperBodega.API.DTOs.Admin;
using SuperBodega.API.Models.Admin;

public class CategoriaService
{
    private readonly IGenericOperationsRepository<Categoria> _categoriaRepository;

    public CategoriaService(IGenericOperationsRepository<Categoria> categoriaRepository){
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IEnumerable<CategoriaDTO>> GetAllCategoriasAsync()
    {
        var categorias = await _categoriaRepository.GetAllAsync();
        // Aqui puedes mapear las entidades a DTOs
        return categorias.Select(c => new CategoriaDTO
        {
            Id = c.Id,
            Nombre = c.Nombre,
            Descripcion = c.Descripcion,
            Estado = c.Estado,
            FechaDeRegistro = c.FechaDeRegistro,
        });
    }

    public async Task<CategoriaDTO> GetCategoriaByIdAsync(int id)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id);
        if (categoria == null)
        {
            return null;
        }
        return new CategoriaDTO
        {
            Id = categoria.Id,
            Nombre = categoria.Nombre,
            Descripcion = categoria.Descripcion,
            Estado = categoria.Estado,
            FechaDeRegistro = categoria.FechaDeRegistro,
        };
    }

    public async Task<CategoriaDTO> CreateCategoriaAsync(CreateCategoriaDTO categoriaDTO)
    {
        var categoria = new Categoria
        {
            Nombre = categoriaDTO.Nombre,
            Descripcion = categoriaDTO.Descripcion,
            Estado = categoriaDTO.Estado,
            FechaDeRegistro = categoriaDTO.FechaDeRegistro
        };

        var newCategoria = await _categoriaRepository.AddAsync(categoria);
        return new CategoriaDTO
        {
            Id = newCategoria.Id,
            Nombre = newCategoria.Nombre,
            Descripcion = newCategoria.Descripcion,
            Estado = newCategoria.Estado,
            FechaDeRegistro = newCategoria.FechaDeRegistro,
        };
    }

    public async Task<CategoriaDTO> UpdateCategoriaAsync(int id,UpdateCategoriaDTO categoriaDTO)
    {
        var categoria = await _categoriaRepository.GetByIdAsync(id);
        if (categoria == null)
        {
            return null;
        }
        categoria.Nombre = categoriaDTO.Nombre;
        categoria.Descripcion = categoriaDTO.Descripcion;
        categoria.Estado = categoriaDTO.Estado;
        var updatedCategoria = await _categoriaRepository.UpdateAsync(categoria);
        return new CategoriaDTO
        {
            Id = updatedCategoria.Id,
            Nombre = updatedCategoria.Nombre,
            Descripcion = updatedCategoria.Descripcion,
            Estado = updatedCategoria.Estado,
            FechaDeRegistro = updatedCategoria.FechaDeRegistro,
        };
    }

    public async Task<bool> DeleteCategoriaAsync (int id)
    {
        return await _categoriaRepository.DeleteAsync(id);
    }    
}