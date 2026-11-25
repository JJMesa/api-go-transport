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
    }

    public VehicleSpecification(VehicleParameters parameters)
    {
        VehicleFilter.Apply(Query, parameters);
    }
}