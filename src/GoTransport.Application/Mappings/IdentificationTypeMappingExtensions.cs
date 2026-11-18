using GoTransport.Application.Dtos.IdentificationType;
using GoTransport.Domain.Entities.Bas;

namespace GoTransport.Application.Mappings;

internal static class IdentificationTypeMappingExtensions
{
    public static IdentificationTypeDto ToDto(this IdentificationType identificationType) => new()
    {
        IdentificationTypeId = identificationType.IdentificationTypeId,
        Description = identificationType.Description
    };
}
