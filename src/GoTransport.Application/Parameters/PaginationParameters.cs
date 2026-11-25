using GoTransport.Application.Commons;

namespace GoTransport.Application.Parameters;

public class PaginationParameters
{
    private int _pageNumber;
    private int _pageSize;

    /// <summary>
    /// 1-based. Model binding uses the setter, so it is clamped here: page 0 or below would make a negative OFFSET.
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

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
}