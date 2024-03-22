using GoTransport.Application.Interfaces.Base;
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
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
            )
        );

        #endregion Contextos

        #region Configuracion identity

        services.AddDefaultIdentity<User>()
            .AddRoles<Role>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        #endregion Configuracion identity

        #region Repositories

        services.AddTransient(typeof(IRepository<>), typeof(BaseRepository<>));

        #endregion Repositories
    }
}