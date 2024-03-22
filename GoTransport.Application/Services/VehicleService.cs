using AutoMapper;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Vehicle;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Vehicles;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

[Transient]
internal class VehicleService : IVehicleService
{
    private readonly IRepository<Vehicle> _vehicleRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService<Vehicle> _cacheService;

    public VehicleService(IRepository<Vehicle> vehicleRepository
        , IMapper mapper
        , ICacheService<Vehicle> cacheService)
    {
        _vehicleRepository = vehicleRepository;
        _mapper = mapper;
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

        return ResponseBuilder<IEnumerable<VehicleDto>>.Ok(_mapper.Map<IEnumerable<VehicleDto>>(vehicles));
    }

    public async Task<JsonPagedResponse<IEnumerable<VehicleDto>>> GetAsync(VehicleParameters parameters, CancellationToken cancellationToken)
    {
        var vehicles = await _vehicleRepository.ListAsync(new PagedVehicleSpecification(parameters), cancellationToken);
        var totalRecords = await _vehicleRepository.CountAsync(new VehicleSpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalRecords);
        return ResponseBuilder<IEnumerable<VehicleDto>>.OkPaged(_mapper.Map<IEnumerable<VehicleDto>>(vehicles), metadata);
    }

    public async Task<JsonResponse<VehicleDto>> GetByIdAsync(dynamic id, CancellationToken cancellationToken)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id, cancellationToken);
        if (vehicle is null) return ResponseBuilder<VehicleDto>.NotFound();

        return ResponseBuilder<VehicleDto>.Ok(_mapper.Map<VehicleDto>(vehicle));
    }

    public async Task<JsonResponse<VehicleDto>> CreateAsync(VehicleCreationDto vehicleCreation)
    {
        if (await IsDuplicatePlate(vehicleCreation.LicensePlate))
            return ResponseBuilder<VehicleDto>.BadRequest(ErrorMessages.DuplicatePlate);

        var vehicle = _mapper.Map<Vehicle>(vehicleCreation);
        await _vehicleRepository.AddAsync(vehicle);

        return ResponseBuilder<VehicleDto>.Created(_mapper.Map<VehicleDto>(vehicle));
    }

    public async Task<JsonResponse<VehicleDto>> UpdateAsync(dynamic id, VehicleUpdateDto vehicleUpdate)
    {
        if (id != vehicleUpdate.VehicleId)
            return ResponseBuilder<VehicleDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle is null) return ResponseBuilder<VehicleDto>.NotFound();

        vehicle = _mapper.Map(vehicleUpdate, vehicle);
        await _vehicleRepository.UpdateAsync(vehicle);

        return ResponseBuilder<VehicleDto>.Ok(_mapper.Map<VehicleDto>(vehicle));
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(dynamic id)
    {
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle is null) return ResponseBuilder<bool?>.NotFound();

        await _vehicleRepository.DeleteAsync(vehicle);
        return ResponseBuilder<bool?>.NoContent();
    }

    private async Task<bool> IsDuplicatePlate(string licensePlate, int? id = null) =>
        await _vehicleRepository.AnyAsync(new DuplicationVehicleSpecification(licensePlate, id));
}