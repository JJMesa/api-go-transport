using FluentValidation;
using GoTransport.Application.Dtos.Reservation;

namespace GoTransport.Application.Validators.Reservation;

public class ReservationUpdateValidator : AbstractValidator<ReservationUpdateDto>
{
    public ReservationUpdateValidator()
    {
        RuleFor(x => x.ReservationId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.ScheduleId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.ReservationDate)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Detail)
            .NotNull()
            .NotEmpty()
            .MaximumLength(64);
    }
}