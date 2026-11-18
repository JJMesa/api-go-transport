using GoTransport.Application.Dtos.Manufacturer;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Mappings;

internal static class ManufacturerMappingExtensions
{
    public static ManufacturerDto ToDto(this Manufacturer manufacturer) => new()
    {
        ManufacturerId = manufacturer.ManufacturerId,
        Description = manufacturer.Description
    };
}
