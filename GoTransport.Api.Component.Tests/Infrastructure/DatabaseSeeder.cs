using GoTransport.Domain.Entities.Bas;
using GoTransport.Persistence.Contexts;

namespace GoTransport.Api.Component.Tests.Infrastructure;

/// <summary>
/// Populates the in-memory database with a small, deterministic dataset that the component tests
/// assert against. Ids match the values exposed through <c>Utils.DefaultId</c> so the existing
/// expectations remain valid.
/// </summary>
public static class DatabaseSeeder
{
    public static void Seed(ApplicationDbContext context)
    {
        context.Set<Department>().AddRange(
            new Department { DepartmentId = 1, Description = "Antioquia", IsActive = true },
            new Department { DepartmentId = 2, Description = "Cundinamarca", IsActive = true });

        context.Set<City>().AddRange(
            new City { CityId = 1, Description = "Medellín", DepartmentId = 1, IsActive = true },
            new City { CityId = 2, Description = "Abejorral", DepartmentId = 1, IsActive = true },
            new City { CityId = 3, Description = "Bello", DepartmentId = 1, IsActive = true },
            new City { CityId = 4, Description = "Envigado", DepartmentId = 1, IsActive = true },
            new City { CityId = 5, Description = "Itagüí", DepartmentId = 1, IsActive = true },
            new City { CityId = 6, Description = "Bogotá", DepartmentId = 2, IsActive = true },
            new City { CityId = 7, Description = "Soacha", DepartmentId = 2, IsActive = true });

        // Persist through SaveChangesAsync so the auditing override stamps the required audit columns.
        context.SaveChangesAsync().GetAwaiter().GetResult();
    }
}