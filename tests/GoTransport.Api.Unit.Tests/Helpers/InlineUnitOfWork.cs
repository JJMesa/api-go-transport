using System.Data;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Wrappers;

namespace GoTransport.Api.Unit.Tests.Helpers;

/// <summary>
/// Runs the operation directly and records the isolation level each transaction asked for.
/// </summary>
internal sealed class InlineUnitOfWork : IUnitOfWork
{
    public List<IsolationLevel> Transactions { get; } = new();

    public Task<JsonResponse<T>> ExecuteInTransactionAsync<T>(Func<Task<JsonResponse<T>>> operation, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default)
    {
        Transactions.Add(isolationLevel);
        return operation();
    }
}
