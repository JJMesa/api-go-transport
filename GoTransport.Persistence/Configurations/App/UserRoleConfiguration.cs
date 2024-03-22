using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class UserRoleConfiguration : BaseEntityConfiguration<UserRole>
{
    public override void Configure(EntityTypeBuilder<UserRole> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_USER_ROLE");

        builder.HasKey(e => new { e.UserId, e.RoleId });

        builder.Property(e => e.UserId)
            .HasColumnName("USER_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.RoleId)
            .HasColumnName("ROLE_ID")
            .HasColumnOrder(2);

        builder.HasOne(d => d.Role).WithMany(p => p.UserRoles)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_USER_ROLE_ROLE_ID");

        builder.HasOne(d => d.User).WithMany(p => p.UserRoles)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_USER_ROLE_USER_ID");

        builder.HasIndex(e => e.RoleId)
            .HasDatabaseName("IDX_APP_USER_ROLE_ROLE_ID");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IDX_APP_USER_ROLE_USER_ID");
    }
}