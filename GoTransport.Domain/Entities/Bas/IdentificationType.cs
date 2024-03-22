using GoTransport.Domain.Common;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Domain.Entities.Bas;

public partial class IdentificationType : AuditableEntity
{
    /// <summary>
    /// Identificador único del Tipo de Identificación
    /// </summary>
    public int IdentificationTypeId { get; set; }

    /// <summary>
    /// Descripción del Tipo de Identificación (Cédula, NIT, Pasaporte, etc.)
    /// </summary>
    public string Description { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; } = new List<Reservation>();
}