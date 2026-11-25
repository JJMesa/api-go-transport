using Ardalis.Specification;

using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Cities;

public class DuplicationCitySpecification : Specification<City>
{
    public DuplicationCitySpecification(string description, int departmentId, int id)
    {
        // The column's Modern_Spanish_CI_AS collation already compares case-insensitively; calling
        // ToLower on it would only stop SQL Server from using the unique index.
        Query.Where(city => city.DepartmentId == departmentId && city.Description == description);

        if (id != 0)
            Query.Where(city => city.CityId != id);
    }
}