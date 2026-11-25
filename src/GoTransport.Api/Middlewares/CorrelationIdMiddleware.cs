using System.Diagnostics;
using System.Text.RegularExpressions;
using Serilog.Context;

namespace GoTransport.Api.Middlewares;

/// <summary>
/// Gives every request one id that appears in the <c>X-Correlation-Id</c> response header, in every log
/// line written while handling it (the <c>CorrelationId</c> property) and in error bodies. It is the W3C
/// trace id of the request's Activity, so an incoming <c>traceparent</c> header links the request to the
/// caller's trace; a caller may also supply its own id in <c>X-Correlation-Id</c>.
/// </summary>
public partial class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = IncomingId(context) ?? Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        context.Items[ItemKey] = correlationId;
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using (LogContext.PushProperty(ItemKey, correlationId))
            await _next(context);
    }

    /// <summary>The correlation id of the current request, for error bodies.</summary>
    public static string? Get(HttpContext context) => context.Items[ItemKey] as string;

    private static string? IncomingId(HttpContext context)
    {
        var value = context.Request.Headers[HeaderName].ToString();

        // Only short, plain ids are echoed into headers and logs.
        return value.Length is > 0 and <= 64 && SafeId().IsMatch(value) ? value : null;
    }

    [GeneratedRegex("^[A-Za-z0-9._-]+$")]
    private static partial Regex SafeId();
}
