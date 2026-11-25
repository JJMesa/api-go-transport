using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Vehicles;

/// <summary>
/// One Vehicle with its Manufacturer, so a detail response has the same shape as a list item.
/// </summary>
public class VehicleDetailSpecification : Specification<Vehicle>
{
    public VehicleDetailSpecification(int vehicleId)
    {
        Query.Where(vehicle => vehicle.VehicleId == vehicleId);

        Query.Include(vehicle => vehicle.Manufacturer);
    }
}
