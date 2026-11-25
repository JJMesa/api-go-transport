namespace GoTransport.Application.Dtos.Reservation;

/// <summary>
/// The Passenger's document number, sent in the request body so it never appears in a URL (and so in
/// request logs, proxy logs or browser history).
/// </summary>
public class PassengerLookupDto
{
    public string PassengerIdentification { get; set; } = null!;
}
