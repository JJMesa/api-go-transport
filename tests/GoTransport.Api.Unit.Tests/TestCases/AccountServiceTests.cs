using System.Net;
using GoTransport.Api.Test.Utilities.Mothers;
using GoTransport.Application.Commons;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Services;
using GoTransport.Application.Settings;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
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
        _userManagerMock = CreateUserManagerMock();
        _signInManagerMock = CreateSignInManagerMock(_userManagerMock);

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

    #region Helpers

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var storeMock = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<SignInManager<User>> CreateSignInManagerMock(Mock<UserManager<User>> userManagerMock)
    {
        var contextAccessorMock = new Mock<IHttpContextAccessor>();
        var claimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();

        return new Mock<SignInManager<User>>(
            userManagerMock.Object,
            contextAccessorMock.Object,
            claimsFactoryMock.Object,
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<User>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<User>>());
    }

    #endregion Helpers
}
