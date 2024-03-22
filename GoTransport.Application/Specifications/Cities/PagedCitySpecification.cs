using Ardalis.Specification;
using GoTransport.Application.Dtos.City;
using GoTransport.Application.Dtos.Department;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Cities;

public class PagedCitySpecification : Specification<City, CityDto>
{
    public PagedCitySpecification(CityParameters parameters)
    {
        if (parameters.DepartmentId.HasValue)
            Query.Where(city => city.DepartmentId == parameters.DepartmentId.Value);

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

        Query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);

        Query.Select(city => new CityDto
        {
            CityId = city.CityId,
            Description = city.Description,
            Department = new DepartmentDto
            {
                DepartmentId = city.Department.DepartmentId,
                Description = city.Department.Description,
                IsActive = city.Department.IsActive!.Value
            },
            IsActive = city.IsActive!.Value
        });
    }
}