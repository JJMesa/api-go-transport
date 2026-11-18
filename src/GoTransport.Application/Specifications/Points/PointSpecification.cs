using Ardalis.Specification;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Points;

public class PointSpecification : Specification<Point>
{
    public PointSpecification()
    {
        Query.Where(point => point.IsActive!.Value);

        Query.Include(point => point.City);

        Query.OrderBy(point => point.City.Description);
    }

    public PointSpecification(int id)
    {
        Query.Where(point => point.PointId == id);

        Query.Include(point => point.City);
    }

    public PointSpecification(PointParameters parameters)
    {
        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(point => point.Detail);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(point => point.Detail);
                break;

            default:
                Query.OrderBy(point => point.Detail);
                break;
        }

        if (parameters.IsActive.HasValue)
            Query.Where(point => point.IsActive == parameters.IsActive);
    }
}