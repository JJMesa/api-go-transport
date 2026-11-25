using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Points;

public class PointSpecification : Specification<Point>
{
    public PointSpecification()
    {
        Query.Where(point => point.IsActive);

        Query.Include(point => point.City);

        Query.OrderBy(point => point.City.Description);

        // Read-only: nothing loaded here is saved back, so skip change tracking.
        Query.AsNoTracking();
    }

    public PointSpecification(int id)
    {
        Query.Where(point => point.PointId == id);

        Query.Include(point => point.City);
    }

    public PointSpecification(PointParameters parameters)
    {
        PointFilter.Apply(Query, parameters);
    }
}