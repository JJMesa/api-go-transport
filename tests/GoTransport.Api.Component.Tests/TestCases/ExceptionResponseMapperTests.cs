using GoTransport.Api.Middlewares;
using GoTransport.Application.Commons;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// The status mapping of <c>ErrorHandlerMiddleware</c>. SQL Server errors cannot be raised on EF InMemory,
/// so they are covered here by error number.
/// </summary>
public class ExceptionResponseMapperTests
{
    [Theory]
    [InlineData(547, 409)]   // foreign-key conflict
    [InlineData(2601, 409)]  // unique index
    [InlineData(2627, 409)]  // unique constraint
    [InlineData(50000, 500)] // anything else
    public void MapSqlError_ShouldMapConstraintViolationsToConflict(int sqlErrorNumber, int expectedStatus)
    {
        // Act
        var actual = ExceptionResponseMapper.MapSqlError(sqlErrorNumber, isTransient: false);

        // Assert
        Assert.Equal(expectedStatus, actual.StatusCode);
    }

    [Fact]
    public void MapSqlError_ShouldMapATransientFaultToServiceUnavailable()
    {
        // Act
        var actual = ExceptionResponseMapper.MapSqlError(1205, isTransient: true);

        // Assert
        Assert.Equal(503, actual.StatusCode);
        Assert.Equal(ErrorMessages.DatabaseUnavailable, actual.Message);
    }

    [Fact]
    public void Map_ShouldMapExhaustedRetriesToServiceUnavailable()
    {
        // Act
        var actual = ExceptionResponseMapper.Map(new RetryLimitExceededException("retries exhausted", new TimeoutException()), requestAborted: false);

        // Assert
        Assert.Equal(503, actual.StatusCode);
    }

    [Fact]
    public void Map_ShouldTreatACancellationByTheClientAsClientClosedRequest()
    {
        // Act
        var actual = ExceptionResponseMapper.Map(new TaskCanceledException(), requestAborted: true);

        // Assert
        Assert.Equal(ExceptionResponseMapper.ClientClosedRequest, actual.StatusCode);
        Assert.Equal(LogLevel.Information, actual.LogLevel);
    }

    [Fact]
    public void Map_ShouldTreatACancellationTheClientDidNotAskForAsAServerError()
    {
        // Act: e.g. a timeout inside the API while the client is still waiting
        var actual = ExceptionResponseMapper.Map(new TaskCanceledException(), requestAborted: false);

        // Assert
        Assert.Equal(500, actual.StatusCode);
        Assert.Equal(LogLevel.Error, actual.LogLevel);
    }

    [Fact]
    public void Map_ShouldMapAnUpdateFailureWithoutSqlDetailsToServerError()
    {
        // Act
        var actual = ExceptionResponseMapper.Map(new DbUpdateException("failed"), requestAborted: false);

        // Assert
        Assert.Equal(500, actual.StatusCode);
    }
}
