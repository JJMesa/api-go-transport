using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class VehicleConfiguration : BaseEntityConfiguration<Vehicle>
{
    public override void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_VEHICLE");

        builder.HasKey(e => e.VehicleId);

        builder.Property(e => e.VehicleId)
            .HasColumnName("VEHICLE_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.ManufacturerId)
            .HasColumnName("MANUFACTURER_ID")
            .HasColumnOrder(2);

        builder.Property(e => e.LicensePlate)
            .HasMaxLength(8)
            .HasColumnType("varchar(8)")
            .IsUnicode(false)
            .HasColumnName("LICENCE_PLATE")
            .HasColumnOrder(3);

        builder.Property(e => e.Model)
            .HasColumnName("MODEL")
            .HasColumnOrder(4);

        builder.Property(e => e.Capacity)
            .HasColumnName("CAPACITY")
            .HasColumnOrder(5);

        builder.HasOne(d => d.Manufacturer).WithMany(p => p.Vehicles)
            .HasForeignKey(d => d.ManufacturerId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_VEHICLE_MANUFACTURER_ID");
    }
}