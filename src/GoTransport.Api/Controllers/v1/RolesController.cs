using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Role;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoTransport.Api.Controllers.v1;

[Authorize(Roles = Roles.Administrator)]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    //GET: api/v1/roles/all
    [HttpGet("all")]
    public async Task<ActionResult<JsonResponse<IEnumerable<RoleDto>>>> GetAllRoles(CancellationToken cancellationToken)
    {
        var response = await _roleService.GetAllAsync(cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }
}