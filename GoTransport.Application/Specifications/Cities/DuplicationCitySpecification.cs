using Ardalis.Specification;

using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Specifications.Cities;

public class DuplicationCitySpecification : Specification<City>
{
    public DuplicationCitySpecification(string description, int departmentId, int id)
    {
        Query.Where(city => city.Description.ToLower().Equals(description.ToLowerInvariant())
                    && city.DepartmentId == departmentId
                    && (city.CityId != id || id == 0));
    }
}