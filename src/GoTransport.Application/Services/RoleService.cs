using System.Data;
using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Role;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Mappings;
using GoTransport.Application.Specifications.Roles;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;

namespace GoTransport.Application.Services;

[Transient]
internal class RoleService : IRoleService
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<Role> _roleRepository;
    private readonly ICacheService<Role> _cacheService;

    public RoleService(UserManager<User> userManager
        , IRepository<Role> roleRepository
        , ICacheService<Role> cacheService)
    {
        _userManager = userManager;
        _roleRepository = roleRepository;
        _cacheService = cacheService;
    }

    public async Task<JsonResponse<IEnumerable<RoleDto>>> GetAllAsync(CancellationToken cancellationToken)
    {
        var roles = _cacheService.Get(CacheKey.Roles);
        if (roles is null)
        {
            roles = await _roleRepository.ListAsync(cancellationToken);
            _cacheService.Set(CacheKey.Roles, roles);
        }

        return ResponseBuilder<IEnumerable<RoleDto>>.Ok(roles.Select(role => role.ToDto()).ToList());
    }

    public async Task<IdentityResult> AssignRolesToUser(User user, long roleId)
    {
        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Any())
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded) return removeResult;
        }

        var roles = await _roleRepository.ListAsync(new RoleSpecification(roleId));
        var roleNames = roles.Select(role => role.Name).Distinct();
        return await _userManager.AddToRolesAsync(user, roleNames);
    }
}