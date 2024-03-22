using GoTransport.Domain.Interfaces;

namespace GoTransport.Domain.Common;

public abstract class AuditableEntity : IAuditableEntity
{
    /// <summary>
    /// Gets or sets ¿Está activo? (1-True, 0-False)
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// Gets or sets fecha de creación del registro
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets fecha de la última modificación del registro
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets usuario que realizó la creación del registro
    /// </summary>
    public string CreationUser { get; set; } = null!;

    /// <summary>
    /// Gets or sets usuario que realizó la última modificación del registro
    /// </summary>
    public string UpdateUser { get; set; } = null!;
}