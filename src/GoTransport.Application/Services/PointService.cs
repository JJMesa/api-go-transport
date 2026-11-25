using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Parameters;
using GoTransport.Application.Specifications.Points;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Services;

internal class PointService : IPointService
{
    private readonly IRepository<Point> _pointRepository;
    private readonly ICacheService<Point> _cacheService;

    public PointService(IRepository<Point> pointRepository
        , ICacheService<Point> cacheService)
    {
        _pointRepository = pointRepository;
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
        return ResponseBuilder<IEnumerable<PointDto>>.Ok(points.Select(point => point.ToDto()).ToList());
    }

    public async Task<JsonPagedResponse<IEnumerable<PointDto>>> GetAsync(PointParameters parameters, CancellationToken cancellationToken)
    {
        var points = await _pointRepository.ListAsync(new PagedPointSpecification(parameters), cancellationToken);
        var totalAmount = await _pointRepository.CountAsync(new PointSpecification(parameters), cancellationToken);
        var metadata = Metadata.Create(parameters.PageNumber, parameters.PageSize, totalAmount);
        return ResponseBuilder<IEnumerable<PointDto>>.OkPaged(points, metadata);
    }

    public async Task<JsonResponse<PointDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var point = await _pointRepository.FirstOrDefaultAsync(new PointSpecification(id), cancellationToken);
        if (point is null) return ResponseBuilder<PointDto>.NotFound();
        return ResponseBuilder<PointDto>.Ok(point.ToDto());
    }

    public async Task<JsonResponse<PointDto>> CreateAsync(PointCreationDto pointCreation, CancellationToken cancellationToken)
    {
        var point = pointCreation.ToEntity();
        await _pointRepository.AddAsync(point, cancellationToken);
        InvalidateCachedLists();

        // Reload with the City, so the response has the same shape as a get-by-id.
        point = await _pointRepository.FirstOrDefaultAsync(new PointSpecification(point.PointId), cancellationToken) ?? point;

        return ResponseBuilder<PointDto>.Created(point.ToDto());
    }

    public async Task<JsonResponse<PointDto>> UpdateAsync(int id, PointUpdateDto pointUpdate, CancellationToken cancellationToken)
    {
        if (id != pointUpdate.PointId)
            return ResponseBuilder<PointDto>.BadRequest(ErrorMessages.UrlAndBodyIdNotEqual);

        var point = await _pointRepository.FirstOrDefaultAsync(new PointSpecification(id), cancellationToken);
        if (point is null) return ResponseBuilder<PointDto>.NotFound();


        pointUpdate.ApplyTo(point);
        await _pointRepository.UpdateAsync(point, cancellationToken);
        InvalidateCachedLists();

        return ResponseBuilder<PointDto>.Ok(point.ToDto());
    }

    public async Task<JsonResponse<bool?>> DeleteAsync(int pointId, CancellationToken cancellationToken)
    {
        var point = await _pointRepository.GetByIdAsync(pointId, cancellationToken);
        if (point is null) return ResponseBuilder<bool?>.NotFound();
        await _pointRepository.DeleteAsync(point, cancellationToken);
        InvalidateCachedLists();
        return ResponseBuilder<bool?>.NoContent();
    }

    /// <summary>
    /// Drops the cached lists that show Points: the Point list and the Route list, whose Routes embed their Points.
    /// </summary>
    private void InvalidateCachedLists()
    {
        _cacheService.Remove(CacheKey.Points);
        _cacheService.Remove(CacheKey.Routes);
    }
}