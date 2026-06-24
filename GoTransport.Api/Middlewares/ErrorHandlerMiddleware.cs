using System.Net;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoTransport.Application.Commons;
using GoTransport.Application.Wrappers;
using Microsoft.Data.SqlClient;

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

        response.ContentType = MediaTypeNames.Application.Json;

        var jsonResponse = new JsonResponse<object>();

        switch (exception)
        {
            case TaskCanceledException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                jsonResponse.HttpCode = HttpStatusCode.BadRequest;
                jsonResponse.Errors = new List<string>() { ErrorMessages.CanceledTask };
                _logger.LogWarning(exception, "Request canceled. {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                jsonResponse.HttpCode = HttpStatusCode.NotFound;
                _logger.LogWarning(exception, "Resource not found. {Method} {Path}",
                    context.Request.Method, context.Request.Path);
                break;

            default:

                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                jsonResponse.HttpCode = HttpStatusCode.InternalServerError;
                jsonResponse.Errors = new List<string>() { ErrorMessages.InternalServerError };

                var sqlEx = exception.InnerException as SqlException;
                if (sqlEx is not null)
                {
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    jsonResponse.HttpCode = HttpStatusCode.BadRequest;
                    jsonResponse.Errors = sqlEx.Number switch
                    {
                        547 => new List<string>() { ErrorMessages.ConflictForeignKey },
                        2601 => new List<string>() { ErrorMessages.ConflictPrimaryKey },
                        _ => new List<string>() { ErrorMessages.InternalServerError },
                    };

                    _logger.LogWarning(exception,
                        "Database conflict (SQL {SqlErrorNumber}). {Method} {Path}",
                        sqlEx.Number, context.Request.Method, context.Request.Path);
                }
                else
                {
                    _logger.LogError(exception, "Unhandled exception. {Method} {Path}",
                        context.Request.Method, context.Request.Path);
                }

                break;
        }

        var result = JsonSerializer.Serialize(jsonResponse,
            new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        await response.WriteAsync(result);
    }
}