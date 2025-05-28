using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SuperBodega.API.Models.Admin;

/// <summary>
/// Clase que representa un cliente.
/// </summary>
public class Cliente
{
    /// <summary>
    /// Identificador único del cliente.
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>
    /// Nombre del cliente.
    /// </summary>
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras, tildes y espacios")]
    public string Nombre { get; set; }

    /// <summary>
    /// Apellido del cliente.
    /// </summary>
    [Required(ErrorMessage = "El apellido es obligatorio")]
    [StringLength(100, ErrorMessage = "El apellido no puede exceder los 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ\s]+$", ErrorMessage = "El apellido solo puede contener letras, tildes y espacios")]
    public string Apellido { get; set; }

    /// <summary>
    /// Email del cliente.
    /// </summary>
    [Required(ErrorMessage = "El email es obligatorio")]
    [EmailAddress(ErrorMessage = "El formato del email es inválido")]
    [StringLength(100, ErrorMessage = "El email no puede exceder los 100 caracteres")]
    public string Email { get; set; }

    /// <summary>
    /// Teléfono del cliente.
    /// </summary>
    [Required(ErrorMessage = "El teléfono es obligatorio")]
    [Phone(ErrorMessage = "Formato de teléfono inválido")]
    [StringLength(15, ErrorMessage = "El teléfono no puede exceder los 15 caracteres")]
    [RegularExpression(@"^\d{10,15}$", ErrorMessage = "El teléfono debe contener solo números y tener entre 10 y 15 dígitos")]
    public string Telefono { get; set; }

    /// <summary>
    /// Dirección del cliente.
    /// </summary>
    [Required(ErrorMessage = "La dirección es obligatoria")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚüÜñÑ0-9\s,.-]+$", ErrorMessage = "La dirección solo puede contener letras, números, espacios y algunos caracteres especiales (, . -)")]
    public string Direccion { get; set; }

    /// <summary>
    /// Estado del cliente (activo/inactivo).
    /// </summary>
    [Required(ErrorMessage = "El estado es obligatorio")]
    [RegularExpression(@"^(true|false)$", ErrorMessage = "El estado debe ser verdadero o falso")]
    public bool Estado { get; set; }

    /// <summary>
    /// Fecha de registro del cliente.
    /// </summary>
    [Required(ErrorMessage = "La fecha de registro es obligatoria")]
    [DataType(DataType.DateTime)]
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}