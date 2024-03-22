using GoTransport.Application.Enums;

namespace GoTransport.Application.Parameters;

public class ReservationParameters : PaginationParameters
{
    public string? SearchCriteria { get; set; }
    public SortDirection? OrderBy { get; set; }
    public bool? IsActive { get; set; }
}