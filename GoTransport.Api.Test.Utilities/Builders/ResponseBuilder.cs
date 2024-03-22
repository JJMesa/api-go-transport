using System.Net;
using GoTransport.Application.Wrappers;

namespace GoTransport.Api.Test.Utilities.Builders;

public class ResponseBuilder<T>
{
    private readonly JsonResponse<T> _jsonReponse = new();

    public ResponseBuilder<T> WithHttpCode(HttpStatusCode httpCode)
    {
        _jsonReponse.HttpCode = httpCode;
        return this;
    }

    public ResponseBuilder<T> WithOk(bool isOk)
    {
        _jsonReponse.Ok = isOk;
        return this;
    }

    public ResponseBuilder<T> WithData(T? data)
    {
        _jsonReponse.Data = data;
        return this;
    }

    public ResponseBuilder<T> WithErrors(IEnumerable<string> errors)
    {
        _jsonReponse.Errors = errors;
        return this;
    }

    public JsonResponse<T> Build() => _jsonReponse;
}