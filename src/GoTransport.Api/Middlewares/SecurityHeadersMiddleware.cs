namespace GoTransport.Api.Middlewares;

/// <summary>
/// Adds browser hardening headers to every response. The API only serves JSON, so nothing may be framed,
/// sniffed as another type, or load resources; the Swagger UI (Development only) is left out of the
/// content policy because it needs its own scripts and styles.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers.XContentTypeOptions = "nosniff";
        headers.XFrameOptions = "DENY";
        headers["Referrer-Policy"] = "no-referrer";

        if (!context.Request.Path.StartsWithSegments("/swagger"))
            headers.ContentSecurityPolicy = "default-src 'none'; frame-ancestors 'none'";

        return _next(context);
    }
}
