using System.Net;
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

    /// <summary>
    /// On a 201, points the <c>Location</c> header at the new resource: the request path (the collection)
    /// followed by the new id.
    /// </summary>
    public static void AddLocationHeader<T>(this HttpResponse response, JsonResponse<T> result, Func<T, object> id)
    {
        if (result.HttpCode != HttpStatusCode.Created || result.Data is null) return;

        var request = response.HttpContext.Request;
        response.Headers.Location = $"{request.PathBase}{request.Path.Value!.TrimEnd('/')}/{id(result.Data)}";
    }
}
