using System.Net;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using GoTransport.Application.Builders;
using GoTransport.Application.Commons;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Settings;
using GoTransport.Application.Wrappers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection("JwtSettings").Get<JwtSettings>();
        var key = new HMACSHA512(Encoding.UTF8.GetBytes(settings!.Secret));
        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key.Key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = settings.TokenIssuer,
            ValidAudience = settings.TokenAudience,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            LifetimeValidator = (notBefore, expires, _, _) => DateTime.UtcNow >= notBefore && expires > DateTime.UtcNow
        };

        services.AddSingleton(tokenValidationParameters);

        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(jwt =>
        {
            jwt.SaveToken = true;
            jwt.TokenValidationParameters = tokenValidationParameters;
            jwt.Events = new JwtBearerEvents()
            {
                // A well-signed token is still rejected once its user is deactivated or their security stamp rotates.
                OnTokenValidated = async e =>
                {
                    var accountService = e.HttpContext.RequestServices.GetRequiredService<IAccountService>();
                    if (!await accountService.IsTokenUserValidAsync(e.Principal!))
                        e.Fail(ErrorMessages.Unauthorized);
                },
                // Disparador error en la autenticacion
                OnAuthenticationFailed = e =>
                {
                    var endpoint = e.HttpContext.GetEndpoint();
                    if (endpoint?.Metadata.GetMetadata<AllowAnonymousAttribute>() is not null)
                    {
                        e.Response.Headers.Clear();
                        e.NoResult();
                        return Task.CompletedTask;
                    }

                    e.NoResult();
                    e.Response.ContentType = MediaTypeNames.Application.Json;
                    e.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    var response = ResponseBuilder<object>.Unauthorized(ErrorMessages.Unauthorized);
                    var result = JsonSerializer.Serialize(response,
                        new JsonSerializerOptions
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        });
                    return e.Response.WriteAsync(result);
                },
                // Disparador error si no se tiene permiso
                OnChallenge = e =>
                {
                    if (!e.Response.HasStarted)
                    {
                        e.HandleResponse();
                        e.Response.ContentType = MediaTypeNames.Application.Json;
                        e.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        var response = ResponseBuilder<object>.Unauthorized(ErrorMessages.Unauthorized);
                        var result = JsonSerializer.Serialize(response,
                        new JsonSerializerOptions
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        });
                        return e.Response.WriteAsync(result);
                    }
                    else
                    {
                        e.HandleResponse();
                        return e.Response.WriteAsync(string.Empty);
                    }
                },
                // Disparador error si no se esta verificado
                OnForbidden = e =>
                {
                    e.Response.ContentType = MediaTypeNames.Application.Json;
                    e.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    var response = ResponseBuilder<object>.Unauthorized(ErrorMessages.Unauthorized);
                    var result = JsonSerializer.Serialize(response,
                        new JsonSerializerOptions
                        {
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        });
                    return e.Response.WriteAsync(result);
                }
            };
        });
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

    public static void AddCorsConfiguration(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("Security",
                builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());
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