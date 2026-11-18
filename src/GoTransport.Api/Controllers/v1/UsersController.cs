using System.Text.Json;
using Asp.Versioning;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.User;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Parameters;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoTransport.Api.Controllers.v1;

[Authorize(Roles = Roles.Administrator)]
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    //GET: api/v1/users
    [HttpGet]
    public async Task<ActionResult<JsonResponse<IEnumerable<UserDto>>>> GetAsync([FromQuery] UserParameters parameters, CancellationToken cancellationToken)
    {
        var response = await _userService.GetAsync(parameters, cancellationToken);
        Response.Headers.Add(Constants.PaginationHeader, JsonSerializer.Serialize(response.Metadata));
        return StatusCode((int)response.HttpCode, response);
    }

    //GET: api/v1/users/1
    [HttpGet("{id:long}")]
    public async Task<ActionResult<JsonResponse<UserDto>>> GetById(long id, CancellationToken cancellationToken)
    {
        var response = await _userService.GetByIdAsync(id, cancellationToken);
        return StatusCode((int)response.HttpCode, response);
    }

    //POST: api/v1/users
    [HttpPost]
    public async Task<ActionResult<JsonResponse<UserDto>>> Create([FromBody] UserCreationDto userCreationDto)
    {
        var response = await _userService.CreateAsync(userCreationDto);
        return StatusCode((int)response.HttpCode, response);
    }

    //PUT: api/v1/users/1
    [HttpPut("{id:long}")]
    public async Task<ActionResult<JsonResponse<UserDto>>> Update(long id, [FromBody] UserUpdateDto userUpdateDto)
    {
        var response = await _userService.UpdateAsync(id, userUpdateDto);
        return StatusCode((int)response.HttpCode, response);
    }
}