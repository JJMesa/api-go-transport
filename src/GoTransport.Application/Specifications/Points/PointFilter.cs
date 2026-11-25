using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Points;

/// <summary>
/// The filters a page of Point rows and its total count share, so the page and the count always agree.
/// </summary>
internal static class PointFilter
{
    public static void Apply(ISpecificationBuilder<Point> query, PointParameters parameters)
    {
        if (parameters.IsActive.HasValue)
            query.Where(point => point.IsActive == parameters.IsActive);
    }
}
