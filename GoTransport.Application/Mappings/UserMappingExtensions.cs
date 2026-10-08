using GoTransport.Application.Dtos.User;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Mappings;

internal static class UserMappingExtensions
{
    public static UserDto ToDto(this User user) => new()
    {
        UserId = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        IsActive = user.IsActive
    };

    /// <summary>
    /// Maps a creation request to a new user. The email doubles as the Identity user name.
    /// </summary>
    public static User ToEntity(this UserCreationDto userCreation) => new()
    {
        FirstName = userCreation.FirstName,
        LastName = userCreation.LastName,
        Email = userCreation.Email,
        UserName = userCreation.Email
    };

    public static void ApplyTo(this UserUpdateDto userUpdate, User user)
    {
        user.FirstName = userUpdate.FirstName;
        user.LastName = userUpdate.LastName;
        user.IsActive = userUpdate.IsActive;
    }
}
