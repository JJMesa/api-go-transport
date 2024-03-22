using FluentValidation;
using GoTransport.Application.Dtos.User;

namespace GoTransport.Application.Validators.User;

public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateValidator()
    {
        RuleFor(e => e.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(e => e.FirstName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(e => e.LastName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.IsActive)
            .NotNull();

        RuleFor(e => e.RoleId)
            .NotNull()
            .NotEmpty();
    }
}