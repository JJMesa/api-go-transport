using System.Net;
using GoTransport.Application.Commons;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Builders;

public static class ResponseBuilder<T>
{
    public static JsonResponse<T> Ok(T data)
    {
        return new JsonResponse<T>() { HttpCode = HttpStatusCode.OK, Ok = true, Data = data };
    }

    public static JsonPagedResponse<T> OkPaged(T data, Metadata metadata)
    {
        return new JsonPagedResponse<T>() { HttpCode = HttpStatusCode.OK, Ok = true, Data = data, Metadata = metadata };
    }

    public static JsonResponse<T> Created(T data)
    {
        return new JsonResponse<T>() { HttpCode = HttpStatusCode.Created, Ok = true, Data = data };
    }

    public static JsonResponse<T> NoContent()
    {
        return new JsonResponse<T>() { HttpCode = HttpStatusCode.NoContent, Ok = true };
    }

    public static JsonResponse<T> NotFound()
    {
        return new JsonResponse<T>() { HttpCode = HttpStatusCode.NotFound, Ok = false, Errors = new List<string> { ErrorMessages.NotFound } };
    }

    public static JsonResponse<T> BadRequest(string error)
    {
        return new JsonResponse<T>() { HttpCode = HttpStatusCode.BadRequest, Ok = false, Errors = new List<string> { error } };
    }

    public static JsonResponse<T> BadRequest(List<string> errorList)
    {
        return new JsonResponse<T>() { HttpCode = HttpStatusCode.BadRequest, Ok = false, Errors = errorList };
    }

    public static JsonResponse<T> Unauthorized(string error)
    {
        return new JsonResponse<T>() { HttpCode = HttpStatusCode.Unauthorized, Ok = false, Errors = new List<string> { error } };
    }

    public static JsonResponse<T> Conflict(string error)
    {
        return new JsonResponse<T>() { HttpCode = HttpStatusCode.Conflict, Ok = false, Errors = new List<string> { error } };
    }
}