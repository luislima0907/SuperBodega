using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin;

/// <summary>
/// Clase que representa una categoría de productos.
/// </summary>
public class Categoria
{
    /// <summary>
    /// Identificador único de la categoría.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre de la categoría.
    /// </summary>
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre de la categoría no puede exceder los 100 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9\s,.-]+$", ErrorMessage = "El nombre solo puede contener letras, números, espacios y algunos caracteres especiales (, . -)")]
    public string Nombre { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción de la categoría.
    /// </summary>
    [StringLength(200, ErrorMessage = "La descripción no puede exceder los 200 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9\s,.-]+$", ErrorMessage = "La descripcion solo puede contener letras, números, espacios y algunos caracteres especiales (, . -)")]
    public string Descripcion { get; set; } = string.Empty;
    
    /// <summary>
    /// Estado de la categoría (activo/inactivo).
    /// </summary>
    [Required(ErrorMessage = "El estado de la categoría es obligatorio.")]
    [RegularExpression(@"^(true|false)$", ErrorMessage = "El estado debe ser verdadero o falso")]
    public bool Estado { get; set; }
    
    /// <summary>
    /// Fecha de registro de la categoría.
    /// </summary>
    [Required(ErrorMessage = "La fecha de registro es obligatoria.")]
    [DataType(DataType.DateTime)]
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}