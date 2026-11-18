namespace GoTransport.Application.Wrappers;

public class Metadata
{
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public int? NextPageNumber => HasNextPage ? CurrentPage + 1 : null;
    public int? PreviousPageNumber => HasPreviousPage ? CurrentPage - 1 : null;

    public Metadata(int totalItems, int pageNumber, int pageSize)
    {
        TotalItems = totalItems;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    public static Metadata Create(int pageNumber, int pageSize, int totalAmount)
    {
        return new Metadata(totalAmount, pageNumber, pageSize);
    }
}