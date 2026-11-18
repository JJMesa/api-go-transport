using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class UserTokenConfiguration : BaseEntityConfiguration<UserToken>
{
    public override void Configure(EntityTypeBuilder<UserToken> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_USER_TOKEN");

        builder.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

        builder.Property(e => e.UserId)
            .HasColumnName("USER_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.LoginProvider)
            .HasMaxLength(128)
            .IsUnicode(false)
            .HasColumnType("varchar(128)")
            .HasColumnName("LOGIN_PROVIDER")
            .HasColumnOrder(2);

        builder.Property(e => e.Name)
            .HasMaxLength(128)
            .IsUnicode(false)
            .HasColumnType("varchar(128)")
            .HasColumnName("NAME")
            .HasColumnOrder(3);

        builder.Property(e => e.Value)
            .IsUnicode(false)
            .HasColumnType("varchar(max)")
            .HasColumnName("VALUE")
            .HasColumnOrder(4);

        builder.HasOne(d => d.User).WithMany(p => p.UserTokens)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_APP_USER_TOKEN_USER_ID");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IDX_APP_USER_TOKEN_USER_ID");
    }
}