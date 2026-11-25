using Microsoft.AspNetCore.Identity;

namespace GoTransport.Application.Commons;

/// <summary>
/// The one password policy. Identity enforces it on every password it stores (including the bootstrap
/// administrator's), and <c>UserCreationValidator</c> reports the same rules on input.
/// </summary>
public static class PasswordPolicy
{
    public const int RequiredLength = 12;
    public const int RequiredUniqueChars = 6;

    /// <summary>
    /// Copies the policy onto Identity's password options.
    /// </summary>
    public static void Apply(PasswordOptions options)
    {
        options.RequiredLength = RequiredLength;
        options.RequiredUniqueChars = RequiredUniqueChars;
        options.RequireDigit = true;
        options.RequireLowercase = true;
        options.RequireUppercase = true;
        options.RequireNonAlphanumeric = true;
    }
}
