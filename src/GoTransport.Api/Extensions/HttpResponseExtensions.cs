using System.Text.Json;
using GoTransport.Application.Commons;
using GoTransport.Application.Wrappers;

namespace GoTransport.Api.Extensions;

public static class HttpResponseExtensions
{
    /// <summary>
    /// Writes the paging metadata of a paged list to the <c>Pagination</c> response header.
    /// </summary>
    public static void AddPaginationHeader(this HttpResponse response, Metadata? metadata) =>
        response.Headers.Append(Constants.PaginationHeader, JsonSerializer.Serialize(metadata));
}
