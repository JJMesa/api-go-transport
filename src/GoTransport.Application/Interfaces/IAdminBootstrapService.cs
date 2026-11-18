using GoTransport.Application.Enums;
using GoTransport.Application.Wrappers;

namespace GoTransport.Application.Interfaces;

public interface IAdminBootstrapService
{
    /// <summary>
    /// Creates the first Administrador user from <c>AdminBootstrapSettings</c> when no administrator exists.
    /// </summary>
    Task<JsonResponse<AdminBootstrapResult>> EnsureAdministratorAsync();
}
