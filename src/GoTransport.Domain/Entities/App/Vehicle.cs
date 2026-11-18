using GoTransport.Domain.Common;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Domain.Entities.App;

public partial class Vehicle : AuditableEntity
{
    /// <summary>
    /// Identificador único del vehículo
    /// </summary>
    public int VehicleId { get; set; }

    /// <summary>
    /// Identificador único del la empresa que manufacturo el vehículo
    /// </summary>
    public int ManufacturerId { get; set; }

    /// <summary>
    /// Placa del vehículo
    /// </summary>
    public string LicensePlate { get; set; } = null!;

    /// <summary>
    /// Modelo del vehículo (2017, 2022, etc)
    /// </summary>
    public int Model { get; set; }

    /// <summary>
    /// Capacidad máx. de personas en el vehículo
    /// </summary>
    public int Capacity { get; set; }

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<Schedule> Schedules { get; } = new List<Schedule>();
}