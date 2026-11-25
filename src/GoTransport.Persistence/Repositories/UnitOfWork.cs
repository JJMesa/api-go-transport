using System.Data;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Wrappers;
using GoTransport.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace GoTransport.Persistence.Repositories;

internal class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JsonResponse<T>> ExecuteInTransactionAsync<T>(Func<Task<JsonResponse<T>>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        // EF InMemory has no transactions, and a nested call joins the transaction already open.
        if (!_context.Database.IsRelational() || _context.Database.CurrentTransaction is not null)
            return await operation();

        var strategy = _context.Database.CreateExecutionStrategy();
        var isRetry = false;

        return await strategy.ExecuteAsync(async ct =>
        {
            // A retried attempt starts clean, so entities added by the failed attempt are not saved twice.
            if (isRetry) _context.ChangeTracker.Clear();
            isRetry = true;

            await using var transaction = await _context.Database.BeginTransactionAsync(isolationLevel, ct);
            var response = await operation();

            if (response.Ok) await transaction.CommitAsync(ct);
            else await transaction.RollbackAsync(ct);

            return response;
        }, cancellationToken);
    }
}
