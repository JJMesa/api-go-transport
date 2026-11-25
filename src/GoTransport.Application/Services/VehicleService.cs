using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Vehicles;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

[Transient]
internal class VehicleService : IVehicleService
{
    private readonly IRepository<Vehicle> _vehicleRepository;
    private readonly ICacheService<Vehicle> _cacheService;

    public VehicleService(IRepository<Vehicle> vehicleRepository
        , ICacheService<Vehicle> cacheService)
    {
        _vehicleRepository = vehicleRepository;
        _cacheService = cacheService;
    }

    public async Task<JsonResponse<IEnumerable<VehicleDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var vehicles = _cacheService.Get(CacheKey.Vehicles);
        if (vehicles is null)
        {
            vehicles = await _vehicleRepository.ListAsync(new VehicleSpecification(), cancellationToken);
            _cacheService.Set(CacheKey.Vehicles, vehicles);
        }

        return ResponseBuilder<IEnumerable<VehicleDto>>.Ok(vehicles.Select(vehicle => vehicle.ToDto()).ToList());
    }

    public async Task<JsonPagedResponse<IEnumerable<VehicleDto>>> GetAsync(VehicleParameters parameters, CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleRepository.ListAsync(new PagedVehicleSpecification(parameters), cancellationToken);
        var totalRecords = await _vehicleRepository.CountAsync(new VehicleSpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalRecords);
        return ResponseBuilder<IEnumerable<VehicleDto>>.OkPaged(vehicles, metadata);
    }

    public async Task<JsonResponse<VehicleDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.FirstOrDefaultAsync(new VehicleDetailSpecification(id), cancellationToken);
        if (vehicle is null) return ResponseBuilder<VehicleDto>.NotFound();

        return ResponseBuilder<VehicleDto>.Ok(vehicle.ToDto());
    }

    public async Task<JsonResponse<VehicleDto>> CreateAsync(VehicleCreationDto vehicleCreation, CancellationToken cancellationToken)
    {
        if (await IsDuplicatePlate(vehicleCreation.LicensePlate, cancellationToken: cancellationToken))
            return ResponseBuilder<VehicleDto>.Conflict(ErrorMessages.DuplicatePlate);

        var vehicle = vehicleCreation.ToEntity();
        await _vehicleRepository.AddAsync(vehicle, cancellationToken);
        InvalidateCachedLists();

        // Reload with the navigations, so the response has the same shape as a get-by-id.
        vehicle = await _vehicleRepository.FirstOrDefaultAsync(new VehicleDetailSpecification(vehicle.VehicleId), cancellationToken) ?? vehicle;

        return ResponseBuilder<VehicleDto>.Created(vehicle.ToDto());
    }

    public async Task<JsonResponse<VehicleDto>> UpdateAsync(int id, VehicleUpdateDto vehicleUpdate, CancellationToken cancellationToken)
    {
        if (id != vehicleUpdate.VehicleId)
            return ResponseBuilder<VehicleDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var vehicle = await _vehicleRepository.GetByIdAsync(id, cancellationToken);
        if (vehicle is null) return ResponseBuilder<VehicleDto>.NotFound();

        vehicleUpdate.ApplyTo(vehicle);
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        InvalidateCachedLists();

        return ResponseBuilder<VehicleDto>.Ok(vehicle.ToDto());
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id, cancellationToken);
        if (vehicle is null) return ResponseBuilder<bool?>.NotFound();

        await _vehicleRepository.DeleteAsync(vehicle, cancellationToken);

        InvalidateCachedLists();
        return ResponseBuilder<bool?>.NoContent();
    }

    private async Task<bool> IsDuplicatePlate(string licensePlate, int? id = null, CancellationToken cancellationToken = default) =>
        await _vehicleRepository.AnyAsync(new DuplicationVehicleSpecification(licensePlate, id), cancellationToken);

    /// <summary>
    /// Drops the cached Vehicle list, so the next read shows this write.
    /// </summary>
    private void InvalidateCachedLists()
    {
        _cacheService.Remove(CacheKey.Vehicles);
    }
}