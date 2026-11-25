using System.Security.Claims;
using System.Text.Encodings.Web;
using GoTransport.Application.Commons;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoTransport.Api.Component.Tests.Infrastructure;

/// <summary>
/// Authentication handler used by the component tests in place of the real JWT bearer flow.
/// A request is authenticated only when it carries the <see cref="AuthHeader"/> header, whose value
/// names the Role the signed-in user has (<see cref="NoRole"/> for none). This keeps the suite
/// isolated from real credentials and the JWT signing configuration while still allowing
/// unauthenticated (401) and unauthorized (403) scenarios to be exercised.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    public const string AuthHeader = "X-Test-Auth";
    public const string TestUserId = "1";

    /// <summary>Header value for a signed-in user that has no Role.</summary>
    public const string NoRole = "none";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(AuthHeader, out var role))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, TestUserId),
            new(ClaimTypes.Name, "Component Test User")
        };

        if (role.ToString() != NoRole)
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
