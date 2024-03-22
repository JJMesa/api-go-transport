using Ardalis.Specification;
using GoTransport.Application.Dtos.User;
using GoTransport.Application.Enums;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Users;

public class PagedUserSpecification : Specification<User, UserDto>
{
    public PagedUserSpecification(UserParameters parameters)
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
            Query.Where(user => user.IsActive == parameters.IsActive);

        Query.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize);

        Query.Select(user => new UserDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            IsActive = user.IsActive
        });
    }
}