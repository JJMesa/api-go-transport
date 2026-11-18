namespace GoTransport.Application.Dtos.Account;

public class UserTokenDto
{
    public string Token { get; set; } = null!;

    public DateTime Expires { get; set; }
}