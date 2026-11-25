namespace GoTransport.Domain.Common;

public static class AuditDefaults
{
    /// <summary>
    /// The audit user for changes made without a signed-in user (seeds, the startup bootstrap). A persisted
    /// value, so it keeps its Spanish form.
    /// </summary>
    public const string SystemUser = "SISTEMA";
}
