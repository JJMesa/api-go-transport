namespace GoTransport.Application.Dtos.User;

public class UserUpdateDto
{
    public long UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public bool IsActive { get; set; }
    public long RoleId { get; set; }
}