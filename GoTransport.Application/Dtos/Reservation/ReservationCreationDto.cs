namespace GoTransport.Application.Dtos.Reservation;

public class ReservationCreationDto
{
    public Guid ScheduleId { get; set; }

    public DateTime ReservationDate { get; set; }

    public string PassengerFirstName { get; set; } = null!;

    public string PassengerLastName { get; set; } = null!;

    public int IdentificationTypeId { get; set; }

    public string PassengerIdentification { get; set; } = null!;

    public string Detail { get; set; } = null!;
}