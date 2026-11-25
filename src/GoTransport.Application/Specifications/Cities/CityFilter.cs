using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Cities;

/// <summary>
/// The filters a page of City rows and its total count share, so the page and the count always agree.
/// </summary>
internal static class CityFilter
{
    public static void Apply(ISpecificationBuilder<City> query, CityParameters parameters)
    {
        if (parameters.DepartmentId.HasValue)
            query.Where(city => city.DepartmentId == parameters.DepartmentId.Value);

        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
            query.Where(city => city.Description.Contains(parameters.SearchCriteria));

        if (parameters.IsActive.HasValue)
            query.Where(city => city.IsActive == parameters.IsActive);
    }
}
