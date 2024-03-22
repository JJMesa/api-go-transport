using AutoMapper;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.IdentificationType;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Services;

[Transient]
internal class IdentificationTypeService : IIdentificationTypeService
{
    private readonly IRepository<IdentificationType> _identificationTypeRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService<IdentificationType> _cacheService;

    public IdentificationTypeService(IRepository<IdentificationType> identificationTypeRepository
        , IMapper mapper
        , ICacheService<IdentificationType> cacheService)
    {
        _identificationTypeRepository = identificationTypeRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<JsonResponse<IEnumerable<IdentificationTypeDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var identificationTypes = _cacheService.Get(CacheKey.IdentificationTypes);
        if (identificationTypes is null)
        {
            identificationTypes = await _identificationTypeRepository.ListAsync(cancellationToken);
            _cacheService.Set(CacheKey.IdentificationTypes, identificationTypes);
        }

        return ResponseBuilder<IEnumerable<IdentificationTypeDto>>.Ok(_mapper.Map<IEnumerable<IdentificationTypeDto>>(identificationTypes));
    }
}