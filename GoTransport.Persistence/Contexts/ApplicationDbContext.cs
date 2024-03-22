using System.Reflection;
using System.Security.Claims;
using GoTransport.Application.Commons;
using GoTransport.Application.Extensions;
using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GoTransport.Persistence.Contexts;

public partial class ApplicationDbContext : IdentityDbContext<User, Role, long, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasAnnotation("Relational:Collation", "Modern_Spanish_CI_AS");
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var username = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() ?? Constants.ApplicationUser;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.IsActive ??= true;
                    entry.Entity.CreationUser ??= username;
                    entry.Entity.CreatedAt = DateTime.UtcNow.ToPacificStandardTime();
                    entry.Entity.UpdateUser ??= username;
                    entry.Entity.UpdatedAt = DateTime.UtcNow.ToPacificStandardTime();
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdateUser ??= username;
                    entry.Entity.UpdatedAt = DateTime.UtcNow.ToPacificStandardTime();
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}