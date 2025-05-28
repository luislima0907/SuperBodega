namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// DTO para visualizar datos completos de una categoría
/// </summary>
public class CategoriaDTO
{
    /// <summary>
    /// Identificador único de la categoría
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Nombre de la categoría
    /// </summary>
    public string Nombre { get; set; }

    /// <summary>
    /// Descripción de la categoría
    /// </summary>
    public string Descripcion { get; set; }

    /// <summary>
    /// Indica si la categoría está activa o inactiva
    /// </summary>
    public bool Estado { get; set; }

    /// <summary>
    /// Fecha en que se registró la categoría en el sistema
    /// </summary>
    /// <remarks>
    /// Esta fecha se establece automáticamente al crear la categoría.
    /// </remarks>
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO para la creación de una nueva categoría
/// </summary>
public class CreateCategoriaDTO
{
    /// <summary>
    /// Nombre de la categoría (obligatorio)
    /// </summary>
    public string Nombre { get; set; }

    /// <summary>
    /// Descripción de la categoría (opcional)
    /// </summary>
    public string Descripcion { get; set; }

    /// <summary>
    /// Indica si la categoría está activa o inactiva
    /// </summary>
    public bool Estado { get; set; }

    /// <summary>
    /// Fecha en que se registró la categoría en el sistema
    /// </summary>
    /// <remarks>
    /// Esta fecha se establece automáticamente al crear la categoría.
    /// </remarks>
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO para la actualización de una categoría existente
/// </summary>
public class UpdateCategoriaDTO
{
    /// <summary>
    /// Nombre de la categoría (obligatorio)
    /// </summary>
    public string Nombre { get; set; }

    /// <summary>
    /// Descripción de la categoría (opcional)
    /// </summary>
    public string Descripcion { get; set; }

    /// <summary>
    /// Indica si la categoría está activa o inactiva
    /// </summary>
    public bool Estado { get; set; }
}