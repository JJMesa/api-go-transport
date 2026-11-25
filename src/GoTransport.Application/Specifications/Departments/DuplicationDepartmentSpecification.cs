using Ardalis.Specification;

using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Departments;

public class DuplicationDepartmentSpecification : Specification<Department>
{
    public DuplicationDepartmentSpecification(string description, int id)
    {
        // The column's Modern_Spanish_CI_AS collation already compares case-insensitively; calling
        // ToLower on it would only stop SQL Server from using the unique index.
        Query.Where(department => department.Description == description);

        if (id != 0)
            Query.Where(department => department.DepartmentId != id);
    }
}