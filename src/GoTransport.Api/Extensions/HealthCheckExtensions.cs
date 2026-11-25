using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using GoTransport.Application.Commons;
using GoTransport.Persistence.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace GoTransport.Api.Extensions;

public static class HealthCheckExtensions
{
    private const string DatabaseCheckName = "database";

    /// <summary>Checks that must pass before the API can serve traffic.</summary>
    private const string ReadinessTag = "ready";

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Registers the application's health checks, including database connectivity.
    /// </summary>
    public static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>(DatabaseCheckName, tags: new[] { ReadinessTag });

        return services;
    }

    /// <summary>
    /// Exposes the health endpoints. The anonymous ones report only an overall status, because check
    /// errors can reveal server or login names:
    /// <list type="bullet">
    /// <item><c>/health/live</c> — the process is up; runs no checks (liveness probe).</item>
    /// <item><c>/health/ready</c> and <c>/health</c> — the dependencies (the database) answer (readiness probe).</item>
    /// <item><c>/health/details</c> — every check with its duration and error, for Administrators only.</item>
    /// </list>
    /// </summary>
    public static WebApplication MapHealthChecksConfiguration(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteStatusOnly
        });

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadinessTag),
            ResponseWriter = WriteStatusOnly
        });

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(ReadinessTag),
            ResponseWriter = WriteStatusOnly
        });

        app.MapHealthChecks("/health/details", new HealthCheckOptions
        {
            ResponseWriter = WriteDetails
        })
        .RequireAuthorization(new AuthorizeAttribute { Roles = Roles.Administrator });

        return app;
    }

    private static Task WriteStatusOnly(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { status = report.Status.ToString() }, SerializerOptions));
    }

    private static Task WriteDetails(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                durationMs = entry.Value.Duration.TotalMilliseconds,
                description = entry.Value.Description,
                error = entry.Value.Exception?.Message
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, SerializerOptions));
    }
}
