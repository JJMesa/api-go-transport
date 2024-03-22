using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class RouteConfiguration : BaseEntityConfiguration<Route>
{
    public override void Configure(EntityTypeBuilder<Route> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_ROUTE");

        builder.HasKey(e => e.RouteId);

        builder.Property(e => e.RouteId)
            .HasColumnName("ROUTE_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.Description)
            .HasMaxLength(128)
            .HasColumnType("varchar(128)")
            .IsUnicode(false)
            .IsRequired(false)
            .HasColumnName("DESCRIPTION")
            .HasColumnOrder(2);

        builder.Property(e => e.OriginPointId)
            .HasColumnName("ORIGIN_POINT_ID")
            .HasColumnOrder(3);

        builder.Property(e => e.DestinationPointId)
            .HasColumnName("DESTINATION_POINT_ID")
            .HasColumnOrder(4);

        builder.HasOne(d => d.OriginPoint).WithMany(p => p.OriginRoutes)
            .HasForeignKey(d => d.OriginPointId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_ROUTE_ORIGIN_POINT_ID");

        builder.HasOne(d => d.DestinationPoint).WithMany(p => p.DestinationRoutes)
            .HasForeignKey(d => d.DestinationPointId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_ROUTE_DESTINATION_POINT_ID");
    }
}