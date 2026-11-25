using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoTransport.Application.Wrappers;

namespace GoTransport.Api.Middlewares;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;

        if (response.HasStarted)
        {
            _logger.LogError(exception, "Unhandled exception. {Method} {Path}",
                context.Request.Method, context.Request.Path);
            return;
        }

        var mapping = ExceptionResponseMapper.Map(exception, context.RequestAborted.IsCancellationRequested);

        _logger.Log(mapping.LogLevel, exception, "Request failed with {StatusCode}. {Method} {Path}",
            mapping.StatusCode, context.Request.Method, context.Request.Path);

        response.StatusCode = mapping.StatusCode;

        // The client is gone: there is nobody to write a body for.
        if (mapping.StatusCode == ExceptionResponseMapper.ClientClosedRequest) return;

        if (mapping.StatusCode == (int)HttpStatusCode.ServiceUnavailable)
            response.Headers.RetryAfter = "5";

        response.ContentType = MediaTypeNames.Application.Json;

        var jsonResponse = new JsonResponse<object>
        {
            HttpCode = (HttpStatusCode)mapping.StatusCode,
            Errors = new List<string> { mapping.Message },
            TraceId = CorrelationIdMiddleware.Get(context)
        };

        var result = JsonSerializer.Serialize(jsonResponse,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        await response.WriteAsync(result);
    }
}