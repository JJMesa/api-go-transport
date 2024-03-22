using FluentValidation;
using GoTransport.Application.Dtos.Schedule;

namespace GoTransport.Application.Validators.Schedule;

public class ScheduleUpdateValidator : AbstractValidator<ScheduleUpdateDto>
{
    public ScheduleUpdateValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.DepartureTime)
            .NotNull();

        RuleFor(x => x.ArrivalTime)
            .NotNull();

        RuleFor(x => x.VehicleId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.RouteId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.IsActive)
            .NotNull();
    }
}