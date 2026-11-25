using System.Net;

namespace GoTransport.Application.Wrappers;

public class JsonResponse<T>
{
    public HttpStatusCode HttpCode { get; set; }
    public bool Ok { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = null!;

    /// <summary>
    /// The request's correlation id on unexpected failures (also in the X-Correlation-Id header), so a
    /// reported error can be found in the logs. Not set on other responses.
    /// </summary>
    public string? TraceId { get; set; }
}