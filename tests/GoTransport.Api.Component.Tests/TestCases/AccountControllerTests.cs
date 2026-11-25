using System.Net;
using System.Net.Mime;
using System.Text;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Extensions;
using GoTransport.Api.Test.Utilities.Commons;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.Account;
using Newtonsoft.Json;

namespace GoTransport.Api.Component.Tests.TestCases;

public class AccountControllerTests : ComponentTest
{
    public AccountControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Login_TooManyAttemptsFromOneClient_TooManyRequests()
    {
        // Arrange: the limit applies whether or not the email exists.
        var login = new StringContent(
            JsonConvert.SerializeObject(new UserLoginDto { Email = "nobody@gotransport.test", Password = "Wrong-Password-1" }),
            Encoding.UTF8, MediaTypeNames.Application.Json);

        var statuses = new List<HttpStatusCode>();
        for (var attempt = 0; attempt < RateLimitingExtensions.LoginPermitLimit; attempt++)
            statuses.Add((await Client.PostAsync(ApiPaths.AuthBasePath, login)).StatusCode);

        // Act
        var apiResponse = await Client.PostAsync(ApiPaths.AuthBasePath, login);
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.All(statuses, status => Assert.Equal(HttpStatusCode.BadRequest, status));
        Assert.Equal(HttpStatusCode.TooManyRequests, apiResponse.StatusCode);
        Assert.Contains(ErrorMessages.TooManyLoginAttempts, jsonResponse);
    }
}
