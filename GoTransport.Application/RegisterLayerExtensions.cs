using System.Globalization;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using GoTransport.Application.Attributes;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Services;
using GoTransport.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoTransport.Application;

public static class RegisterLayerExtensions
{
    public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("es-CO");
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddFluentValidationAutoValidation();
        services.AddTransient(typeof(ICacheService<>), typeof(CacheService<>));

        services.Configure<CachingSettings>(configuration.GetSection("CachingSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));

        services.RegisterServices();
    }

    public static void RegisterServices(this IServiceCollection services)
    {
        Type scoped = typeof(ScopedAttribute);
        Type singleton = typeof(SingletonAttribute);
        Type transient = typeof(TransientAttribute);

        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => p.IsDefined(scoped, true) || p.IsDefined(transient, true) || (p.IsDefined(singleton, true) && !p.IsInterface))
            .Select(s => new
            {
                Service = s.GetInterface($"I{s.Name}"),
                Implementation = s
            })
            .Where(x => x.Service != null);

        foreach (var type in types)
        {
            if (type.Implementation.IsDefined(scoped, false))
                services.AddScoped(type.Service!, type.Implementation);

            if (type.Implementation.IsDefined(transient, false))
                services.AddTransient(type.Service!, type.Implementation);

            if (type.Implementation.IsDefined(singleton, false))
                services.AddSingleton(type.Service!, type.Implementation);
        }
    }
}