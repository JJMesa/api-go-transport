using GoTransport.Domain.Entities.App;
using GoTransport.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoTransport.Persistence.Configurations.App;

internal class ReservationConfiguration : BaseEntityConfiguration<Reservation>
{
    public override void Configure(EntityTypeBuilder<Reservation> builder)
    {
        base.Configure(builder);

        builder.ToTable("APP_RESERVATION");

        builder.HasKey(a => a.ReservationId);

        builder.Property(e => e.ReservationId)
            .HasColumnName("RESERVATION_ID")
            .HasColumnOrder(1);

        builder.Property(e => e.ScheduleId)
            .HasColumnName("SCHEDULE_ID")
            .HasColumnOrder(2);

        builder.Property(e => e.ReservationDate)
            .HasColumnType("date")
            .HasColumnName("RESERVATION_DATE")
            .HasColumnOrder(3);

        builder.Property(e => e.PassengerFirstName)
            .HasMaxLength(128)
            .HasColumnType("varchar(128)")
            .IsUnicode(false)
            .HasColumnName("PASSENGER_FIRST_NAME")
            .HasColumnOrder(4);

        builder.Property(e => e.PassengerLastName)
            .HasMaxLength(128)
            .HasColumnType("varchar(128)")
            .IsUnicode(false)
            .HasColumnName("PASSENGER_LAST_NAME")
            .HasColumnOrder(5);

        builder.Property(e => e.IdentificationTypeId)
            .HasColumnName("IDENTIFICATION_TYPE_ID")
            .HasColumnOrder(6);

        builder.Property(e => e.PassengerIdentification)
            .HasMaxLength(128)
            .HasColumnType("varchar(128)")
            .IsUnicode(false)
            .HasColumnName("PASSENGER_IDENTIFICATION")
            .HasColumnOrder(7);

        builder.Property(e => e.Detail)
            .HasMaxLength(128)
            .HasColumnType("varchar(128)")
            .IsUnicode(false)
            .IsRequired(false)
            .HasColumnName("DETAIL")
            .HasColumnOrder(8);

        builder.HasOne(d => d.Schedule).WithMany(p => p.Reservations)
            .HasForeignKey(d => d.ScheduleId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_RESERVATION_SCHEDULE_ID");

        builder.HasOne(d => d.IdentificationType).WithMany(p => p.Reservations)
            .HasForeignKey(d => d.IdentificationTypeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_APP_RESERVATION_IDENTIFICATION_TYPE_ID");
    }
}