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

        if (!string.IsNullOrWhiteSpace(parameters.DestinationPointSearchCriteria))
        {
            Query.Search(ruote => $"{ruote.DestinationPoint.City.Description}", $"%{parameters.OriginPointSearchCriteria}%");
            Query.Search(ruote => $"{ruote.DestinationPoint.Detail}", $"%{parameters.OriginPointSearchCriteria}%");
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