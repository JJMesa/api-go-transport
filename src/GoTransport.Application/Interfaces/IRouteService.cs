using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IRouteService
{
    /// <summary>
    /// Every active Route, served from the in-memory cache.
    /// </summary>
    Task<JsonResponse<IEnumerable<RouteDto>>> GetAllAsync(CancellationToken cancellationToken);

    Task<JsonResponse<RouteDto>> GetByIdAsync(int id, CancellationToken cancellationToken);

    Task<JsonResponse<RouteDto>> CreateAsync(RouteCreationDto routeCreation, CancellationToken cancellationToken);

    Task<JsonResponse<RouteDto>> UpdateAsync(int id, RouteUpdateDto routeUpdate, CancellationToken cancellationToken);

    Task<JsonResponse<bool?>> DeleteAsync(int id, CancellationToken cancellationToken);

    Task<JsonPagedResponse<IEnumerable<RouteDto>>> GetAsync(RouteParameters parameters, CancellationToken cancellationToken);
}