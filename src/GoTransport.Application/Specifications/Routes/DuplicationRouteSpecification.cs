using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Routes;

public class DuplicationRouteSpecification : Specification<Route>
{
    public DuplicationRouteSpecification(int originPointId, int destinationPointId, int? routeId)
    {
        Query.Where(route => route.OriginPointId == originPointId && route.DestinationPointId == destinationPointId);

        if (routeId.HasValue)
            Query.Where(route => route.RouteId != routeId.Value);
    }
}