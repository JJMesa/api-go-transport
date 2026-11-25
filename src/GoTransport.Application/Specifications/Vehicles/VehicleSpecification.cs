using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Vehicles;

public class VehicleSpecification : Specification<Vehicle>
{
    public VehicleSpecification()
    {
        Query.Where(vehicle => vehicle.IsActive!.Value);

        Query.OrderBy(vehicle => vehicle.LicensePlate);

        // The cached catalog shows each Vehicle with its Manufacturer, like the paged list.
        Query.Include(vehicle => vehicle.Manufacturer);

        // Read-only: nothing loaded here is saved back, so skip change tracking.
        Query.AsNoTracking();
    }

    public VehicleSpecification(VehicleParameters parameters)
    {
        VehicleFilter.Apply(Query, parameters);
    }
}