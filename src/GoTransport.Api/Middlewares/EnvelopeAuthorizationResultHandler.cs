using System.Net;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GoTransport.Api.Middlewares;

/// <summary>
/// Renders authorization failures in the API's response envelope, with the same JSON options as the
/// controllers: 401 when the caller is not signed in (or the token was rejected), 403 when the signed-in
/// user lacks the required Role. It works for any authentication scheme, so tests see the real bodies.
/// </summary>
internal sealed class EnvelopeAuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Challenged)
        {
            context.Response.Headers.WWWAuthenticate = "Bearer";
            var response = ResponseBuilder<object>.Unauthorized(ErrorMessages.Unauthorized);

            // The JWT handler ignores a token sent without the "Bearer " prefix; say so instead of a bare 401.
            if (HasNonBearerAuthorization(context.Request))
                response.Errors = [.. response.Errors ?? [], ErrorMessages.MissingBearerScheme];

            await WriteAsync(context, response);
            return;
        }

        if (authorizeResult.Forbidden)
        {
            await WriteAsync(context, ResponseBuilder<object>.Forbidden(ErrorMessages.Forbidden));
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    private static bool HasNonBearerAuthorization(HttpRequest request)
    {
        var authorization = request.Headers.Authorization.ToString();
        return authorization.Length > 0 && !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);
    }

    private static Task WriteAsync(HttpContext context, JsonResponse<object> response)
    {
        var jsonOptions = context.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
        context.Response.StatusCode = (int)response.HttpCode;
        return context.Response.WriteAsJsonAsync(response, jsonOptions);
    }
}
