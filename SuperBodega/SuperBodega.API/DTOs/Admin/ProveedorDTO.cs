namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// DTO para visualizar datos completos de un proveedor
/// </summary>
public class ProveedorDTO
{
    /// <summary>
    /// Identificador único del proveedor
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre del proveedor
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Email del proveedor
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Teléfono del proveedor
    /// </summary>
    public string Telefono { get; set; }
    
    /// <summary>
    /// Dirección del proveedor
    /// </summary>
    public string Direccion { get; set; }
    
    /// <summary>
    /// Indica si el proveedor está activo o inactivo
    /// </summary>
    public bool Estado { get; set; }
    
    /// <summary>
    /// Fecha en que se registró el proveedor en el sistema
    /// </summary>
    public DateTime FechaDeRegistro { get; set; }
}

/// <summary>
/// DTO para la creación de un nuevo proveedor
/// </summary>
public class CreateProveedorDTO
{
    /// <summary>
    /// Nombre del proveedor
    /// </summary>
    public string Nombre { get; set; }

    /// <summary>
    /// Email del proveedor
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Teléfono del proveedor
    /// </summary>
    public string Telefono { get; set; }
    
    /// <summary>
    /// Dirección del proveedor
    /// </summary>
    public string Direccion { get; set; }
    
    /// <summary>
    /// Indica si el proveedor está activo o inactivo
    /// </summary>
    public bool Estado { get; set; }
    
    /// <summary>
    /// Fecha en que se registró el proveedor en el sistema
    /// </summary>
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO para la actualización de un proveedor existente
/// </summary>
public class UpdateProveedorDTO
{
    /// <summary>
    /// Nombre del proveedor
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Email del proveedor
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Teléfono del proveedor
    /// </summary>
    public string Telefono { get; set; }
    
    /// <summary>
    /// Dirección del proveedor
    /// </summary>
    public string Direccion { get; set; }
    
    /// <summary>
    /// Indica si el proveedor está activo o inactivo
    /// </summary>
    public bool Estado { get; set; }
}