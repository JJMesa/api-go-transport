using GoTransport.Domain.Common;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Domain.Entities.Bas;

public partial class City : AuditableEntity
{
    /// <summary>
    /// Identificador único del municipio
    /// </summary>
    public int CityId { get; set; }

    /// <summary>
    /// Descripción del municipio (Bogotá, Medellín, etc)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Identificador único del departamento
    /// </summary>
    public int DepartmentId { get; set; }

    public virtual Department Department { get; set; } = null!;

    public virtual ICollection<Point> Points { get; } = new List<Point>();
}