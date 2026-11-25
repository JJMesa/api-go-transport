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

    // Public so clients can deserialize the Pagination header; same parameter order as Create.
    public Metadata(int pageNumber, int pageSize, int totalItems)
    {
        CurrentPage = pageNumber;
        PageSize = pageSize;
        TotalItems = totalItems;
        TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    public static Metadata Create(int pageNumber, int pageSize, int totalItems) => new(pageNumber, pageSize, totalItems);
}