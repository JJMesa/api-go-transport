using GoTransport.Domain.Common;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Domain.Entities.Bas;

public partial class Manufacturer : AuditableEntity
{
    /// <summary>
    /// Identificador único del la empresa que manufacturo el vehículo
    /// </summary>
    public int ManufacturerId { get; set; }

    /// <summary>
    /// Nombre de la empresa que manufacturó el vehículo
    /// </summary>
    public string Description { get; set; } = null!;

    public virtual ICollection<Vehicle> Vehicles { get; } = new List<Vehicle>();
}