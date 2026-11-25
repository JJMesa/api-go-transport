using FluentValidation;
using GoTransport.Application.Commons;
using GoTransport.Application.Dtos.User;

namespace GoTransport.Application.Validators.User;

public class UserCreationValidator : AbstractValidator<UserCreationDto>
{
    public UserCreationValidator()
    {
        RuleFor(e => e.FirstName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(e => e.LastName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty()
            .MaximumLength(256);

        // Mirrors PasswordPolicy, which Identity enforces when the password is stored.
        RuleFor(x => x.Password)
            .NotNull()
            .NotEmpty()
            .MinimumLength(PasswordPolicy.RequiredLength)
            .Matches("[A-Z]").WithMessage("'{PropertyName}' debe contener al menos una letra mayúscula.")
            .Matches("[a-z]").WithMessage("'{PropertyName}' debe contener al menos una letra minúscula.")
            .Matches(@"\d").WithMessage("'{PropertyName}' debe contener al menos un número.")
            .Matches("[^a-zA-Z0-9]").WithMessage("'{PropertyName}' debe contener al menos un símbolo o carácter especial.")
            .Must(password => password is null || password.Distinct().Count() >= PasswordPolicy.RequiredUniqueChars)
                .WithMessage($"'{{PropertyName}}' must contain at least {PasswordPolicy.RequiredUniqueChars} different characters.")
            .Matches("^[^£#“”]*$").WithMessage("'{PropertyName}' no debe contener los siguientes caracteres especiales: £, #, “”.");
    }
}