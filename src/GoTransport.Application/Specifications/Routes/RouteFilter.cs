using Ardalis.Specification;
using GoTransport.Application.Extensions;
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
            query.Search(route => route.OriginPoint.City.Description, parameters.OriginPointSearchCriteria.ToContainsPattern());
            query.Search(route => route.OriginPoint.Detail, parameters.OriginPointSearchCriteria.ToContainsPattern());
        }

        // Search terms in the same group are OR-ed and groups are AND-ed: the origin matches its City or
        // Point, the destination matches its own, and both must hold when both are given.
        if (!string.IsNullOrWhiteSpace(parameters.DestinationPointSearchCriteria))
        {
            query.Search(route => route.DestinationPoint.City.Description, parameters.DestinationPointSearchCriteria.ToContainsPattern(), group: 2);
            query.Search(route => route.DestinationPoint.Detail, parameters.DestinationPointSearchCriteria.ToContainsPattern(), group: 2);
        }

        if (parameters.IsActive.HasValue)
            query.Where(route => route.IsActive == parameters.IsActive);
    }
}
