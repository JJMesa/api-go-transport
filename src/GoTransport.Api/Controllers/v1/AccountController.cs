using Asp.Versioning;
using GoTransport.Api.Extensions;
using GoTransport.Application.Dtos.Account;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GoTransport.Api.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    //POST: api/v1/account/login
    [AllowAnonymous]
    [EnableRateLimiting(RateLimitingExtensions.LoginPolicy)]
    [HttpPost("login")]
    public async Task<ActionResult<JsonResponse<UserTokenDto>>> Login([FromBody] UserLoginDto userLoginDto)
    {
        var response = await _accountService.LoginAsync(userLoginDto);
        return StatusCode((int)response.HttpCode, response);
    }
}