using System.Security.Claims;
using GoTransport.Application.Dtos.Account;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IAccountService
{
    Task<JsonResponse<UserTokenDto>> LoginAsync(UserLoginDto request);

    /// <summary>
    /// Checks that the user a validated token was issued to still exists, is active and holds the security stamp the token carries.
    /// </summary>
    Task<bool> IsTokenUserValidAsync(ClaimsPrincipal principal);
}