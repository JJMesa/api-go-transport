using AutoMapper;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Extensions;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Points;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

[Transient]
internal class PointService : IPointService
{
    private readonly IRepository<Point> _pointRepository;
    private readonly IMapper _mapper;
    private readonly ICacheService<Point> _cacheService;

    public PointService(IRepository<Point> pointRepository
        , IMapper mapper
        , ICacheService<Point> cacheService)
    {
        _pointRepository = pointRepository;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<JsonResponse<IEnumerable<PointDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var points = _cacheService.Get(CacheKey.Points);
        if (points is null)
        {
            points = await _pointRepository.ListAsync(new PointSpecification(), cancellationToken);
            _cacheService.Set(CacheKey.Points, points);
        }
        return ResponseBuilder<IEnumerable<PointDto>>.Ok(_mapper.Map<IEnumerable<PointDto>>(points));
    }

    public async Task<JsonPagedResponse<IEnumerable<PointDto>>> GetAsync(PointParameters parameters, CancellationToken cancellationToken)
    {
        var points = await _pointRepository.ListAsync(new PagedPointSpecification(parameters), cancellationToken);
        var totalAmount = await _pointRepository.CountAsync(new PointSpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalAmount);
        return ResponseBuilder<IEnumerable<PointDto>>.OkPaged(points, metadata);
    }

    public async Task<JsonResponse<PointDto>> GetByIdAsync(dynamic id, CancellationToken cancellationToken)
    {
        var point = await _pointRepository.FirstOrDefaultAsync(new PointSpecification(id), cancellationToken);
        if (point is null) return ResponseBuilder<PointDto>.NotFound();
        return ResponseBuilder<PointDto>.Ok(_mapper.Map<PointDto>(point));
    }

    public async Task<JsonResponse<PointDto>> CreateAsync(PointCreationDto pointCreation)
    {
        var point = _mapper.Map<Point>(pointCreation);
        await _pointRepository.AddAsync(point);

        return ResponseBuilder<PointDto>.Created(_mapper.Map<PointDto>(point));
    }

    public async Task<JsonResponse<PointDto>> UpdateAsync(dynamic id, PointUpdateDto pointUpdate)
    {
        if (id != pointUpdate.PointId)
            return ResponseBuilder<PointDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var point = await _pointRepository.FirstOrDefaultAsync(new PointSpecification(id));
        if (point is null) return ResponseBuilder<PointDto>.NotFound();

        pointUpdate.Detail = pointUpdate.Detail.RemoveExtraBlank();

        point = _mapper.Map(pointUpdate, point);
        await _pointRepository.UpdateAsync(point);

        return ResponseBuilder<PointDto>.Ok(_mapper.Map<PointDto>(point));
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(dynamic pointId)
    {
        var point = await _pointRepository.GetByIdAsync(pointId);
        if (point is null) return ResponseBuilder<bool?>.NotFound();
        await _pointRepository.DeleteAsync(point);
        return ResponseBuilder<bool?>.NoContent();
    }
}