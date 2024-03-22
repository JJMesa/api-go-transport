using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class ScheduleConfiguration : BaseEntityConfiguration<Schedule>
{
    public override void Configure(EntityTypeBuilder<Schedule> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_SCHEDULE");

        builder.HasKey(e => e.ScheduleId);

        builder.Property(e => e.ScheduleId)
            .HasColumnName("SCHEDULE_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.DepartureTime)
            .HasMaxLength(5)
            .HasColumnType("time(5)")
            .HasColumnName("DEPARTURE_TIME")
            .HasColumnOrder(2);

        builder.Property(e => e.ArrivalTime)
            .HasMaxLength(5)
            .HasColumnType("time(5)")
            .HasColumnName("ARRIVAL_TIME")
            .HasColumnOrder(3);

        builder.Property(e => e.Duration)
            .HasMaxLength(5)
            .HasColumnType("time(5)")
            .HasColumnName("DURATION")
            .HasColumnOrder(4);

        builder.Property(e => e.RouteId)
            .HasColumnName("ROUTE_ID")
            .HasColumnOrder(5);

        builder.Property(e => e.VehicleId)
            .HasColumnName("VEHICLE_ID")
            .HasColumnOrder(6);

        builder.HasOne(d => d.Vehicle).WithMany(p => p.Schedules)
            .HasForeignKey(d => d.VehicleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_SCHEDULE_VEHICLE_ID");

        builder.HasOne(d => d.Route).WithMany(p => p.Schedules)
            .HasForeignKey(d => d.RouteId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_SCHEDULE_ROUTE_ID");
    }
}