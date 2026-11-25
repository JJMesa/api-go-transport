using System.Data;

namespace GoTransport.Domain.Interfaces;

/// <summary>
/// Runs several repository calls as one database transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Runs <paramref name="operation"/> in a transaction that commits when <paramref name="commitWhen"/>
    /// accepts its result and rolls back otherwise. A transient database failure can run the operation again
    /// from a clean change tracker, so the operation must load everything it needs itself. Inside an open
    /// transaction, or on a provider without transactions (EF InMemory), the operation just runs.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, Func<TResult, bool> commitWhen,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
}
