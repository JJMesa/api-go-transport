using System.Net;
using GoTransport.Application.Commons;
using GoTransport.Application.Enums;
using GoTransport.Application.Services;
using GoTransport.Application.Settings;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class AdminBootstrapServiceTests
{
    private const string AdminEmail = "admin@gotransport.test";
    private const string AdminPassword = "Bootstrap_Pass1";

    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<RoleManager<Role>> _roleManagerMock;

    public AdminBootstrapServiceTests()
    {
        _userManagerMock = CreateUserManagerMock();
        _roleManagerMock = CreateRoleManagerMock();
    }

    #region EnsureAdministratorAsync

    [Fact]
    public async Task EnsureAdministratorAsync_ShouldSkipWhenCredentialsAreNotConfigured()
    {
        // Arrange
        var service = CreateService(new AdminBootstrapSettings());

        // Act
        var actual = await service.EnsureAdministratorAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.Equal(AdminBootstrapResult.NotConfigured, actual.Data);
        _userManagerMock.Verify(x => x.GetUsersInRoleAsync(It.IsAny<string>()), Times.Never);
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task EnsureAdministratorAsync_ShouldSkipWhenAnAdministratorAlreadyExists()
    {
        // Arrange
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(Roles.Administrator)).ReturnsAsync(new List<User> { new() });
        var service = CreateService(ConfiguredSettings());

        // Act
        var actual = await service.EnsureAdministratorAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.Equal(AdminBootstrapResult.AlreadyExists, actual.Data);
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task EnsureAdministratorAsync_ShouldCreateAdministratorWhenNoneExists()
    {
        // Arrange
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(Roles.Administrator)).ReturnsAsync(new List<User>());
        _roleManagerMock.Setup(x => x.RoleExistsAsync(Roles.Administrator)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), AdminPassword)).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), Roles.Administrator)).ReturnsAsync(IdentityResult.Success);
        var service = CreateService(ConfiguredSettings());

        // Act
        var actual = await service.EnsureAdministratorAsync();

        // Assert
        Assert.Equal(HttpStatusCode.Created, actual.HttpCode);
        Assert.Equal(AdminBootstrapResult.Created, actual.Data);
        _userManagerMock.Verify(x => x.CreateAsync(It.Is<User>(u => u.Email == AdminEmail && u.UserName == AdminEmail), AdminPassword), Times.Once);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.Is<User>(u => u.Email == AdminEmail), Roles.Administrator), Times.Once);
    }

    [Fact]
    public async Task EnsureAdministratorAsync_ShouldReturnBadRequestWhenAdministratorRoleIsMissing()
    {
        // Arrange
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(Roles.Administrator)).ReturnsAsync(new List<User>());
        _roleManagerMock.Setup(x => x.RoleExistsAsync(Roles.Administrator)).ReturnsAsync(false);
        var service = CreateService(ConfiguredSettings());

        // Act
        var actual = await service.EnsureAdministratorAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(ErrorMessages.AdministratorRoleMissing, actual.Errors);
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task EnsureAdministratorAsync_ShouldReturnBadRequestWhenPasswordIsRejected()
    {
        // Arrange
        var passwordError = new IdentityError { Code = "PasswordTooShort", Description = "Passwords must be at least 6 characters." };
        _userManagerMock.Setup(x => x.GetUsersInRoleAsync(Roles.Administrator)).ReturnsAsync(new List<User>());
        _roleManagerMock.Setup(x => x.RoleExistsAsync(Roles.Administrator)).ReturnsAsync(true);
        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), AdminPassword)).ReturnsAsync(IdentityResult.Failed(passwordError));
        var service = CreateService(ConfiguredSettings());

        // Act
        var actual = await service.EnsureAdministratorAsync();

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(passwordError.Description, actual.Errors);
        _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    #endregion EnsureAdministratorAsync

    #region Helpers

    private AdminBootstrapService CreateService(AdminBootstrapSettings settings) =>
        new(_userManagerMock.Object, _roleManagerMock.Object, Options.Create(settings), NullLogger<AdminBootstrapService>.Instance);

    private static AdminBootstrapSettings ConfiguredSettings() => new()
    {
        Email = AdminEmail,
        Password = AdminPassword
    };

    private static Mock<UserManager<User>> CreateUserManagerMock()
    {
        var storeMock = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    private static Mock<RoleManager<Role>> CreateRoleManagerMock()
    {
        var storeMock = new Mock<IRoleStore<Role>>();
        return new Mock<RoleManager<Role>>(storeMock.Object, null!, null!, null!, null!);
    }

    #endregion Helpers
}
