using System.Security.Claims;
using System.Text.Encodings.Web;
using GoTransport.Application.Commons;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoTransport.Api.Component.Tests.Infrastructure;

/// <summary>
/// Authentication handler used by the component tests in place of the real JWT bearer flow.
/// A request is authenticated as an <see cref="Roles.Administrator"/> only when it carries the
/// <see cref="AuthHeader"/> header. This keeps the suite isolated from real credentials and the
/// JWT signing configuration while still allowing unauthenticated scenarios to be exercised.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";
    public const string AuthHeader = "X-Test-Auth";
    private const string TestUserId = "1";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.ContainsKey(AuthHeader))
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, TestUserId),
            new Claim(ClaimTypes.Name, "Component Test User"),
            new Claim(ClaimTypes.Role, Roles.Administrator)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
