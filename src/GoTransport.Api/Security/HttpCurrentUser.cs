using System.Security.Claims;
using GoTransport.Domain.Interfaces;

namespace GoTransport.Api.Security;

/// <summary>
/// The signed-in user of the current HTTP request: the token's NameIdentifier claim.
/// </summary>
internal sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
