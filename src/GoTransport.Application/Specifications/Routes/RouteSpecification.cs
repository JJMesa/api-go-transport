using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Routes;

public class RouteSpecification : Specification<Route>
{
    public RouteSpecification()
    {
        Query.Where(route => route.IsActive!.Value);

        Query.OrderBy(route => route.Description);

        Query.Include(route => route.OriginPoint);
        Query.Include(route => route.OriginPoint!.City);
        Query.Include(route => route.DestinationPoint);
        Query.Include(route => route.DestinationPoint!.City);

        // Read-only: nothing loaded here is saved back, so skip change tracking.
        Query.AsNoTracking();
    }

    public RouteSpecification(RouteParameters parameters)
    {
        RouteFilter.Apply(Query, parameters);
    }
}