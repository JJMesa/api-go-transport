namespace GoTransport.Domain.Interfaces;

/// <summary>
/// The signed-in user making the current change, for the audit columns. Implemented by the Api from the
/// request; null outside a request (startup bootstrap, seeds).
/// </summary>
public interface ICurrentUser
{
    string? UserId { get; }
}
