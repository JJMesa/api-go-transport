using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Vehicles;

public class DuplicationVehicleSpecification : Specification<Vehicle>
{
    public DuplicationVehicleSpecification(string licensePlate, int? vehicleId)
    {
        Query.Where(vehicle =>
            vehicle.LicensePlate == licensePlate
            && (vehicle.VehicleId != vehicleId || vehicleId == null));
    }
}