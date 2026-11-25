using GoTransport.Application.Dtos.Point;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IPointService
{
    /// <summary>
    /// Every active Point, served from the in-memory cache.
    /// </summary>
    Task<JsonResponse<IEnumerable<PointDto>>> GetAllAsync(CancellationToken cancellationToken);

    Task<JsonResponse<PointDto>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<JsonResponse<PointDto>> CreateAsync(PointCreationDto pointCreation);

    Task<JsonResponse<PointDto>> UpdateAsync(int id, PointUpdateDto pointUpdate);

    Task<JsonResponse<bool?>> DeleteAsync(int id);

    Task<JsonPagedResponse<IEnumerable<PointDto>>> GetAsync(PointParameters parameters, CancellationToken cancellationToken);
}