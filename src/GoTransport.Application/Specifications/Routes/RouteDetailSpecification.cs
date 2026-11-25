using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Routes;

/// <summary>
/// One Route with the navigations its DTO shows, so a detail response has the same shape as a list item.
/// </summary>
public class RouteDetailSpecification : Specification<Route>
{
    public RouteDetailSpecification(int routeId)
    {
        Query.Where(route => route.RouteId == routeId);

        Query.Include(route => route.OriginPoint.City);
        Query.Include(route => route.DestinationPoint.City);

        // Read-only: nothing loaded here is saved back, so skip change tracking.
        Query.AsNoTracking();
    }
}
