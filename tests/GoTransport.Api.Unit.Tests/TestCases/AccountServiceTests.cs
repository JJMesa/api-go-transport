using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using GoTransport.Api.Test.Utilities.Mothers;
using GoTransport.Api.Unit.Tests.Helpers;
using GoTransport.Application.Commons;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Services;
using GoTransport.Application.Settings;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class AccountServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly IAccountService _accountService;

    public AccountServiceTests()
    {
        _userManagerMock = IdentityMocks.UserManager();
        _signInManagerMock = IdentityMocks.SignInManager(_userManagerMock);

        var jwtSettingsMock = new Mock<IOptionsSnapshot<JwtSettings>>();
        jwtSettingsMock.Setup(x => x.Value).Returns(AccountBuilderMother.JwtSettings());

        _accountService = new AccountService(_userManagerMock.Object, _signInManagerMock.Object, jwtSettingsMock.Object);
    }

    #region LoginAsync

    [Fact]
    public async Task LoginAsync_ShouldReturnTokenForValidCredentials()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { Roles.Administrator });
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, It.IsAny<string>(), true)).ReturnsAsync(SignInResult.Success);

        // Act
        var actual = await _accountService.LoginAsync(AccountBuilderMother.UserLoginDtoOk());

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.NotNull(actual.Data);
        Assert.False(string.IsNullOrWhiteSpace(actual.Data!.Token));
    }

    [Fact]
    public async Task LoginAsync_ShouldPutTheSecurityStampInTheToken()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.GetRolesAsync(user)).ReturnsAsync(new List<string> { Roles.Administrator });
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, It.IsAny<string>(), true)).ReturnsAsync(SignInResult.Success);

        // Act
        var actual = await _accountService.LoginAsync(AccountBuilderMother.UserLoginDtoOk());

        // Assert
        var token = new JwtSecurityTokenHandler().ReadJwtToken(actual.Data!.Token);
        Assert.Contains(token.Claims, claim => claim.Type == UserCustomClaims.SecurityStamp && claim.Value == user.SecurityStamp);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnBadRequestWhenUserDoesNotExist()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((User)null!);

        // Act
        var actual = await _accountService.LoginAsync(AccountBuilderMother.UserLoginDtoOk());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.InvalidCredentials, actual.Errors!);
        _signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public async Task LoginAsync_ShouldReturnBadRequestForInactiveUser(bool? isActive)
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        user.IsActive = isActive;
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, It.IsAny<string>(), true)).ReturnsAsync(SignInResult.Success);

        // Act
        var actual = await _accountService.LoginAsync(AccountBuilderMother.UserLoginDtoOk());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.InvalidCredentials, actual.Errors!);
        _signInManagerMock.Verify(x => x.CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnBadRequestForInvalidPassword()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, It.IsAny<string>(), true)).ReturnsAsync(SignInResult.Failed);

        // Act
        var actual = await _accountService.LoginAsync(AccountBuilderMother.UserLoginDtoOk());

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.InvalidCredentials, actual.Errors!);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnTooManyRequestsWhenAccountIsLockedOut()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        _userManagerMock.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
        _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, It.IsAny<string>(), true)).ReturnsAsync(SignInResult.LockedOut);

        // Act
        var actual = await _accountService.LoginAsync(AccountBuilderMother.UserLoginDtoOk());

        // Assert
        Assert.Equal(HttpStatusCode.TooManyRequests, actual.HttpCode);
        Assert.Null(actual.Data);
        Assert.Contains(ErrorMessages.UserLockedOut, actual.Errors!);
    }

    #endregion LoginAsync

    #region IsTokenUserValidAsync

    [Fact]
    public async Task IsTokenUserValidAsync_ShouldAcceptActiveUserWithCurrentStamp()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        // Act
        var actual = await _accountService.IsTokenUserValidAsync(TokenPrincipal(user.Id, user.SecurityStamp));

        // Assert
        Assert.True(actual);
    }

    [Fact]
    public async Task IsTokenUserValidAsync_ShouldRejectDeactivatedUser()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        user.IsActive = false;
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        // Act
        var actual = await _accountService.IsTokenUserValidAsync(TokenPrincipal(user.Id, user.SecurityStamp));

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task IsTokenUserValidAsync_ShouldRejectTokenWithOutdatedStamp()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        // Act
        var actual = await _accountService.IsTokenUserValidAsync(TokenPrincipal(user.Id, "stamp-before-the-last-update"));

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task IsTokenUserValidAsync_ShouldRejectTokenWithoutStamp()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);

        // Act
        var actual = await _accountService.IsTokenUserValidAsync(TokenPrincipal(user.Id, securityStamp: null));

        // Assert
        Assert.False(actual);
    }

    [Fact]
    public async Task IsTokenUserValidAsync_ShouldRejectUnknownUser()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null!);

        // Act
        var actual = await _accountService.IsTokenUserValidAsync(TokenPrincipal(99, "any-stamp"));

        // Assert
        Assert.False(actual);
    }

    #endregion IsTokenUserValidAsync

    #region Helpers

    private static ClaimsPrincipal TokenPrincipal(long userId, string? securityStamp)
    {
        var claims = new List<Claim> { new(UserCustomClaims.Id, userId.ToString()) };
        if (securityStamp is not null) claims.Add(new Claim(UserCustomClaims.SecurityStamp, securityStamp));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));
    }

    #endregion Helpers
}
