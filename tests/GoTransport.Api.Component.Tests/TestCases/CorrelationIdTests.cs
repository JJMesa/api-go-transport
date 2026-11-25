using System.Text.Json;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace GoTransport.Api.Component.Tests.TestCases;

public class CorrelationIdTests : ComponentTest
{
    public CorrelationIdTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task EveryResponse_CarriesACorrelationId()
    {
        // Act
        var apiResponse = await Client.GetAsync($"{ApiPaths.HealthPath}/live");

        // Assert: a W3C trace id (32 hex characters)
        var correlationId = apiResponse.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single();
        Assert.Matches("^[0-9a-f]{32}$", correlationId);
    }

    [Fact]
    public async Task ACallerSuppliedId_IsEchoed()
    {
        // Arrange
        Client.DefaultRequestHeaders.Add(CorrelationIdMiddleware.HeaderName, "front-end-req-42");

        // Act
        var apiResponse = await Client.GetAsync($"{ApiPaths.HealthPath}/live");

        // Assert
        Assert.Equal("front-end-req-42", apiResponse.Headers.GetValues(CorrelationIdMiddleware.HeaderName).Single());
    }

    [Fact]
    public async Task AnUnexpectedFailure_ReturnsTheCorrelationIdInTheBody()
    {
        // Arrange: the middlewares in their pipeline order, with a request handler that fails
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers[CorrelationIdMiddleware.HeaderName] = "trace-me";
        var errorHandler = new ErrorHandlerMiddleware(_ => throw new InvalidOperationException("boom"), NullLogger<ErrorHandlerMiddleware>.Instance);
        var pipeline = new CorrelationIdMiddleware(errorHandler.Invoke);

        // Act
        await pipeline.Invoke(context);
        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body);

        // Assert
        Assert.Equal(500, context.Response.StatusCode);
        Assert.Equal("trace-me", body.RootElement.GetProperty("traceId").GetString());
    }
}
