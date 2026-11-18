using Ardalis.Specification;

using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Departments;

public class DuplicationDepartmentSpecification : Specification<Department>
{
    public DuplicationDepartmentSpecification(string description, int id)
    {
        Query.Where(department => department.Description.ToLower().Equals(description.ToLowerInvariant())
            && (department.DepartmentId != id || id == 0));
    }
}