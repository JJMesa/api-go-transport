using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class RoleClaimConfiguration : BaseEntityConfiguration<RoleClaim>
{
    public override void Configure(EntityTypeBuilder<RoleClaim> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_ROLE_CLAIM");

        builder.HasKey(e => e.Id);

        builder.Property<int>("Id").UseIdentityColumn();

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint")
            .HasColumnName("CLAIM_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.RoleId)
            .IsRequired()
            .HasColumnName("ROLE_ID")
            .HasColumnOrder(2);

        builder.Property(e => e.ClaimType)
            .HasMaxLength(128)
            .IsUnicode(false)
            .HasColumnType("varchar(128)")
            .HasColumnName("CLAIM_TYPE")
            .HasColumnOrder(3);

        builder.Property(e => e.ClaimValue)
            .IsUnicode(false)
            .HasColumnType("varchar(max)")
            .HasColumnName("CLAIM_VALUE")
            .HasColumnOrder(4);

        builder.HasOne(d => d.Role).WithMany(p => p.RoleClaims)
            .HasForeignKey(d => d.RoleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_APP_ROLE_CLAIM_ROLE_ID");
    }
}