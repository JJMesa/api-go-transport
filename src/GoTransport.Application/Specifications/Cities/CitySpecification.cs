using Ardalis.Specification;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Cities;

public class CitySpecification : Specification<City>
{
    public CitySpecification(int? cityId = null, int? departmentId = null)
    {
        Query.Where(city => city.DepartmentId == departmentId || departmentId == null);
        Query.Where(city => city.CityId == cityId || cityId == null);
        Query.Where(City => City.IsActive!.Value);

        Query.OrderBy(city => city.Description);

        Query.Include(city => city.Department);
    }

    public CitySpecification(CityParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
            Query.Where(city => city.Description.Contains(parameters.SearchCriteria));

        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(city => city.Description);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(city => city.Description);
                break;

            default:
                Query.OrderBy(city => city.Description);
                break;
        }

        if (parameters.IsActive.HasValue)
            Query.Where(city => city.IsActive == parameters.IsActive);
    }
}