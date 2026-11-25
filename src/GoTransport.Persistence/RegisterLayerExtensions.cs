using GoTransport.Domain.Interfaces;
using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Contexts;
using GoTransport.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        // Password and lockout rules are configured by Application (AddApplicationLayer).
        services.AddDefaultIdentity<User>()
            .AddRoles<Role>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        #endregion Configuracion identity

        #region Repositories

        services.AddTransient(typeof(IRepository<>), typeof(BaseRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        #endregion Repositories
    }
}