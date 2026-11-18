using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class RoleConfiguration : BaseEntityConfiguration<Role>
{
    public override void Configure(EntityTypeBuilder<Role> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_ROLE");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("ROLE_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.Name)
            .HasMaxLength(256)
            .HasColumnType("varchar(256)")
            .IsUnicode(false)
            .IsRequired(true)
            .HasColumnName("NAME")
            .HasColumnOrder(2);

        builder.Property(e => e.NormalizedName)
            .HasMaxLength(256)
            .IsUnicode(false)
            .IsRequired(true)
            .HasColumnName("NORMALIZED_NAME")
            .HasColumnOrder(3);

        builder.Property(e => e.ConcurrencyStamp)
            .HasMaxLength(128)
            .IsUnicode(false)
            .IsConcurrencyToken()
            .HasColumnName("CONCURRENCY_STAMP")
            .HasColumnOrder(4);
    }
}