using FluentValidation;

namespace GoTransport.Application.Extensions;

public static class FluentValidationExtensions
{
    public static void ValidateAndThrowArgumentException<T>(this IValidator<T> validator, T instance)
    {
        var res = validator.Validate(instance);

        if (!res.IsValid)
        {
            throw new ValidationException(res.Errors);
        }
    }
}