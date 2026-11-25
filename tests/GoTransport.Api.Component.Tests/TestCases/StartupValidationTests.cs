using GoTransport.Api.Component.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Settings are validated when the host starts, so a misconfigured API refuses to start instead of
/// failing the first request that reads them.
/// </summary>
public class StartupValidationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public StartupValidationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Theory]
    [InlineData("JwtSettings:Secret", "too-short-for-hs256")]
    [InlineData("JwtSettings:TokenIssuer", "")]
    [InlineData("CachingSettings:SlidingExpiration", "0")]
    [InlineData("LockoutSettings:MaxFailedAccessAttempts", "0")]
    [InlineData("AdminBootstrap:Email", "admin@gotransport.test")]
    public void Startup_WithInvalidSettings_Fails(string key, string value)
    {
        // Arrange
        using var misconfigured = _factory.WithWebHostBuilder(builder => builder.UseSetting(key, value));

        // Act
        var exception = Record.Exception(() => misconfigured.CreateClient());

        // Assert
        Assert.IsType<OptionsValidationException>(exception);
    }

    [Fact]
    public void Startup_WithValidSettings_Succeeds()
    {
        // Act
        var exception = Record.Exception(() => _factory.CreateClient());

        // Assert
        Assert.Null(exception);
    }
}
