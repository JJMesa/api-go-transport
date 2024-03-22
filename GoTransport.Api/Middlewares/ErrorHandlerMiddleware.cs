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

    public ErrorHandlerMiddleware(RequestDelegate next) =>
        _next = next;

    public async Task Invoke(HttpContext context)
    {
        try
        {
            context.Request.EnableBuffering();
            string bodyAsText = await new StreamReader(context.Request.Body).ReadToEndAsync();
            context.Request.Body.Position = 0;
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = MediaTypeNames.Application.Json;

        var jsonResponse = new JsonResponse<object>();

        switch (exception)
        {
            case TaskCanceledException:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                jsonResponse.HttpCode = HttpStatusCode.BadRequest;
                jsonResponse.Errors = new List<string>() { ErrorMessages.CanceledTask };
                break;

            case KeyNotFoundException:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                jsonResponse.HttpCode = HttpStatusCode.NotFound;
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