using GoTransport.Application.Dtos.Route;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IRouteService : IServiceBase<RouteCreationDto, RouteUpdateDto, RouteDto>
{
    Task<JsonPagedResponse<IEnumerable<RouteDto>>> GetAsync(RouteParameters parameters, CancellationToken cancellationToken);
}