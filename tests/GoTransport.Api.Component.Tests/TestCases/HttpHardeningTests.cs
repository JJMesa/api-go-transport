using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Test.Utilities.Commons;
using Microsoft.AspNetCore.Hosting;

namespace GoTransport.Api.Component.Tests.TestCases;

public class HttpHardeningTests : IClassFixture<CustomWebApplicationFactory>
{
    private const string FrontEnd = "https://app.gotransport.test";

    private readonly CustomWebApplicationFactory _factory;

    public HttpHardeningTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Responses_CarryTheSecurityHeaders()
    {
        // Act
        var apiResponse = await _factory.CreateClient().GetAsync($"{ApiPaths.HealthPath}/live");

        // Assert
        Assert.Equal("nosniff", apiResponse.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", apiResponse.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("no-referrer", apiResponse.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("frame-ancestors 'none'", apiResponse.Headers.GetValues("Content-Security-Policy").Single());
    }

    [Fact]
    public async Task Cors_WithoutConfiguredOrigins_AllowsNoBrowserOrigin()
    {
        // Act: outside Development, an unconfigured API trusts no origin
        var apiResponse = await _factory.CreateClient().SendAsync(Preflight());

        // Assert
        Assert.False(apiResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task Cors_WithAConfiguredOrigin_AllowsOnlyThatOrigin()
    {
        // Arrange
        using var factory = _factory.WithWebHostBuilder(builder => builder.UseSetting("Cors:AllowedOrigins:0", FrontEnd));
        var client = factory.CreateClient();

        // Act
        var allowed = await client.SendAsync(Preflight(FrontEnd));
        var other = await client.SendAsync(Preflight("https://evil.test"));

        // Assert
        Assert.Equal(FrontEnd, allowed.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.False(other.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static HttpRequestMessage Preflight(string origin = FrontEnd)
    {
        var request = new HttpRequestMessage(HttpMethod.Options, ApiPaths.CitiesFullPath);
        request.Headers.Add("Origin", origin);
        request.Headers.Add("Access-Control-Request-Method", "GET");
        return request;
    }
}
