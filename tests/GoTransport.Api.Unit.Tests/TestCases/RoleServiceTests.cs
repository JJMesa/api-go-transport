using GoTransport.Api.Unit.Tests.Helpers;
using GoTransport.Application.Commons;
using GoTransport.Application.Interfaces;
using GoTransport.Application.Services;
using GoTransport.Application.Specifications.Roles;
using GoTransport.Domain.Entities.App;
using GoTransport.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace GoTransport.Api.Unit.Tests.TestCases;

public class RoleServiceTests
{
    private const long OperatorRoleId = 2;

    private readonly Mock<UserManager<User>> _userManagerMock = IdentityMocks.UserManager();
    private readonly Mock<IRepository<Role>> _roleRepositoryMock = new();
    private readonly IRoleService _roleService;
    private readonly User _user = new() { Id = 7, Email = "ana@gotransport.test", UserName = "ana@gotransport.test" };

    public RoleServiceTests()
    {
        _roleService = new RoleService(_userManagerMock.Object, _roleRepositoryMock.Object, new Mock<ICacheService<Role>>().Object);
    }

    #region AssignRolesToUser

    [Fact]
    public async Task AssignRolesToUser_ShouldReplaceTheCurrentRoleWithTheRequestedOne()
    {
        // Arrange
        ArrangeRole(new Role { Id = OperatorRoleId, Name = Roles.Operator });
        _userManagerMock.Setup(x => x.GetRolesAsync(_user)).ReturnsAsync(new List<string> { Roles.Administrator });
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(_user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.AddToRolesAsync(_user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var actual = await _roleService.AssignRolesToUser(_user, OperatorRoleId, CancellationToken.None);

        // Assert
        Assert.True(actual.Succeeded);
        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(_user, It.Is<IEnumerable<string>>(roles => roles.Single() == Roles.Administrator)), Times.Once);
        _userManagerMock.Verify(x => x.AddToRolesAsync(_user, It.Is<IEnumerable<string>>(roles => roles.Single() == Roles.Operator)), Times.Once);
    }

    [Fact]
    public async Task AssignRolesToUser_ShouldOnlyAddForAUserWithoutRoles()
    {
        // Arrange
        ArrangeRole(new Role { Id = OperatorRoleId, Name = Roles.Operator });
        _userManagerMock.Setup(x => x.GetRolesAsync(_user)).ReturnsAsync(new List<string>());
        _userManagerMock.Setup(x => x.AddToRolesAsync(_user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var actual = await _roleService.AssignRolesToUser(_user, OperatorRoleId, CancellationToken.None);

        // Assert
        Assert.True(actual.Succeeded);
        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task AssignRolesToUser_ShouldFailForAnUnknownRoleAndKeepTheCurrentOne()
    {
        // Arrange: no Role with that id
        _roleRepositoryMock.Setup(x => x.ListAsync(It.IsAny<RoleSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Role>());

        // Act
        var actual = await _roleService.AssignRolesToUser(_user, 99, CancellationToken.None);

        // Assert
        Assert.False(actual.Succeeded);
        Assert.Contains(actual.Errors, error => error.Description == ErrorMessages.UnknownRole);
        _userManagerMock.Verify(x => x.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _userManagerMock.Verify(x => x.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task AssignRolesToUser_ShouldStopWhenTheCurrentRoleCannotBeRemoved()
    {
        // Arrange
        var removeError = new IdentityError { Code = "RemoveFailed", Description = "The current role could not be removed." };
        ArrangeRole(new Role { Id = OperatorRoleId, Name = Roles.Operator });
        _userManagerMock.Setup(x => x.GetRolesAsync(_user)).ReturnsAsync(new List<string> { Roles.Administrator });
        _userManagerMock.Setup(x => x.RemoveFromRolesAsync(_user, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Failed(removeError));

        // Act
        var actual = await _roleService.AssignRolesToUser(_user, OperatorRoleId, CancellationToken.None);

        // Assert
        Assert.False(actual.Succeeded);
        _userManagerMock.Verify(x => x.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    #endregion AssignRolesToUser

    #region RoleExistsAsync

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RoleExistsAsync_ShouldAskTheRepository(bool exists)
    {
        // Arrange
        _roleRepositoryMock.Setup(x => x.AnyAsync(It.IsAny<RoleSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(exists);

        // Act
        var actual = await _roleService.RoleExistsAsync(OperatorRoleId, CancellationToken.None);

        // Assert
        Assert.Equal(exists, actual);
    }

    #endregion RoleExistsAsync

    private void ArrangeRole(Role role) =>
        _roleRepositoryMock.Setup(x => x.ListAsync(It.IsAny<RoleSpecification>(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<Role> { role });
}
