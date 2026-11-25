using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Account;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Issues real tokens through the login endpoint and presents them to the real JWT bearer handler:
/// signing and validation keys, lifetime, role claims and the per-request user check.
/// </summary>
public class JwtRoundTripTests : IClassFixture<JwtWebApplicationFactory>
{
    private const string Password = "Transporte-2026";

    private readonly JwtWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public JwtRoundTripTests(JwtWebApplicationFactory factory)
    {
        factory.ResetDatabase();
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ATokenFromLogin_OpensTheEndpointsOfItsRole()
    {
        // Arrange
        await CreateUserAsync("admin@gotransport.test", Roles.Administrator);
        var token = await LoginAsync("admin@gotransport.test");

        // Act
        var withToken = await GetCitiesAsync(token);
        var withoutToken = await GetCitiesAsync(token: null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, withToken.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, withoutToken.StatusCode);
    }

    [Fact]
    public async Task AnOperatorToken_IsForbiddenOnAdministratorEndpoints()
    {
        // Arrange
        await CreateUserAsync("operator@gotransport.test", Roles.Operator);
        var token = await LoginAsync("operator@gotransport.test");

        // Act
        var apiResponse = await GetCitiesAsync(token);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, apiResponse.StatusCode);
    }

    [Fact]
    public async Task ATokenStopsWorking_OnceItsUserIsDeactivated()
    {
        // Arrange
        await CreateUserAsync("admin@gotransport.test", Roles.Administrator);
        var token = await LoginAsync("admin@gotransport.test");

        // Act: deactivating a user rotates the security stamp, as UserService.UpdateAsync does
        using (var scope = _factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = (await userManager.FindByEmailAsync("admin@gotransport.test"))!;
            user.IsActive = false;
            await userManager.UpdateSecurityStampAsync(user);
        }

        var apiResponse = await GetCitiesAsync(token);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, apiResponse.StatusCode);
    }

    [Fact]
    public async Task ATamperedToken_IsRejected()
    {
        // Arrange
        await CreateUserAsync("admin@gotransport.test", Roles.Administrator);
        var token = await LoginAsync("admin@gotransport.test");
        var tampered = token[..^4] + (token.EndsWith("AAAA") ? "BBBB" : "AAAA");

        // Act
        var apiResponse = await GetCitiesAsync(tampered);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, apiResponse.StatusCode);
    }

    [Fact]
    public async Task ATokenWithoutTheBearerPrefix_IsRejectedWithAHint()
    {
        // Arrange: what Swagger UI sent before F-68 when only the token was pasted
        await CreateUserAsync("admin@gotransport.test", Roles.Administrator);
        var token = await LoginAsync("admin@gotransport.test");
        var request = new HttpRequestMessage(HttpMethod.Get, ApiPaths.CitiesFullPath);
        request.Headers.TryAddWithoutValidation("Authorization", token);

        // Act
        var apiResponse = await _client.SendAsync(request);
        var actual = JsonConvert.DeserializeObject<JsonResponse<object>>(await apiResponse.Content.ReadAsStringAsync());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, apiResponse.StatusCode);
        Assert.Equal([ErrorMessages.Unauthorized, ErrorMessages.MissingBearerScheme], actual!.Errors);
    }

    [Fact]
    public async Task NoToken_IsRejectedWithoutTheHint()
    {
        // Act
        var apiResponse = await GetCitiesAsync(null);
        var actual = JsonConvert.DeserializeObject<JsonResponse<object>>(await apiResponse.Content.ReadAsStringAsync());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, apiResponse.StatusCode);
        Assert.Equal([ErrorMessages.Unauthorized], actual!.Errors);
    }

    private async Task CreateUserAsync(string email, string role)
    {
        using var scope = _factory.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new Role { Name = role });

        var user = new User { Email = email, UserName = email, FirstName = "Test", LastName = role };
        var created = await userManager.CreateAsync(user, Password);
        Assert.True(created.Succeeded, string.Join(" ", created.Errors.Select(e => e.Description)));
        await userManager.AddToRoleAsync(user, role);
    }

    private async Task<string> LoginAsync(string email)
    {
        var body = new StringContent(JsonConvert.SerializeObject(new UserLoginDto { Email = email, Password = Password }),
            Encoding.UTF8, MediaTypeNames.Application.Json);
        var apiResponse = await _client.PostAsync(ApiPaths.AuthBasePath, body);
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        var actual = JsonConvert.DeserializeObject<JsonResponse<UserTokenDto>>(await apiResponse.Content.ReadAsStringAsync());
        return actual!.Data!.Token;
    }

    private Task<HttpResponseMessage> GetCitiesAsync(string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, ApiPaths.CitiesFullPath);
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return _client.SendAsync(request);
    }
}
