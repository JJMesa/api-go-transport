using FluentValidation;
using GoTransport.Application.Dtos.Reservation;

namespace GoTransport.Application.Validators.Reservation;

public class PassengerLookupValidator : AbstractValidator<PassengerLookupDto>
{
    public PassengerLookupValidator()
    {
        RuleFor(x => x.PassengerIdentification)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);
    }
}
