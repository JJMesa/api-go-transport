using System.Text.Json.Serialization;
using System.Globalization;
using GoTransport.Api.Extensions;
using GoTransport.Api.Security;
using GoTransport.Application;
using GoTransport.Domain.Interfaces;
using GoTransport.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseDefaultServiceProvider(options =>
    {
        options.ValidateScopes = true;
        options.ValidateOnBuild = true;
    });

    // WebApplication.CreateBuilder already adds User Secrets in Development only, after appsettings and
    // before environment variables, so environment variables win wherever they are set.
    var configuration = builder.Configuration;

    builder.AddSerilogConfiguration();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
    builder.Services.AddApplicationLayer(configuration);
    builder.Services.AddPersistenceLayer(configuration);
    builder.Services.AddControllers()
        .AddJsonOptions(opt => opt.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull);

    builder.Services.AddVersioningConfiguration();

    builder.Services.AddCustomResponseFluentValidation();

    builder.Services.AddJwtAuthentication();
    builder.Services.AddAuthorizationResponses();
    builder.Services.AddRateLimitingConfiguration();

    builder.Services.AddCorsConfiguration();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGenConfiguration();

    builder.Services.AddConventionConfiguration();

    builder.Services.AddHealthChecksConfiguration();

    var app = builder.Build();

    await app.EnsureAdministratorAsync();

    app.UseCorrelationId();

    app.UseSerilogRequestLogging();

    app.UseErrorHandlerMiddleware();

    app.UseSecurityHeaders();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // HSTS tells browsers to use HTTPS only; not in Development, where it would stick to localhost.
    if (!app.Environment.IsDevelopment())
        app.UseHsts();

    app.UseHttpsRedirection();

    app.UseCors(ServiceExtensions.CorsPolicy);

    app.UseAuthentication();

    app.UseAuthorization();

    app.UseRateLimiter();

    app.MapControllers();

    app.MapHealthChecksConfiguration();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // Log, then let the failure end the process with a non-zero exit code so orchestrators (and the
    // component tests) see a failed start instead of a clean shutdown. HostAbortedException is how the
    // EF design-time tools stop the host after Build(); it is not a failure.
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Exposes the implicit Program class so the component tests can bootstrap the API host.
public partial class Program { }