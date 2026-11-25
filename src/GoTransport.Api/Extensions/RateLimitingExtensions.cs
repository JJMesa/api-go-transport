using System.Threading.RateLimiting;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GoTransport.Api.Extensions;

public static class RateLimitingExtensions
{
    /// <summary>Policy name for the sign-in endpoint.</summary>
    public const string LoginPolicy = "login";

    /// <summary>Sign-in attempts allowed per client IP address in each one-minute window.</summary>
    public const int LoginPermitLimit = 10;

    /// <summary>
    /// Limits sign-in attempts per client IP, so password guessing and deliberate lockouts are slowed
    /// down whether or not the email exists. Rejections use the API's response envelope (429).
    /// Behind a reverse proxy, forward the client IP (ForwardedHeaders) or every caller shares one bucket.
    /// </summary>
    public static IServiceCollection AddRateLimitingConfiguration(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = (context, cancellationToken) =>
            {
                var jsonOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions;
                context.HttpContext.Response.Headers.RetryAfter = "60";
                return new ValueTask(context.HttpContext.Response.WriteAsJsonAsync(
                    ResponseBuilder<object>.TooManyRequests(ErrorMessages.TooManyLoginAttempts), jsonOptions, cancellationToken));
            };

            options.AddPolicy(LoginPolicy, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = LoginPermitLimit,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        });

        return services;
    }
}
