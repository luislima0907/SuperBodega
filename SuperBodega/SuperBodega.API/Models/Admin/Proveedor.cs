using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin;

/// <summary>
/// Clase que representa un proveedor de productos.
/// </summary>
public class Proveedor
{
    /// <summary>
    /// Identificador único del proveedor.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre del proveedor.
    /// </summary>
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del proveedor no puede exceder los 100 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras, tildes y espacios")]
    public string Nombre { get; set; }
    
    /// <summary>
    /// Email del proveedor
    /// </summary>
    [Required(ErrorMessage = "El email del proveedor es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email es inválido.")]
    [StringLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres.")]
    public string Email { get; set; }
    
    /// <summary>
    /// Teléfono del proveedor
    /// </summary>
    [Required(ErrorMessage = "El teléfono del proveedor es obligatorio.")]
    [Phone(ErrorMessage = "Formato de teléfono inválido.")]
    [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres.")]
    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "El teléfono debe contener solo números y tener entre 10 y 15 dígitos")]
    public string Telefono { get; set; }
    
    /// <summary>
    /// Dirección del proveedor
    /// </summary>
    [Required(ErrorMessage = "La dirección del proveedor es obligatoria.")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres.")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9\s,.-]+$", ErrorMessage = "La dirección solo puede contener letras, números, espacios y algunos caracteres especiales (, . -)")]
    public string Direccion { get; set; }
    
    /// <summary>
    /// Estado del proveedor (activo/inactivo).
    /// </summary>
    [Required(ErrorMessage = "El estado del proveedor es obligatorio.")]
    [RegularExpression(@"^(true|false)$", ErrorMessage = "El estado debe ser verdadero o falso")]
    public bool Estado { get; set; }
    
    /// <summary>
    /// Fecha de registro del proveedor.
    /// </summary>
    [Required(ErrorMessage = "La fecha de registro es obligatoria.")]
    [DataType(DataType.DateTime)]
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}