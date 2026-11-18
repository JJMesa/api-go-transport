using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_USER");

        builder.HasKey(x => x.Id);

        builder.Property(e => e.Id)
            .HasColumnName("USER_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.FirstName)
            .HasMaxLength(128)
            .HasColumnType("varchar(128)")
            .IsUnicode(false)
            .HasColumnName("FIRST_NAME")
            .HasColumnOrder(2);

        builder.Property(e => e.LastName)
            .HasMaxLength(128)
            .HasColumnType("varchar(128)")
            .IsUnicode(false)
            .HasColumnName("LAST_NAME")
            .HasColumnOrder(3);

        builder.Property(e => e.UserName)
            .HasMaxLength(256)
            .HasColumnType("varchar(256)")
            .IsUnicode(false)
            .IsRequired(true)
            .HasColumnName("USERNAME")
            .HasColumnOrder(4);

        builder.Property(e => e.NormalizedUserName)
            .HasMaxLength(256)
            .IsUnicode(false)
            .IsRequired(true)
            .HasColumnName("NORMALIZED_USERNAME")
            .HasColumnOrder(5);

        builder.Property(e => e.Email)
            .HasMaxLength(256)
            .IsUnicode(false)
            .IsRequired(true)
            .HasColumnType("varchar(256)")
            .HasColumnName("EMAIL")
            .HasColumnOrder(6);

        builder.Property(e => e.NormalizedEmail)
            .HasMaxLength(256)
            .IsUnicode(false)
            .IsRequired(true)
            .HasColumnType("varchar(256)")
            .HasColumnName("NORMALIZED_EMAIL")
            .HasColumnOrder(7);

        builder.Property(e => e.EmailConfirmed)
            .HasColumnType("bit")
            .HasColumnName("EMAIL_CONFIRMED")
            .HasColumnOrder(8);

        builder.Property(e => e.PasswordHash)
            .IsUnicode(false)
            .HasColumnType("varchar(max)")
            .HasColumnName("PASSWORD_HASH")
            .HasColumnOrder(9);

        builder.Property(e => e.SecurityStamp)
            .HasMaxLength(128)
            .IsUnicode(false)
            .HasColumnName("SECURITY_STAMP")
            .HasColumnOrder(10);

        builder.Property(e => e.ConcurrencyStamp)
            .HasMaxLength(128)
            .IsUnicode(false)
            .IsConcurrencyToken()
            .HasColumnName("CONCURRENCY_STAMP")
            .HasColumnOrder(11);

        builder.Property(e => e.PhoneNumber)
            .HasMaxLength(128)
            .IsUnicode(false)
            .HasColumnName("PHONE_NUMBER")
            .HasColumnOrder(12);

        builder.Property(e => e.PhoneNumberConfirmed)
            .HasColumnType("bit")
            .HasColumnName("PHONE_NUMBER_CONFIRMED")
            .HasColumnOrder(13);

        builder.Property(e => e.TwoFactorEnabled)
            .HasColumnType("bit")
            .HasColumnName("TWO_FACTOR_ENABLED")
            .HasColumnOrder(14);

        builder.Property(e => e.LockoutEnd)
            .HasColumnType("datetimeoffset")
            .HasColumnName("LOCKOUT_END")
            .HasColumnOrder(15);

        builder.Property(e => e.LockoutEnabled)
            .HasColumnType("bit")
            .HasColumnName("LOCKOUT_ENABLED")
            .HasColumnOrder(16);

        builder.Property(e => e.AccessFailedCount)
            .HasColumnType("int")
            .HasColumnName("ACCESS_FAILED_COUNT")
            .HasColumnOrder(17);

        builder.HasIndex(e => e.NormalizedEmail)
            .HasDatabaseName("IDX_APP_USER_NORMALIZED_EMAIL");

        builder.HasIndex(e => e.NormalizedUserName)
            .IsUnique()
            .HasDatabaseName("IDX_APP_USER_NORMALIZED_USERNAME")
            .HasFilter("[NORMALIZED_USERNAME] IS NOT NULL");
    }
}