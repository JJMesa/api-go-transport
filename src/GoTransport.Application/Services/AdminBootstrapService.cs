using GoTransport.Application.Attributes;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Enums;
using GoTransport.Application.Extensions;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Settings;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoTransport.Application.Services;

[Transient]
internal class AdminBootstrapService : IAdminBootstrapService
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly AdminBootstrapSettings _settings;
    private readonly ILogger<AdminBootstrapService> _logger;

    public AdminBootstrapService(UserManager<User> userManager
        , RoleManager<Role> roleManager
        , IOptions<AdminBootstrapSettings> settings
        , ILogger<AdminBootstrapService> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<JsonResponse<AdminBootstrapResult>> EnsureAdministratorAsync()
    {
        if (!_settings.IsConfigured)
        {
            _logger.LogInformation("Administrator bootstrap skipped: AdminBootstrap:Email and AdminBootstrap:Password are not configured");
            return ResponseBuilder<AdminBootstrapResult>.Ok(AdminBootstrapResult.NotConfigured);
        }

        var administrators = await _userManager.GetUsersInRoleAsync(Roles.Administrator);
        if (administrators.Count > 0)
        {
            _logger.LogInformation("Administrator bootstrap skipped: a {Role} user already exists; the AdminBootstrap settings can be removed", Roles.Administrator);
            return ResponseBuilder<AdminBootstrapResult>.Ok(AdminBootstrapResult.AlreadyExists);
        }

        if (!await _roleManager.RoleExistsAsync(Roles.Administrator))
            return ResponseBuilder<AdminBootstrapResult>.BadRequest(ErrorMessages.AdministratorRoleMissing);

        var administrator = new User
        {
            FirstName = _settings.FirstName,
            LastName = _settings.LastName,
            Email = _settings.Email,
            UserName = _settings.Email
        };

        var creation = await _userManager.CreateAsync(administrator, _settings.Password);
        if (!creation.Succeeded)
            return ResponseBuilder<AdminBootstrapResult>.BadRequest(creation.Errors.ToErrorList());

        var roleAssignment = await _userManager.AddToRoleAsync(administrator, Roles.Administrator);
        if (!roleAssignment.Succeeded)
            return ResponseBuilder<AdminBootstrapResult>.BadRequest(roleAssignment.Errors.ToErrorList());

        _logger.LogInformation("Administrator bootstrap created the first {Role} user", Roles.Administrator);
        return ResponseBuilder<AdminBootstrapResult>.Created(AdminBootstrapResult.Created);
    }
}
