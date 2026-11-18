using GoTransport.Domain.Common;

namespace GoTransport.Domain.Entities.App;

public partial class Route : AuditableEntity
{
    /// <summary>
    /// Identificador único del trayecto
    /// </summary>
    public int RouteId { get; set; }

    /// <summary>
    /// Descripción del trayecto
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// Identificador único del punto de origen
    /// </summary>
    public int OriginPointId { get; set; }

    /// <summary>
    /// Identificador único del punto de destino
    /// </summary>
    public int DestinationPointId { get; set; }

    public virtual Point OriginPoint { get; set; } = null!;

    public virtual Point DestinationPoint { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; } = new List<Schedule>();
}