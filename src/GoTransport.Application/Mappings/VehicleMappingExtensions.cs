using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

internal static class VehicleMappingExtensions
{
    /// <summary>
    /// Maps a vehicle to its DTO. A navigation property that was not loaded maps to null.
    /// </summary>
    public static VehicleDto ToDto(this Vehicle vehicle) => new()
    {
        VehicleId = vehicle.VehicleId,
        Manufacturer = vehicle.Manufacturer?.ToDto()!,
        LicensePlate = vehicle.LicensePlate,
        Model = vehicle.Model,
        Capacity = vehicle.Capacity,
        IsActive = vehicle.IsActive ?? false
    };

    public static Vehicle ToEntity(this VehicleCreationDto vehicleCreation) => new()
    {
        ManufacturerId = vehicleCreation.ManufacturerId,
        LicensePlate = vehicleCreation.LicensePlate,
        Model = vehicleCreation.Model,
        Capacity = vehicleCreation.Capacity
    };

    public static void ApplyTo(this VehicleUpdateDto vehicleUpdate, Vehicle vehicle)
    {
        vehicle.Capacity = vehicleUpdate.Capacity;
    }
}
