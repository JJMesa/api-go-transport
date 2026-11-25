using System.Data.Common;
using System.Net;
using GoTransport.Application.Commons;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace GoTransport.Api.Middlewares;

/// <summary>
/// Decides how an unhandled exception is answered: status code, envelope message and log level.
/// </summary>
internal static class ExceptionResponseMapper
{
    /// <summary>
    /// "Client Closed Request": not a standard HTTP code, but the usual one (nginx and others) for a
    /// request the client abandoned. Nobody reads the body; the code keeps such requests out of the 5xx rate.
    /// </summary>
    public const int ClientClosedRequest = 499;

    public sealed record Mapping(int StatusCode, string Message, LogLevel LogLevel);

    public static Mapping Map(Exception exception, bool requestAborted)
    {
        if (exception is OperationCanceledException && requestAborted)
            return new(ClientClosedRequest, ErrorMessages.CanceledTask, LogLevel.Information);

        // EnableRetryOnFailure gives up on a transient fault with this exception.
        if (exception is RetryLimitExceededException)
            return new((int)HttpStatusCode.ServiceUnavailable, ErrorMessages.DatabaseUnavailable, LogLevel.Error);

        var sqlException = FindSqlException(exception);
        if (sqlException is not null)
            return MapSqlError(sqlException.Number, ((DbException)sqlException).IsTransient);

        return new((int)HttpStatusCode.InternalServerError, ErrorMessages.InternalServerError, LogLevel.Error);
    }

    /// <summary>
    /// Constraint violations are the client's conflict with stored data (409); transient faults mean the
    /// database is briefly unavailable (503); anything else is our failure (500).
    /// </summary>
    public static Mapping MapSqlError(int sqlErrorNumber, bool isTransient) => sqlErrorNumber switch
    {
        547 => new((int)HttpStatusCode.Conflict, ErrorMessages.ConflictForeignKey, LogLevel.Warning),
        2601 or 2627 => new((int)HttpStatusCode.Conflict, ErrorMessages.ConflictPrimaryKey, LogLevel.Warning),
        _ when isTransient => new((int)HttpStatusCode.ServiceUnavailable, ErrorMessages.DatabaseUnavailable, LogLevel.Error),
        _ => new((int)HttpStatusCode.InternalServerError, ErrorMessages.InternalServerError, LogLevel.Error)
    };

    private static SqlException? FindSqlException(Exception? exception)
    {
        while (exception is not null)
        {
            if (exception is SqlException sqlException) return sqlException;
            exception = exception.InnerException;
        }

        return null;
    }
}
