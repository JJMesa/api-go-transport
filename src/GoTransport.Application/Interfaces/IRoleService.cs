using GoTransport.Application.Dtos.Role;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;

namespace GoTransport.Application.Interfaces;

public interface IRoleService
{
    Task<IdentityResult> AssignRolesToUser(User user, long roleId, CancellationToken cancellationToken);

    Task<bool> RoleExistsAsync(long roleId, CancellationToken cancellationToken);

    Task<JsonResponse<IEnumerable<RoleDto>>> GetAllAsync(CancellationToken cancellationToken);
}