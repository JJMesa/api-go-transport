using Ardalis.Specification;
using GoTransport.Application.Enums;
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
    }

    public RouteSpecification(RouteParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.OriginPointSearchCriteria))
        {
            Query.Search(ruote => $"{ruote.OriginPoint.City.Description}", $"%{parameters.OriginPointSearchCriteria}%");
            Query.Search(ruote => $"{ruote.OriginPoint.Detail}", $"%{parameters.OriginPointSearchCriteria}%");
        }

        // Search terms in the same group are OR-ed and groups are AND-ed: the origin matches its City or
        // Point, the destination matches its own, and both must hold when both are given.
        if (!string.IsNullOrWhiteSpace(parameters.DestinationPointSearchCriteria))
        {
            Query.Search(ruote => $"{ruote.DestinationPoint.City.Description}", $"%{parameters.DestinationPointSearchCriteria}%", group: 2);
            Query.Search(ruote => $"{ruote.DestinationPoint.Detail}", $"%{parameters.DestinationPointSearchCriteria}%", group: 2);
        }

        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(ruote => ruote.Description);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(ruote => ruote.Description);
                break;

            default:
                Query.OrderBy(ruote => ruote.Description);
                break;
        }

        if (parameters.IsActive.HasValue)
            Query.Where(ruote => ruote.IsActive == parameters.IsActive);
    }
}