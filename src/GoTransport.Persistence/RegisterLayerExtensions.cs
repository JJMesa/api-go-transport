using GoTransport.Application.Commons;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Settings;
using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Contexts;
using GoTransport.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace GoTransport.Persistence;

public static class RegisterLayerExtensions
{
    public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration configuration)
    {
        #region Contextos

        services.AddDbContext<ApplicationDbContext>(opt =>
            opt.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection")!,
                // Retries transient SQL Server failures (including deadlock victims). Transactions must then run
                // through the execution strategy, which IUnitOfWork does.
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName).EnableRetryOnFailure()
            )
        );

        #endregion Contextos

        #region Configuracion identity

        services.AddDefaultIdentity<User>(options =>
            {
                PasswordPolicy.Apply(options.Password);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        // Read through the validated LockoutSettings options rather than straight from configuration.
        services.AddOptions<IdentityOptions>()
            .Configure<IOptions<LockoutSettings>>((options, lockout) =>
            {
                options.Lockout.MaxFailedAccessAttempts = lockout.Value.MaxFailedAccessAttempts;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(lockout.Value.LockoutTimeInMinutes);
            });

        #endregion Configuracion identity

        #region Repositories

        services.AddTransient(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        #endregion Repositories
    }
}