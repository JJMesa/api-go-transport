using GoTransport.Application.Enums;

namespace GoTransport.Application.Parameters;

public class PointParameters : PaginationParameters
{
    public SortDirection? OrderBy { get; set; }
    public bool? IsActive { get; set; }
}