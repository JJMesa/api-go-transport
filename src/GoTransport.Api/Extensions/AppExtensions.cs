using GoTransport.Api.Middlewares;
using GoTransport.Application.Interfaces;

namespace GoTransport.Api.Extensions;

public static class AppExtensions
{
    public static void UseErrorHandlerMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ErrorHandlerMiddleware>();
    }

    /// <summary>
    /// Creates the first Administrador user from the AdminBootstrap settings when none exists.
    /// A failed bootstrap stops startup so a misconfiguration never leaves the API without an administrator.
    /// </summary>
    public static async Task EnsureAdministratorAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var adminBootstrap = scope.ServiceProvider.GetRequiredService<IAdminBootstrapService>();

        var response = await adminBootstrap.EnsureAdministratorAsync();
        if (!response.Ok)
            throw new InvalidOperationException($"Administrator bootstrap failed: {string.Join(" ", response.Errors)}");
    }
}