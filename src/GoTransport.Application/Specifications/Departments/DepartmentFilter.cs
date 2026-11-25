using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Departments;

/// <summary>
/// The filters a page of Department rows and its total count share, so the page and the count always agree.
/// </summary>
internal static class DepartmentFilter
{
    public static void Apply(ISpecificationBuilder<Department> query, DepartmentParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
            query.Where(department => department.Description.Contains(parameters.SearchCriteria));

        if (parameters.IsActive.HasValue)
            query.Where(department => department.IsActive == parameters.IsActive);
    }
}
