using GoTransport.Application.Extensions;
using GoTransport.Application.Dtos.Route;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

internal static class RouteMappingExtensions
{
    /// <summary>
    /// Maps a route to its DTO. A navigation property that was not loaded maps to null.
    /// </summary>
    public static RouteDto ToDto(this Route route) => new()
    {
        RouteId = route.RouteId,
        Description = route.Description,
        OriginPoint = route.OriginPoint?.ToDto(),
        DestinationPoint = route.DestinationPoint?.ToDto(),
        IsActive = route.IsActive
    };

    public static Route ToEntity(this RouteCreationDto routeCreation) => new()
    {
        Description = routeCreation.Description.RemoveExtraBlank(),
        OriginPointId = routeCreation.OriginPointId,
        DestinationPointId = routeCreation.DestinationPointId
    };

    public static void ApplyTo(this RouteUpdateDto routeUpdate, Route route)
    {
        route.Description = routeUpdate.Description.RemoveExtraBlank();
        route.OriginPointId = routeUpdate.OriginPointId;
        route.DestinationPointId = routeUpdate.DestinationPointId;
        route.IsActive = routeUpdate.IsActive;
    }
}
