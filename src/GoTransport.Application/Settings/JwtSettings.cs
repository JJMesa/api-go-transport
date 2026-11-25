using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GoTransport.Application.Settings;

public class JwtSettings
{
    /// <summary>Tolerance for clock differences between the token issuer and the API.</summary>
    private static readonly TimeSpan AllowedClockSkew = TimeSpan.FromSeconds(30);

    public string Secret { get; set; } = null!;
    public TimeSpan ExpiryTime { get; set; }
    public string TokenIssuer { get; set; } = null!;
    public string TokenAudience { get; set; } = null!;

    /// <summary>
    /// The one key that both signs tokens (AccountService) and validates them (the JWT bearer handler).
    /// </summary>
    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(Secret));

    /// <summary>
    /// The rules the API applies to every incoming token, built from the same settings that issue it.
    /// </summary>
    public TokenValidationParameters CreateValidationParameters() => new()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = CreateSigningKey(),
        ValidateIssuer = true,
        ValidIssuer = TokenIssuer,
        ValidateAudience = true,
        ValidAudience = TokenAudience,
        RequireExpirationTime = true,
        ValidateLifetime = true,
        ClockSkew = AllowedClockSkew
    };
}