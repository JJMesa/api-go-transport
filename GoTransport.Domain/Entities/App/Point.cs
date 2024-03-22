using GoTransport.Domain.Common;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Domain.Entities.App;

public partial class Point : AuditableEntity
{
    /// <summary>
    /// Identificador único del punto, ya sea de origen o destino
    /// </summary>
    public int PointId { get; set; }

    /// <summary>
    /// Identificador único del municipio
    /// </summary>
    public int CityId { get; set; }

    /// <summary>
    /// Detalle del punto (Zona Sur, Terminal de Transporte Norte, etc)
    /// </summary>
    public string Detail { get; set; } = null!;

    public virtual City City { get; set; } = null!;

    public virtual ICollection<Route> OriginRoutes { get; } = new List<Route>();

    public virtual ICollection<Route> DestinationRoutes { get; } = new List<Route>();
}