using Ardalis.Specification;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Roles;

public class RoleSpecification : Specification<Role>
{
    public RoleSpecification(long roleId)
    {
        Query.Where(x => x.Id == roleId);
    }

    public RoleSpecification(User user)
    {
        Query.Where(role => role.UserRoles.Any(userRole => userRole.UserId == user.Id));
    }
}