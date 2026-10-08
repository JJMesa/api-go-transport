using GoTransport.Application.Dtos.City;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Mappings;

internal static class CityMappingExtensions
{
    /// <summary>
    /// Maps a city to its DTO. A navigation property that was not loaded maps to null.
    /// </summary>
    public static CityDto ToDto(this City city) => new()
    {
        CityId = city.CityId,
        Description = city.Description,
        Department = city.Department?.ToDto()!,
        IsActive = city.IsActive ?? false
    };

    public static City ToEntity(this CityCreationDto cityCreation) => new()
    {
        Description = cityCreation.Description,
        DepartmentId = cityCreation.DepartmentId
    };

    public static void ApplyTo(this CityUpdateDto cityUpdate, City city)
    {
        city.Description = cityUpdate.Description;
        city.DepartmentId = cityUpdate.DepartmentId;
        city.IsActive = cityUpdate.IsActive;
    }
}
