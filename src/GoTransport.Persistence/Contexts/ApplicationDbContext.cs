using System.Reflection;
using System.Security.Claims;
using GoTransport.Application.Commons;
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
    private readonly TimeProvider _clock;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor httpContextAccessor, TimeProvider clock)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
        _clock = clock;
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasAnnotation("Relational:Collation", "Modern_Spanish_CI_AS");
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    // SaveChanges() and SaveChangesAsync(CancellationToken) both end in these two overloads, so every way of
    // saving stamps the audit columns.
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        StampAuditColumns();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        StampAuditColumns();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void StampAuditColumns()
    {
        // Audit timestamps are stored in UTC.
        var now = _clock.GetUtcNow().UtcDateTime;
        var username = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)?.ToString() ?? Constants.ApplicationUser;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.IsActive ??= true;
                    entry.Entity.CreationUser ??= username;
                    entry.Entity.CreatedAt = now;
                    entry.Entity.UpdateUser ??= username;
                    entry.Entity.UpdatedAt = now;
                    break;

                case EntityState.Modified:
                    // A loaded row already holds its last modifier, so always overwrite it.
                    entry.Entity.UpdateUser = username;
                    entry.Entity.UpdatedAt = now;
                    break;
            }
        }
    }
}