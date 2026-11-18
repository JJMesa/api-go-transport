namespace GoTransport.Application.Dtos.Account;

public class UserLoginDto
{
    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;
}