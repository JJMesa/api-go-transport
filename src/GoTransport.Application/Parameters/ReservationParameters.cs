using GoTransport.Application.Enums;

namespace GoTransport.Application.Parameters;

public class ReservationParameters : PaginationParameters
{
    public string? SearchCriteria { get; set; }
    public SortDirection? OrderBy { get; set; }
    public bool? IsActive { get; set; }

    /// <summary>
    /// Narrows the Schedule's list to one Trip: the Reservations on this travel date.
    /// </summary>
    public DateTime? TravelDate { get; set; }
}