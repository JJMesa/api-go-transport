using System.Net;
using GoTransport.Api.Test.Utilities.Mothers;
using GoTransport.Api.Unit.Tests.Helpers;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.User;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Interfaces.Base;
using GoTransport.Application.Services;
using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class UserServiceTests
{
    private const long OperatorRoleId = 2;

    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IRepository<User>> _userRepositoryMock;
    private readonly Mock<IRoleService> _roleServiceMock;
    private readonly IUserService _userService;

    public UserServiceTests()
    {
        _userManagerMock = IdentityMocks.UserManager();
        _userRepositoryMock = new Mock<IRepository<User>>();
        _roleServiceMock = new Mock<IRoleService>();
        _userService = new UserService(_userManagerMock.Object, _userRepositoryMock.Object, _roleServiceMock.Object);
    }

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldAssignTheRequestedRole()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        ArrangeSuccessfulUpdate(user);

        // Act
        var actual = await _userService.UpdateAsync(user.Id, AccountBuilderMother.UserUpdateDtoOk(user.Id, OperatorRoleId));

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        _roleServiceMock.Verify(x => x.AssignRolesToUser(user, OperatorRoleId), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldSaveThroughUserManagerAndRefreshTheSecurityStamp()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        ArrangeSuccessfulUpdate(user);

        // Act
        var actual = await _userService.UpdateAsync(user.Id, AccountBuilderMother.UserUpdateDtoOk(user.Id, OperatorRoleId, isActive: false));

        // Assert
        Assert.Equal(HttpStatusCode.OK, actual.HttpCode);
        Assert.False(actual.Data!.IsActive);
        _userManagerMock.Verify(x => x.UpdateSecurityStampAsync(It.Is<User>(u => u.IsActive == false)), Times.Once);
        _userRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnNotFoundForUnknownUser()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((User)null!);

        // Act
        var actual = await _userService.UpdateAsync(99, AccountBuilderMother.UserUpdateDtoOk(99, OperatorRoleId));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, actual.HttpCode);
        _roleServiceMock.Verify(x => x.AssignRolesToUser(It.IsAny<User>(), It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestWhenUrlAndBodyIdsDiffer()
    {
        // Act
        var actual = await _userService.UpdateAsync(1, AccountBuilderMother.UserUpdateDtoOk(2, OperatorRoleId));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(ErrorMessages.UrlAndBodyIdNotEqual, actual.Errors);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestWhenRoleAssignmentFails()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        var roleError = new IdentityError { Code = "RoleError", Description = "Role could not be assigned." };
        ArrangeSuccessfulUpdate(user);
        _roleServiceMock.Setup(x => x.AssignRolesToUser(user, OperatorRoleId)).ReturnsAsync(IdentityResult.Failed(roleError));

        // Act
        var actual = await _userService.UpdateAsync(user.Id, AccountBuilderMother.UserUpdateDtoOk(user.Id, OperatorRoleId));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(roleError.Description, actual.Errors);
    }

    #endregion UpdateAsync

    [Fact]
    public async Task UpdateAsync_ShouldReturnBadRequestForUnknownRole()
    {
        // Arrange
        var user = AccountBuilderMother.UserEntity();
        ArrangeSuccessfulUpdate(user);
        _roleServiceMock.Setup(x => x.RoleExistsAsync(OperatorRoleId)).ReturnsAsync(false);

        // Act
        var actual = await _userService.UpdateAsync(user.Id, AccountBuilderMother.UserUpdateDtoOk(user.Id, OperatorRoleId));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(ErrorMessages.UnknownRole, actual.Errors);
        _userManagerMock.Verify(x => x.UpdateSecurityStampAsync(It.IsAny<User>()), Times.Never);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_ShouldReturnBadRequestForUnknownRoleWithoutCreatingTheUser()
    {
        // Arrange
        var creation = new UserCreationDto { FirstName = "Ana", LastName = "Ruiz", Email = "ana@gotransport.test", Password = "Secret#123", RoleId = 99 };
        _roleServiceMock.Setup(x => x.RoleExistsAsync(99)).ReturnsAsync(false);

        // Act
        var actual = await _userService.CreateAsync(creation);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, actual.HttpCode);
        Assert.Contains(ErrorMessages.UnknownRole, actual.Errors);
        _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    #endregion CreateAsync

    #region Helpers

    private void ArrangeSuccessfulUpdate(User user)
    {
        _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString())).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);
        _roleServiceMock.Setup(x => x.AssignRolesToUser(user, It.IsAny<long>())).ReturnsAsync(IdentityResult.Success);
        _roleServiceMock.Setup(x => x.RoleExistsAsync(It.IsAny<long>())).ReturnsAsync(true);
    }

    #endregion Helpers
}
