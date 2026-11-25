using System.Net;
using System.Text.Json;
using GoTransport.Api.Middlewares;
using GoTransport.Application.Commons;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Runs <see cref="ErrorHandlerMiddleware"/> on a bare <see cref="DefaultHttpContext"/> for the paths a request
/// through the host cannot reach on demand: a failure after the response has started, and the headers of a
/// 503. Which status each exception gets is covered by <c>ExceptionResponseMapperTests</c>.
/// </summary>
public class ErrorHandlerMiddlewareTests
{
    private readonly ListLogger _logger = new();

    [Fact]
    public async Task Invoke_ShouldAnswer503WithRetryAfterWhenTheDatabaseIsUnavailable()
    {
        // Arrange
        var context = NewContext();

        // Act
        await Run(context, new RetryLimitExceededException("Retries exhausted."));

        // Assert
        Assert.Equal((int)HttpStatusCode.ServiceUnavailable, context.Response.StatusCode);
        Assert.Equal("5", context.Response.Headers.RetryAfter.ToString());
        Assert.Contains(ErrorMessages.DatabaseUnavailable, await BodyOf(context));
    }

    [Fact]
    public async Task Invoke_ShouldAnswer500WithoutRetryAfterForAnUnexpectedError()
    {
        // Arrange
        var context = NewContext();

        // Act
        await Run(context, new InvalidOperationException("Unexpected."));

        // Assert
        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.False(context.Response.Headers.ContainsKey("Retry-After"));
        Assert.Contains(ErrorMessages.InternalServerError, await BodyOf(context));
    }

    [Fact]
    public async Task Invoke_ShouldOnlyLogWhenTheResponseHasAlreadyStarted()
    {
        // Arrange: part of the body is already on the wire, so status and headers can no longer change
        var context = NewContext();
        context.Features.Set<IHttpResponseFeature>(new StartedResponse());
        var exception = new InvalidOperationException("Failed while streaming.");

        // Act
        await Run(context, exception);

        // Assert
        Assert.Equal((int)HttpStatusCode.OK, context.Response.StatusCode);
        Assert.Empty(await BodyOf(context));
        var entry = Assert.Single(_logger.Entries);
        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Same(exception, entry.Exception);
    }

    private Task Run(HttpContext context, Exception exception) =>
        new ErrorHandlerMiddleware(_ => throw exception, _logger).Invoke(context);

    private static DefaultHttpContext NewContext()
    {
        var context = new DefaultHttpContext();
        context.Request.Method = HttpMethods.Get;
        context.Request.Path = "/api/v1/cities";
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static async Task<string> BodyOf(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        if (body.Length > 0) JsonDocument.Parse(body).Dispose();
        return body;
    }

    private sealed class StartedResponse : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = (int)HttpStatusCode.OK;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = Stream.Null;
        public bool HasStarted => true;
        public void OnStarting(Func<object, Task> callback, object state) { }
        public void OnCompleted(Func<object, Task> callback, object state) { }
    }

    private sealed class ListLogger : ILogger<ErrorHandlerMiddleware>
    {
        public List<(LogLevel Level, Exception? Exception)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, exception));
    }
}
