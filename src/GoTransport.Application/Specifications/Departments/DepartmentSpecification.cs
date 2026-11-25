using Ardalis.Specification;
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
        DepartmentFilter.Apply(Query, parameters);
    }
}