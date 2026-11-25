using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Vehicles;

public class DuplicationVehicleSpecification : Specification<Vehicle>
{
    public DuplicationVehicleSpecification(string licensePlate, int? vehicleId)
    {
        Query.Where(vehicle => vehicle.LicensePlate == licensePlate);

        if (vehicleId.HasValue)
            Query.Where(vehicle => vehicle.VehicleId != vehicleId.Value);
    }
}