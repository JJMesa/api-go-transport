using GoTransport.Application.Dtos.IdentificationType;
using GoTransport.Application.Dtos.Schedule;

namespace GoTransport.Application.Dtos.Reservation;

public class ReservationDto
{
    public Guid ReservationId { get; set; }

    public ScheduleDto? Schedule { get; set; }

    /// <summary>
    /// The travel date: the day of the Trip, which with the Schedule identifies it. Not the day the Reservation was made.
    /// </summary>
    public DateTime ReservationDate { get; set; }

    public string PassengerFirstName { get; set; } = null!;

    public string PassengerLastName { get; set; } = null!;

    public IdentificationTypeDto? IdentificationType { get; set; }

    public string PassengerIdentification { get; set; } = null!;

    public string Detail { get; set; } = null!;

    public bool IsActive { get; set; }
}