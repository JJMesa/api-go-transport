using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Cities;

public class CitySpecification : Specification<City>
{
    public CitySpecification(int? cityId = null, int? departmentId = null)
    {
        // Add a filter only when it is given, so the SQL has no "OR @p IS NULL" branch.
        if (departmentId.HasValue)
            Query.Where(city => city.DepartmentId == departmentId.Value);

        if (cityId.HasValue)
            Query.Where(city => city.CityId == cityId.Value);
        Query.Where(City => City.IsActive);

        Query.OrderBy(city => city.Description);

        Query.Include(city => city.Department);

        // Read-only: nothing loaded here is saved back, so skip change tracking.
        Query.AsNoTracking();
    }

    public CitySpecification(CityParameters parameters)
    {
        CityFilter.Apply(Query, parameters);
    }
}