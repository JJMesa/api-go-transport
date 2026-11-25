using System.Net;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Application.Commons;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Calls every controller endpoint as an anonymous caller, a user without a Role, an Operator and an
/// Administrator, and checks each one is let through (not 401/403) or stopped (401 anonymous, 403 signed
/// in) according to the access table below. A new endpoint is covered automatically; its expected access
/// comes from <see cref="AccessOf"/>.
/// </summary>
public partial class AuthorizationMatrixTests : ComponentTest
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthorizationMatrixTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _factory = factory;
    }

    private enum Access { Anonymous, AdministratorOrOperator, Administrator }

    /// <summary>The intended access of each endpoint: the source of truth this test holds the API to.</summary>
    private static Access AccessOf(string method, string path) => (method, path) switch
    {
        ("POST", "/api/v1/account/login") => Access.Anonymous,
        ("GET", "/api/v1/identification-types/all") => Access.Anonymous,
        ("GET", "/api/v1/routes") => Access.Anonymous,
        _ when path.StartsWith("/api/v1/reservations") || path.EndsWith("/reservations") => Access.AdministratorOrOperator,
        _ => Access.Administrator
    };

    [Fact]
    public async Task EveryEndpoint_AdmitsExactlyItsRoles()
    {
        var endpoints = _factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .OfType<RouteEndpoint>()
            .Where(endpoint => endpoint.Metadata.GetMetadata<ControllerActionDescriptor>() is not null)
            .ToList();
        var failures = new List<string>();

        foreach (var endpoint in endpoints)
        {
            var method = endpoint.Metadata.GetMetadata<HttpMethodMetadata>()!.HttpMethods.Single();
            // Templates keep the controller name as declared ("Account"); routes are matched case-insensitively.
            var path = Concrete(endpoint.RoutePattern.RawText!).ToLowerInvariant();
            var access = AccessOf(method, path);

            foreach (var caller in new string?[] { null, TestAuthHandler.NoRole, Roles.Operator, Roles.Administrator })
            {
                var status = await SendAsync(method, path, caller);
                var admitted = access switch
                {
                    Access.Anonymous => true,
                    Access.AdministratorOrOperator => caller is Roles.Administrator or Roles.Operator,
                    _ => caller is Roles.Administrator
                };
                var expectedRejection = caller is null ? HttpStatusCode.Unauthorized : HttpStatusCode.Forbidden;

                if (admitted && status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    failures.Add($"{method} {path} as {caller ?? "anonymous"}: expected access, got {(int)status}");
                else if (!admitted && status != expectedRejection)
                    failures.Add($"{method} {path} as {caller ?? "anonymous"}: expected {(int)expectedRejection}, got {(int)status}");
            }
        }

        Assert.True(endpoints.Count > 40, $"only {endpoints.Count} endpoints found");
        Assert.Empty(failures);
    }

    private async Task<HttpStatusCode> SendAsync(string method, string path, string? caller)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), path);
        if (method is "POST" or "PUT")
            request.Content = new StringContent("{}", Encoding.UTF8, MediaTypeNames.Application.Json);
        if (caller is not null)
            request.Headers.Add(TestAuthHandler.AuthHeader, caller);

        using var response = await Client.SendAsync(request);
        return response.StatusCode;
    }

    /// <summary>Fills a route template with values that satisfy its constraints.</summary>
    private static string Concrete(string template) =>
        "/" + RouteParameter().Replace(template.TrimStart('/'), match => match.Groups["constraint"].Value switch
        {
            "apiVersion" => "1",
            "guid" => DatabaseSeeder.ScheduleId.ToString(),
            _ => "1"
        });

    [GeneratedRegex(@"\{(?<name>\w+)(?::(?<constraint>\w+))?\}")]
    private static partial Regex RouteParameter();
}
