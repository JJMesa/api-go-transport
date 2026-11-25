using GoTransport.Domain.Entities.App;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace GoTransport.Api.Unit.Tests.Helpers;

/// <summary>
/// Moq doubles for the Identity managers, whose constructors take many collaborators the tests never use.
/// </summary>
internal static class IdentityMocks
{
    public static Mock<UserManager<User>> UserManager()
    {
        var storeMock = new Mock<IUserStore<User>>();
        return new Mock<UserManager<User>>(storeMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    public static Mock<RoleManager<Role>> RoleManager()
    {
        var storeMock = new Mock<IRoleStore<Role>>();
        return new Mock<RoleManager<Role>>(storeMock.Object, null!, null!, null!, null!);
    }

    public static Mock<SignInManager<User>> SignInManager(Mock<UserManager<User>> userManagerMock)
    {
        return new Mock<SignInManager<User>>(
            userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            Options.Create(new IdentityOptions()),
            Mock.Of<ILogger<SignInManager<User>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<User>>());
    }
}
