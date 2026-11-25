using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IVehicleService
{
    /// <summary>
    /// Every active Vehicle, served from the in-memory cache.
    /// </summary>
    Task<JsonResponse<IEnumerable<VehicleDto>>> GetAllAsync(CancellationToken cancellationToken);

    Task<JsonResponse<VehicleDto>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<JsonResponse<VehicleDto>> CreateAsync(VehicleCreationDto vehicleCreation);

    Task<JsonResponse<VehicleDto>> UpdateAsync(int id, VehicleUpdateDto vehicleUpdate);

    Task<JsonResponse<bool?>> DeleteAsync(int id);

    Task<JsonPagedResponse<IEnumerable<VehicleDto>>> GetAsync(VehicleParameters parameters, CancellationToken cancellationToken);
}