using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class PointConfiguration : BaseEntityConfiguration<Point>
{
    public override void Configure(EntityTypeBuilder<Point> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_POINT");

        builder.HasKey(e => e.PointId);

        builder.Property(e => e.PointId)
            .HasColumnName("POINT_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.CityId)
            .HasColumnName("CITY_ID")
            .HasColumnOrder(2);

        builder.Property(e => e.Detail)
            .HasMaxLength(64)
            .HasColumnType("varchar(64)")
            .IsUnicode(false)
            .IsRequired(false)
            .HasColumnName("DETAIL")
            .HasColumnOrder(3);

        builder.HasOne(d => d.City).WithMany(p => p.Points)
            .HasForeignKey(d => d.CityId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_POINT_CITY_ID");
    }
}