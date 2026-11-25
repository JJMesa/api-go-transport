using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GoTransport.Application.Settings;

public class JwtSettings
{
    /// <summary>Tolerance for clock differences between the token issuer and the API.</summary>
    private static readonly TimeSpan AllowedClockSkew = TimeSpan.FromSeconds(30);

    /// <summary>HS256 needs a key of at least 256 bits, so the secret must have 32 bytes or more.</summary>
    [Required, MinLength(32)]
    public string Secret { get; set; } = null!;

    [Range(typeof(TimeSpan), "00:01:00", "7.00:00:00")]
    public TimeSpan ExpiryTime { get; set; }

    [Required]
    public string TokenIssuer { get; set; } = null!;

    [Required]
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