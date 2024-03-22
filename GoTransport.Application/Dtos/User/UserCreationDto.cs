namespace GoTransport.Application.Dtos.User;

public class UserCreationDto
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public long RoleId { get; set; }
}