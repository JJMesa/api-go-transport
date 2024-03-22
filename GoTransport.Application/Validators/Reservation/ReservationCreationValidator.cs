using FluentValidation;
using GoTransport.Application.Dtos.Reservation;

namespace GoTransport.Application.Validators.Reservation;

public class ReservationCreationValidator : AbstractValidator<ReservationCreationDto>
{
    public ReservationCreationValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.ReservationDate)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.PassengerFirstName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.PassengerLastName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.IdentificationTypeId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.PassengerIdentification)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Detail)
            .NotNull()
            .NotEmpty()
            .MaximumLength(64);
    }
}