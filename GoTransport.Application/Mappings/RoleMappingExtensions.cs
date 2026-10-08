using GoTransport.Application.Dtos.Role;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

internal static class RoleMappingExtensions
{
    public static RoleDto ToDto(this Role role) => new()
    {
        RoleId = (int)role.Id,
        Name = role.Name
    };
}
