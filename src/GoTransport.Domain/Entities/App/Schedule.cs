using GoTransport.Domain.Common;

namespace GoTransport.Domain.Entities.App;

public partial class Schedule : AuditableEntity
{
    /// <summary>
    /// Identificador único del horario
    /// </summary>
    public Guid ScheduleId { get; set; }

    /// <summary>
    /// Hora de salida
    /// </summary>
    public TimeSpan DepartureTime { get; set; }

    /// <summary>
    /// Hora de llegada
    /// </summary>
    public TimeSpan ArrivalTime { get; set; }

    /// <summary>
    /// Duración aproximada del trayecto
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Identificador único del vehículo
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Identificador único del trayecto
    /// </summary>
    public int RouteId { get; set; }

    public virtual Vehicle Vehicle { get; set; } = null!;

    public virtual Route Route { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; } = new List<Reservation>();
}