using System.Globalization;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Services;
using GoTransport.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace GoTransport.Application;

public static class RegisterLayerExtensions
{
    public static void AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("es-CO");
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddFluentValidationAutoValidation();
        services.AddMemoryCache();
        // The one source of "now", so time-dependent rules can be tested with a fake clock.
        services.TryAddSingleton(TimeProvider.System);
        services.AddTransient(typeof(ICacheService<>), typeof(CacheService<>));

        // Invalid settings stop the host at startup instead of failing the first request that reads them.
        AddValidatedSettings<CachingSettings>(services, configuration, "CachingSettings");
        AddValidatedSettings<JwtSettings>(services, configuration, "JwtSettings");
        AddValidatedSettings<LockoutSettings>(services, configuration, "LockoutSettings");
        AddValidatedSettings<AdminBootstrapSettings>(services, configuration, "AdminBootstrap")
            .Validate(settings => string.IsNullOrWhiteSpace(settings.Email) == string.IsNullOrWhiteSpace(settings.Password),
                "AdminBootstrap needs both Email and Password, or neither.");

        services.RegisterServices();
    }

    private static OptionsBuilder<TSettings> AddValidatedSettings<TSettings>(IServiceCollection services, IConfiguration configuration, string section)
        where TSettings : class =>
        services.AddOptions<TSettings>()
            .Bind(configuration.GetSection(section))
            .ValidateDataAnnotations()
            .ValidateOnStart();

    /// <summary>
    /// Every Application service, registered explicitly: a missing line fails startup validation
    /// (ValidateOnBuild) with the interface's name, instead of a reflection scan skipping it silently.
    /// </summary>
    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<IAdminBootstrapService, AdminBootstrapService>();
        services.AddTransient<ICityService, CityService>();
        services.AddTransient<IDepartmentService, DepartmentService>();
        services.AddTransient<IIdentificationTypeService, IdentificationTypeService>();
        services.AddTransient<IManufacturerService, ManufacturerService>();
        services.AddTransient<IPointService, PointService>();
        services.AddTransient<IReservationService, ReservationService>();
        services.AddTransient<IRoleService, RoleService>();
        services.AddTransient<IRouteService, RouteService>();
        services.AddTransient<IScheduleService, ScheduleService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IVehicleService, VehicleService>();
    }
}