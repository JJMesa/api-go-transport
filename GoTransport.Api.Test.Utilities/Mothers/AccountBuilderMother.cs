using GoTransport.Application.Dtos.Account;
using GoTransport.Application.Settings;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Api.Test.Utilities.Mothers;

public static class AccountBuilderMother
{
    public const string ValidEmail = "admin@gotransport.com";
    public const string ValidPassword = "Password123*";

    public static UserLoginDto UserLoginDtoOk()
    {
        return new UserLoginDto
        {
            Email = ValidEmail,
            Password = ValidPassword
        };
    }

    public static User UserEntity()
    {
        return new User
        {
            Id = 1,
            Email = ValidEmail,
            UserName = ValidEmail,
            FirstName = "Admin",
            LastName = "User",
            IsActive = true
        };
    }

    public static JwtSettings JwtSettings()
    {
        return new JwtSettings
        {
            Secret = "unit-tests-dummy-secret-value-with-enough-length-for-hs256-signing",
            ExpiryTime = TimeSpan.FromHours(1),
            TokenIssuer = "GoTransport.UnitTests",
            TokenAudience = "GoTransport.UnitTests"
        };
    }
}
