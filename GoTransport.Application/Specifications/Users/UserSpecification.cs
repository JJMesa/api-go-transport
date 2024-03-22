using Ardalis.Specification;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Users;

public class UserSpecification : Specification<User>
{
    public UserSpecification(UserParameters parameters)
    {
        if (parameters.RoleId.HasValue)
            Query.Where(user => user.UserRoles.Any(role => role.RoleId == parameters.RoleId.Value));

        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
        {
            Query.Search(user => $"{user.FirstName}", $"%{parameters.SearchCriteria}%");
            Query.Search(user => $"{user.LastName}", $"%{parameters.SearchCriteria}%");
        }

        switch (parameters.OrderBy)
        {
            case SortDirection.Ascending:
                Query.OrderBy(user => user.FirstName)
                    .ThenBy(user => user.LastName);
                break;

            case SortDirection.Descending:
                Query.OrderByDescending(user => user.FirstName)
                    .ThenByDescending(user => user.LastName);
                break;

            default:
                Query.OrderBy(user => user.FirstName)
                    .ThenBy(user => user.LastName);
                break;
        }

        if (parameters.IsActive.HasValue)
            Query.Where(city => city.IsActive == parameters.IsActive);
    }
}