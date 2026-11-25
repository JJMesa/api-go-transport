using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Routes;

/// <summary>
/// The filters a page of Route rows and its total count share, so the page and the count always agree.
/// </summary>
internal static class RouteFilter
{
    public static void Apply(ISpecificationBuilder<Route> query, RouteParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.OriginPointSearchCriteria))
        {
            query.Search(ruote => $"{ruote.OriginPoint.City.Description}", $"%{parameters.OriginPointSearchCriteria}%");
            query.Search(ruote => $"{ruote.OriginPoint.Detail}", $"%{parameters.OriginPointSearchCriteria}%");
        }

        // Search terms in the same group are OR-ed and groups are AND-ed: the origin matches its City or
        // Point, the destination matches its own, and both must hold when both are given.
        if (!string.IsNullOrWhiteSpace(parameters.DestinationPointSearchCriteria))
        {
            query.Search(ruote => $"{ruote.DestinationPoint.City.Description}", $"%{parameters.DestinationPointSearchCriteria}%", group: 2);
            query.Search(ruote => $"{ruote.DestinationPoint.Detail}", $"%{parameters.DestinationPointSearchCriteria}%", group: 2);
        }

        if (parameters.IsActive.HasValue)
            query.Where(ruote => ruote.IsActive == parameters.IsActive);
    }
}
