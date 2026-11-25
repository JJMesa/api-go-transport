using System.Data;
using GoTransport.Application.Wrappers;
using GoTransport.Domain.Interfaces;

namespace GoTransport.Application.Extensions;

public static class UnitOfWorkExtensions
{
    /// <summary>
    /// Runs a service operation in one transaction that commits only when its response is <c>Ok</c>.
    /// </summary>
    public static Task<JsonResponse<T>> ExecuteInTransactionAsync<T>(this IUnitOfWork unitOfWork, Func<Task<JsonResponse<T>>> operation,
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted, CancellationToken cancellationToken = default) =>
        unitOfWork.ExecuteInTransactionAsync(operation, response => response.Ok, isolationLevel, cancellationToken);
}
