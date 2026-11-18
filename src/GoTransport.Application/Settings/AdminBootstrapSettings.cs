namespace GoTransport.Application.Settings;

/// <summary>
/// Credentials for the first Administrador user, created at startup only while no administrator exists.
/// Supply them through User Secrets or environment variables, never in a committed appsettings file.
/// </summary>
public class AdminBootstrapSettings
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = "Admin";
    public string LastName { get; set; } = "GoTransport";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password);
}
