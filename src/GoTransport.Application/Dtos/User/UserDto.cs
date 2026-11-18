namespace GoTransport.Application.Dtos.User;

public class UserDto
{
    public long UserId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool? IsActive { get; set; }
}