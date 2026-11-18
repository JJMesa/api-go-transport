namespace GoTransport.Application.Settings;

public class JwtSettings
{
    public string Secret { get; set; } = null!;
    public TimeSpan ExpiryTime { get; set; }
    public string TokenIssuer { get; set; } = null!;
    public string TokenAudience { get; set; } = null!;
}