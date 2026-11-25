using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Routes;

public class RouteSpecification : Specification<Route>
{
    public RouteSpecification()
    {
        Query.Where(ruote => ruote.IsActive!.Value);

        Query.OrderBy(ruote => ruote.Description);

        Query.Include(ruote => ruote.OriginPoint);
        Query.Include(ruote => ruote.OriginPoint!.City);
        Query.Include(ruote => ruote.DestinationPoint);
        Query.Include(ruote => ruote.DestinationPoint!.City);

        // Read-only: nothing loaded here is saved back, so skip change tracking.
        Query.AsNoTracking();
    }

    public RouteSpecification(RouteParameters parameters)
    {
        RouteFilter.Apply(Query, parameters);
    }
}