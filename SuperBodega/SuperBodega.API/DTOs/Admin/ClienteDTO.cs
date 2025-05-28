namespace SuperBodega.API.DTOs.Admin;

/// <summary>
/// DTO para visualizar datos completos de un cliente
/// </summary>
public class ClienteDTO
{
    /// <summary>
    /// Identificador único del cliente
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre del cliente
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Apellido del cliente
    /// </summary>
    public string Apellido { get; set; }
    
    /// <summary>
    /// Correo electrónico del cliente
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Número de teléfono del cliente
    /// </summary>
    public string Telefono { get; set; }
    
    /// <summary>
    /// Dirección física del cliente
    /// </summary>
    public string Direccion { get; set; }
    
    /// <summary>
    /// Indica si el cliente está activo o inactivo
    /// </summary>
    public bool Estado { get; set; }
    
    /// <summary>
    /// Fecha en que se registró el cliente en el sistema
    /// </summary>
    /// <remarks>
    /// Esta fecha se establece automáticamente al crear el cliente.
    /// </remarks>
    public DateTime FechaDeRegistro { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// DTO para la creación de un nuevo cliente
/// </summary>
public class CreateClienteDTO
{
    /// <summary>
    /// Nombre del cliente (obligatorio)
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Apellido del cliente (obligatorio)
    /// </summary>
    public string Apellido { get; set; }
    
    /// <summary>
    /// Correo electrónico del cliente
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Número de teléfono del cliente
    /// </summary>
    public string Telefono { get; set; }
    
    /// <summary>
    /// Dirección física del cliente
    /// </summary>
    public string Direccion { get; set; }
    
    /// <summary>
    /// Indica si el cliente está activo o inactivo
    /// </summary>
    public bool Estado { get; set; }
    
    /// <summary>
    /// Fecha de registro del cliente
    /// </summary>
    /// <remarks>
    /// Esta fecha se establece automáticamente al crear el cliente.
    /// </remarks>
    public DateTime FechaDeRegistro { get; set; } = DateTime.Now;
}

/// <summary>
/// DTO para actualizar datos de un cliente existente
/// </summary>
public class UpdateClienteDTO
{
    /// <summary>
    /// Nombre del cliente (obligatorio)
    /// </summary>
    public string Nombre { get; set; }
    
    /// <summary>
    /// Apellido del cliente (obligatorio)
    /// </summary>
    public string Apellido { get; set; }
    
    /// <summary>
    /// Correo electrónico del cliente
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Número de teléfono del cliente
    /// </summary>
    public string Telefono { get; set; }
    
    /// <summary>
    /// Dirección física del cliente
    /// </summary>
    public string Direccion { get; set; }
    
    /// <summary>
    /// Estado del cliente (activo/inactivo)
    /// </summary>
    public bool Estado { get; set; }
}