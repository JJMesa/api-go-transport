using System.Data;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces.Base;

/// <summary>
/// Runs several repository calls as one database transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Runs <paramref name="operation"/> in a transaction that commits when the returned response is
    /// successful and rolls back otherwise. A transient database failure can run the operation again
    /// from a clean change tracker, so the operation must load everything it needs itself. Inside an
    /// open transaction, or on a provider without transactions (EF InMemory), the operation just runs.
    /// </summary>
    Task<JsonResponse<T>> ExecuteInTransactionAsync<T>(Func<Task<JsonResponse<T>>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default);
}
