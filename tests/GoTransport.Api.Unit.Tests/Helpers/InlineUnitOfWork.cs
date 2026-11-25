using System.Data;
using GoTransport.Domain.Interfaces;

namespace GoTransport.Api.Unit.Tests.Helpers;

/// <summary>
/// Runs the operation directly and records the isolation level each transaction asked for.
/// </summary>
internal sealed class InlineUnitOfWork : IUnitOfWork
{
    public List<IsolationLevel> Transactions { get; } = new();

    public Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, Func<TResult, bool> commitWhen,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        Transactions.Add(isolationLevel);
        return operation();
    }
}
