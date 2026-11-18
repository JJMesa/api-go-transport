using FluentValidation;
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

        RuleFor(x => x.Password)
            .NotNull()
            .NotEmpty()
            .MinimumLength(6)
            .Matches("[A-Z]").WithMessage("'{PropertyName}' debe contener al menos una letra mayúscula.")
            .Matches("[a-z]").WithMessage("'{PropertyName}' debe contener al menos una letra minúscula.")
            .Matches(@"\d").WithMessage("'{PropertyName}' debe contener al menos un número.")
            .Matches(@"[][""!@$%^&*(){}:;<>,.?/+_=|'~\\-]").WithMessage("'{PropertyName}' debe contener al menos un símbolo o carácter especial.")
            .Matches("^[^£#“”]*$").WithMessage("'{PropertyName}' no debe contener los siguientes caracteres especiales: £, #, “”.");
    }
}