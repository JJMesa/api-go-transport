using Ardalis.Specification;
using GoTransport.Application.Extensions;
using GoTransport.Application.Parameters;
using GoTransport.Domain.Entities.App;

namespace GoTransport.Application.Specifications.Users;

/// <summary>
/// The filters a page of User rows and its total count share, so the page and the count always agree.
/// </summary>
internal static class UserFilter
{
    public static void Apply(ISpecificationBuilder<User> query, UserParameters parameters)
    {
        if (parameters.RoleId.HasValue)
            query.Where(user => user.UserRoles.Any(role => role.RoleId == parameters.RoleId.Value));

        if (!string.IsNullOrWhiteSpace(parameters.SearchCriteria))
        {
            query.Search(user => user.FirstName, parameters.SearchCriteria.ToContainsPattern());
            query.Search(user => user.LastName, parameters.SearchCriteria.ToContainsPattern());
        }

        if (parameters.IsActive.HasValue)
            query.Where(user => user.IsActive == parameters.IsActive);
    }
}
