using Ardalis.Specification;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Users;

public class UserSpecification : Specification<User>
{
    public UserSpecification(UserParameters parameters)
    {
        UserFilter.Apply(Query, parameters);
    }
}