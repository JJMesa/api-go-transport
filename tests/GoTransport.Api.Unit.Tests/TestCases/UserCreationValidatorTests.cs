using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.User;
using GoTransport.Application.Validators.User;
using Microsoft.AspNetCore.Identity;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class UserCreationValidatorTests
{
    private readonly UserCreationValidator _validator = new();

    [Theory]
    [InlineData("Transporte-2026")]
    [InlineData("Bus-Medellin-9")]
    public void Validate_ShouldAcceptAPasswordThatMeetsThePolicy(string password)
    {
        // Act
        var actual = _validator.Validate(UserWithPassword(password));

        // Assert
        Assert.True(actual.IsValid, string.Join(" ", actual.Errors));
    }

    [Theory]
    [InlineData("Short-2026")]       // under 12 characters
    [InlineData("transporte-2026")]  // no uppercase letter
    [InlineData("TRANSPORTE-2026")]  // no lowercase letter
    [InlineData("Transporte-Bus")]   // no digit
    [InlineData("Transporte2026")]   // no symbol
    [InlineData("Aa1!Aa1!Aa1!")]     // only 4 distinct characters
    public void Validate_ShouldRejectAPasswordThatBreaksThePolicy(string password)
    {
        // Act
        var actual = _validator.Validate(UserWithPassword(password));

        // Assert
        Assert.Contains(actual.Errors, error => error.PropertyName == nameof(UserCreationDto.Password));
    }

    [Fact]
    public void Apply_ShouldGiveIdentityTheSamePolicy()
    {
        // Arrange
        var options = new PasswordOptions();

        // Act
        PasswordPolicy.Apply(options);

        // Assert
        Assert.Equal(PasswordPolicy.RequiredLength, options.RequiredLength);
        Assert.Equal(PasswordPolicy.RequiredUniqueChars, options.RequiredUniqueChars);
        Assert.True(options.RequireDigit && options.RequireLowercase && options.RequireUppercase && options.RequireNonAlphanumeric);
    }

    private static UserCreationDto UserWithPassword(string password) => new()
    {
        FirstName = "Ana",
        LastName = "Ruiz",
        Email = "ana@gotransport.test",
        Password = password,
        RoleId = 2
    };
}
