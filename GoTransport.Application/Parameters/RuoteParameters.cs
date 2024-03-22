using GoTransport.Application.Enums;

namespace GoTransport.Application.Parameters;

public class RouteParameters : PaginationParameters
{
    public string? OriginPointSearchCriteria { get; set; }
    public string? DestinationPointSearchCriteria { get; set; }
    public SortDirection? OrderBy { get; set; }
    public bool? IsActive { get; set; }
}