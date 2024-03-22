namespace GoTransport.Application.Dtos.Reservation;

public class ReservationUpdateDto
{
    public Guid ReservationId { get; set; }

    public Guid ScheduleId { get; set; }

    public DateTime ReservationDate { get; set; }

    public string Detail { get; set; } = null!;
}