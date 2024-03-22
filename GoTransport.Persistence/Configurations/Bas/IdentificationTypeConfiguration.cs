using GoTransport.Domain.Entities.Bas;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.Bas;

internal class IdentificationTypeConfiguration : BaseEntityConfiguration<IdentificationType>
{
    public override void Configure(EntityTypeBuilder<IdentificationType> builder)
    {
        base.Configure(builder);

        builder.ToTable("BAS_IDENTIFICATION_TYPE");

        builder.HasKey(e => e.IdentificationTypeId);

        builder.Property(e => e.IdentificationTypeId)
            .HasColumnType("int")
            .HasColumnName("IDENTIFICATION_TYPE_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.Description)
            .HasMaxLength(32)
            .HasColumnType("varchar(32)")
            .IsUnicode(false)
            .HasColumnName("DESCRIPTION")
            .HasColumnOrder(2);
    }
}