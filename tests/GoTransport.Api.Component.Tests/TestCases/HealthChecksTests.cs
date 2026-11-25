using System.Net;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Test.Utilities.Commons;

namespace GoTransport.Api.Component.Tests.TestCases;

public class HealthChecksTests : ComponentTest
{
    public HealthChecksTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Theory]
    [InlineData("/live")]
    [InlineData("/ready")]
    [InlineData("")]
    public async Task AnonymousProbes_ReportOnlyTheStatus(string probe)
    {
        // Act
        var apiResponse = await Client.GetAsync($"{ApiPaths.HealthPath}{probe}");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Equal("{\"status\":\"Healthy\"}", jsonResponse);
    }

    [Fact]
    public async Task Details_WithoutAuthentication_Unauthorized()
    {
        // Act
        var apiResponse = await Client.GetAsync($"{ApiPaths.HealthPath}/details");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, apiResponse.StatusCode);
        Assert.DoesNotContain("database", jsonResponse);
    }

    [Fact]
    public async Task Details_AsAdministrator_ListsEachCheck()
    {
        // Act
        await AddAuthorization();
        var apiResponse = await Client.GetAsync($"{ApiPaths.HealthPath}/details");
        var jsonResponse = await apiResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, apiResponse.StatusCode);
        Assert.Contains("\"name\":\"database\"", jsonResponse);
    }
}
