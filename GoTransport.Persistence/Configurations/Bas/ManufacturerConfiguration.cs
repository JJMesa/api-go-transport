using GoTransport.Domain.Entities.Bas;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.Bas;

internal class ManufacturerConfiguration : BaseEntityConfiguration<Manufacturer>
{
    public override void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        base.Configure(builder);

        builder.ToTable("BAS_MANUFACTURER");

        builder.HasKey(e => e.ManufacturerId);

        builder.Property(e => e.ManufacturerId)
            .HasColumnType("int")
            .HasColumnName("MANUFACTURER_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.Description)
            .HasMaxLength(32)
            .HasColumnType("varchar(32)")
            .IsUnicode(false)
            .HasColumnName("DESCRIPTION")
            .HasColumnOrder(2);
    }
}