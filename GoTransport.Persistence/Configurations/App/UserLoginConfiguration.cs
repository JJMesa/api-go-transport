using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class UserLoginConfiguration : BaseEntityConfiguration<UserLogin>
{
    public override void Configure(EntityTypeBuilder<UserLogin> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_USER_LOGIN");

        builder.HasKey(e => new { e.LoginProvider, e.ProviderKey });

        builder.Property(e => e.LoginProvider)
            .HasMaxLength(128)
            .IsUnicode(false)
            .HasColumnType("varchar(128)")
            .HasColumnName("LOGIN_PROVIDER")
            .HasColumnOrder(1);

        builder.Property(e => e.ProviderKey)
            .HasMaxLength(128)
            .IsUnicode(false)
            .HasColumnType("varchar(128)")
            .HasColumnName("PROVIDER_KEY")
            .HasColumnOrder(2);

        builder.Property(e => e.ProviderDisplayName)
            .IsUnicode(false)
            .HasColumnType("varchar(max)")
            .HasColumnName("PROVIDER_DISPLAY_NAME")
            .HasColumnOrder(3);

        builder.Property(e => e.UserId)
            .IsRequired()
            .HasColumnName("USER_ID")
            .HasColumnOrder(4);

        builder.HasOne(d => d.User).WithMany(p => p.UserLogins)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_APP_USER_LOGIN_USER_ID");

        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IDX_APP_USER_LOGIN_USER_ID");
    }
}