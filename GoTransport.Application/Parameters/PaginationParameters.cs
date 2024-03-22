using GoTransport.Application.Commons;

namespace GoTransport.Application.Parameters;

public class PaginationParameters
{
    private int _pageSize;

    public int PageNumber { get; set; }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > Constants.MaximumRecordsPaged ? Constants.MaximumRecordsPaged : (value < 1 ? 1 : value);
    }

    public PaginationParameters()
    {
        PageNumber = 1;
        PageSize = 10;
    }

    public PaginationParameters(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber < 1 ? 1 : pageNumber;
        PageSize = pageSize > Constants.MaximumRecordsPaged ? Constants.MaximumRecordsPaged : (pageSize < 1 ? 1 : pageSize);
    }
}