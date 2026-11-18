using Serilog;

namespace GoTransport.Api.Extensions;

public static class LoggingExtensions
{
    /// <summary>
    /// Configures Serilog as the structured logging provider, reading sinks
    /// and levels from the "Serilog" configuration section.
    /// </summary>
    public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        builder.Host.UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

        return builder;
    }
}
