using System.Net;
using Asp.Versioning;
using GoTransport.Api.Middlewares;
using GoTransport.Application.Commons;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Settings;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace GoTransport.Api.Extensions;

public static class ServiceExtensions
{
    public static void AddVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(config =>
        {
            config.DefaultApiVersion = new ApiVersion(1, 0);
            config.ReportApiVersions = true;
            // Every route carries its version as a URL segment (api/v1/...).
            config.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            // Groups endpoints as "v1" for Swagger and writes the version into the documented routes.
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });
    }

    public static void AddJwtAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        // 401 and 403 bodies are written by EnvelopeAuthorizationResultHandler, whatever the scheme.
        .AddJwtBearer(jwt =>
        {
            jwt.SaveToken = true;
            jwt.Events = new JwtBearerEvents()
            {
                // A well-signed token is still rejected once its user is deactivated or their security stamp rotates.
                OnTokenValidated = async e =>
                {
                    var accountService = e.HttpContext.RequestServices.GetRequiredService<IAccountService>();
                    if (!await accountService.IsTokenUserValidAsync(e.Principal!))
                        e.Fail(ErrorMessages.Unauthorized);
                }
            };
        });

        ConfigureTokenValidation(services);
    }

    private static void ConfigureTokenValidation(IServiceCollection services) =>
        // Built from the validated JwtSettings when first needed, so a missing section fails startup validation
        // with a clear message instead of a NullReferenceException while registering services.
        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtSettings>>((jwt, settings) => jwt.TokenValidationParameters = settings.Value.CreateValidationParameters());

    public static void AddAuthorizationResponses(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, EnvelopeAuthorizationResultHandler>();
    }

    public static void AddConventionConfiguration(this IServiceCollection services)
    {
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    }

    public static void AddSwaggerGenConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GoTransport.Api",
                Version = "v1",
                Description = "API para la gestión de puntos, rutas, horarios, vehículos y reservas de una empresa de transporte.",
                Contact = new OpenApiContact
                {
                    Name = "Juan Mesa",
                    Url = new Uri("https://www.linkedin.com/in/juanjmesam/")
                }
            });

            c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT Authorization header using the bearer scheme.\n\nEnter prefix (Bearer), space, and your token.\nExample: 'Bearer abcdefghi123456789'"
            });

            c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, document)] = []
            });
        });
    }

    public const string CorsPolicy = "Security";

    /// <summary>
    /// Browser origins come from <c>Cors:AllowedOrigins</c>. With none configured, Development allows any
    /// origin (local front ends) and every other environment allows none, so a deployment must list its
    /// front ends explicitly.
    /// </summary>
    public static void AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors();
        services.AddOptions<CorsOptions>()
            .Configure<IConfiguration, IHostEnvironment>((options, configuration, environment) =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

                options.AddPolicy(CorsPolicy, policy =>
                {
                    if (allowedOrigins.Length > 0)
                        policy.WithOrigins(allowedOrigins);
                    else if (environment.IsDevelopment())
                        policy.AllowAnyOrigin();

                    policy.AllowAnyMethod().AllowAnyHeader();
                });
            });
    }

    public static void AddCustomResponseFluentValidation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(opt => opt.InvalidModelStateResponseFactory = actionContext =>
        {
            var jsonResponse = new JsonResponse<object>
            {
                HttpCode = HttpStatusCode.BadRequest,
                Errors = actionContext.ModelState.Values
                .Where(v => v.Errors.Count > 0)
                .SelectMany(x => x.Errors)
                .Select(x => x.ErrorMessage)
            };

            return new BadRequestObjectResult(jsonResponse);
        });
    }
}