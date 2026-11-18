namespace GoTransport.Domain.Interfaces;

public interface IAuditableEntity
{
    /// <summary>
    /// ¿Está activo? (1-True, 0-False)
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Fecha de creación del registro
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Fecha de la última modificación del registro
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Usuario que realizó la creación del registro
    /// </summary>
    public string CreationUser { get; set; }

    /// <summary>
    /// Usuario que realizó la última modificación del registro
    /// </summary>
    public string UpdateUser { get; set; }
}