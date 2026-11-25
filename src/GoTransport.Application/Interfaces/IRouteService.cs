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

    Task<JsonResponse<RouteDto>> CreateAsync(RouteCreationDto routeCreation);

    Task<JsonResponse<RouteDto>> UpdateAsync(int id, RouteUpdateDto routeUpdate);

    Task<JsonResponse<bool?>> DeleteAsync(int id);

    Task<JsonPagedResponse<IEnumerable<RouteDto>>> GetAsync(RouteParameters parameters, CancellationToken cancellationToken);
}