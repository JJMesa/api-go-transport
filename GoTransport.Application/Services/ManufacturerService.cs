using AutoMapper;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Manufacturer;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Services;

[Transient]
internal class ManufacturerService : IManufacturerService
{
    private readonly IRepository<Manufacturer> _manufacturerRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService<Manufacturer> _cacheService;

    public ManufacturerService(IRepository<Manufacturer> manufacturerRepository
        , IMapper mapper
        , ICacheService<Manufacturer> cacheService)
    {
        _manufacturerRepository = manufacturerRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<JsonResponse<IEnumerable<ManufacturerDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var manufacturers = _cacheService.Get(CacheKey.Manufacturers);
        if (manufacturers is null)
        {
            manufacturers = await _manufacturerRepository.ListAsync(cancellationToken);
            _cacheService.Set(CacheKey.Manufacturers, manufacturers);
        }

        return ResponseBuilder<IEnumerable<ManufacturerDto>>.Ok(_mapper.Map<IEnumerable<ManufacturerDto>>(manufacturers));
    }
}