using GoTransport.Api.Component.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Builds the OpenAPI document in-process. The Swagger UI is only served in Development, but the
/// generator is registered in every environment, so a broken Swagger setup still fails here.
/// </summary>
public class SwaggerDocumentTests : ComponentTest
{
    private readonly CustomWebApplicationFactory _factory;

    public SwaggerDocumentTests(CustomWebApplicationFactory factory) : base(factory)
    {
        _factory = factory;
    }

    [Fact]
    public void GetSwagger_DescribesVersionedRoutes()
    {
        // Act
        var document = _factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

        // Assert
        Assert.Contains("/api/v1/cities", document.Paths.Keys);
        Assert.DoesNotContain(document.Paths.Keys, path => path.Contains("{version}"));
    }

    [Fact]
    public void GetSwagger_DeclaresTheBearerScheme()
    {
        // Act
        var document = _factory.Services.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

        // Assert
        Assert.Contains("Bearer", document.Components!.SecuritySchemes!.Keys);
        Assert.NotEmpty(document.Security!);
    }
}
