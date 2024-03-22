using GoTransport.Domain.Common;

namespace GoTransport.Domain.Entities.Bas;

public partial class Department : AuditableEntity
{
    /// <summary>
    /// Identificador único del departamento
    /// </summary>
    public int DepartmentId { get; set; }

    /// <summary>
    /// Descripción del departamento (Cundinamarca, Antioquia, etc)
    /// </summary>
    public string Description { get; set; } = null!;

    public ICollection<City> Cities { get; set; } = new List<City>();
}