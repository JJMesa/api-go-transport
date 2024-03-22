using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class UserClaimConfiguration : BaseEntityConfiguration<UserClaim>
{
    public override void Configure(EntityTypeBuilder<UserClaim> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_USER_CLAIM");

        builder.HasKey(e => e.Id);

        SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"));

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd()
            .HasColumnType("bigint")
            .HasColumnName("CLAIM_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.UserId)
            .HasColumnName("USER_ID")
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

        builder.HasOne(d => d.User).WithMany(p => p.UserClaims)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_APP_USER_CLAIM_USER_ID");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IDX_APP_USER_CLAIM_USER_ID");
    }
}