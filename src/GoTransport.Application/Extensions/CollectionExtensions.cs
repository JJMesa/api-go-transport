using Microsoft.AspNetCore.Identity;

namespace GoTransport.Application.Extensions;

public static class CollectionExtensions
{
    public static List<string> ToErrorList(this IEnumerable<IdentityError> errors) =>
        errors.Select(error => error.Description).ToList();
}