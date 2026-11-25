using System.Data;
using GoTransport.Api.Component.Tests.Infrastructure;
using GoTransport.Domain.Entities.Bas;
using GoTransport.Persistence.Contexts;
using GoTransport.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace GoTransport.Api.Component.Tests.TestCases;

/// <summary>
/// Runs <see cref="UnitOfWork"/> on a relational provider (in-memory SQLite). The other component tests use
/// EF InMemory, which has no transactions, so they only reach its early return. SQLite does not reproduce
/// SQL Server's Serializable range locks: the Capacity oversell race itself still needs a SQL Server run.
/// </summary>
public sealed class UnitOfWorkTests : IDisposable
{
    private const string NewDepartment = "Boyacá";

    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public UnitOfWorkTests()
    {
        // A shared cache, so SQLite honours ReadUncommitted instead of raising it to Serializable; the name
        // keeps each test's in-memory database apart.
        _connection = new SqliteConnection($"DataSource=unit-of-work-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
        _connection.Open();
        _context = SqliteApplicationDbContext.CreateSeeded(_connection,
            sqlite => sqlite.ExecutionStrategy(dependencies => new RetryOnceOnTransientError(dependencies)));
        _unitOfWork = new UnitOfWork(_context);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldCommitWhenCommitWhenAcceptsTheResult()
    {
        // Act
        var result = await _unitOfWork.ExecuteInTransactionAsync(AddDepartmentAsync, ok => ok);

        // Assert
        Assert.True(result);
        Assert.Equal(1, await CountNewDepartmentsAsync());
        Assert.Null(_context.Database.CurrentTransaction);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldRollBackWhenCommitWhenRejectsTheResult()
    {
        // Act: the operation saves, but its result says the work failed
        var result = await _unitOfWork.ExecuteInTransactionAsync(AddDepartmentAsync, _ => false);

        // Assert
        Assert.True(result);
        Assert.Equal(0, await CountNewDepartmentsAsync());
        Assert.Null(_context.Database.CurrentTransaction);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldRollBackWhenTheOperationThrows()
    {
        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => _unitOfWork.ExecuteInTransactionAsync<bool>(async () =>
        {
            await AddDepartmentAsync();
            throw new InvalidOperationException("The operation failed after saving.");
        }, ok => ok));

        // Assert
        Assert.Equal(0, await CountNewDepartmentsAsync());
        Assert.Null(_context.Database.CurrentTransaction);
    }

    [Theory]
    [InlineData(IsolationLevel.Serializable)]
    [InlineData(IsolationLevel.ReadUncommitted)]
    public async Task ExecuteInTransactionAsync_ShouldRunTheOperationInATransactionWithTheRequestedIsolationLevel(IsolationLevel isolationLevel)
    {
        // Arrange
        IsolationLevel? seen = null;

        // Act
        await _unitOfWork.ExecuteInTransactionAsync(() =>
        {
            seen = _context.Database.CurrentTransaction?.GetDbTransaction().IsolationLevel;
            return Task.FromResult(true);
        }, ok => ok, isolationLevel);

        // Assert
        Assert.Equal(isolationLevel, seen);
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldJoinTheOpenTransactionWhenNested()
    {
        // Arrange
        IDbContextTransaction? outer = null;
        IDbContextTransaction? inner = null;

        // Act: the inner call accepts its result, the outer one rejects it
        await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            outer = _context.Database.CurrentTransaction;
            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                inner = _context.Database.CurrentTransaction;
                return await AddDepartmentAsync();
            }, ok => ok);
        }, _ => false);

        // Assert: one transaction, so the outer rollback also undoes the inner work
        Assert.NotNull(outer);
        Assert.Same(outer, inner);
        Assert.Equal(0, await CountNewDepartmentsAsync());
    }

    [Fact]
    public async Task ExecuteInTransactionAsync_ShouldRetryATransientFailureWithACleanChangeTracker()
    {
        // Arrange
        var attempts = 0;

        // Act: the first attempt adds the Department, then fails before saving
        var result = await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            attempts++;
            _context.Add(new Department { Description = NewDepartment });
            if (attempts == 1) throw new TransientTestException();

            await _context.SaveChangesAsync();
            return true;
        }, ok => ok);

        // Assert: the entity the failed attempt added was not saved a second time
        Assert.True(result);
        Assert.Equal(2, attempts);
        Assert.Equal(1, await CountNewDepartmentsAsync());
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<bool> AddDepartmentAsync()
    {
        _context.Add(new Department { Description = NewDepartment });
        await _context.SaveChangesAsync();
        return true;
    }

    private Task<int> CountNewDepartmentsAsync() =>
        _context.Set<Department>().AsNoTracking().CountAsync(department => department.Description == NewDepartment);

    private sealed class TransientTestException : Exception;

    /// <summary>Stands in for SQL Server's retrying strategy: one retry, no delay, on <see cref="TransientTestException"/>.</summary>
    private sealed class RetryOnceOnTransientError : ExecutionStrategy
    {
        public RetryOnceOnTransientError(ExecutionStrategyDependencies dependencies)
            : base(dependencies, maxRetryCount: 1, maxRetryDelay: TimeSpan.Zero)
        {
        }

        protected override bool ShouldRetryOn(Exception exception) => exception is TransientTestException;

        protected override TimeSpan? GetNextDelay(Exception lastException) =>
            base.GetNextDelay(lastException) is null ? null : TimeSpan.Zero;
    }
}
