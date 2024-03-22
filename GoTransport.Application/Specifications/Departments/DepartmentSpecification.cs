using Ardalis.Specification;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Departments;

public class DepartmentSpecification : Specification<Department>
{
    public DepartmentSpecification()
    {
        Query.Where(department => department.IsActive!.Value);

        Query.OrderBy(department => department.Description);
    }

    public DepartmentSpecification(DepartmentParameters parameters)
    {
        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
            Query.Where(department => department.Description.Contains(parameters.SearchCriteria));

        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(department => department.Description);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(department => department.Description);
                break;

            default:
                Query.OrderBy(department => department.Description);
                break;
        }

        if (parameters.IsActive.HasValue)
            Query.Where(department => department.IsActive == parameters.IsActive);
    }
}