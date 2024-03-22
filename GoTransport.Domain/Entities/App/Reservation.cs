using GoTransport.Domain.Common;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Domain.Entities.App;

public partial class Reservation : AuditableEntity
{
    /// <summary>
    /// Identificador único de la reserva
    /// </summary>
    public Guid ReservationId { get; set; }

    /// <summary>
    /// Identificador único del horario
    /// </summary>
    public Guid ScheduleId { get; set; }

    /// <summary>
    /// Fecha de la reserva
    /// </summary>
    public DateTime ReservationDate { get; set; }

    /// <summary>
    /// Nombres de la persona que hizo la reserva
    /// </summary>
    public string PassengerFirstName { get; set; } = null!;

    /// <summary>
    /// Appelidos de la persona que hizo la reserva
    /// </summary>
    public string PassengerLastName { get; set; } = null!;

    /// <summary>
    /// Identificador único del Tipo de Identificación
    /// </summary>
    public int IdentificationTypeId { get; set; }

    /// <summary>
    /// Número de identificación de la persona que hizo la reserva
    /// </summary>
    public string PassengerIdentification { get; set; } = null!;

    /// <summary>
    /// Detalles adicionales de la reserva
    /// </summary>
    public string Detail { get; set; } = null!;

    public virtual Schedule Schedule { get; set; } = null!;

    public virtual IdentificationType IdentificationType { get; set; } = null!;
}