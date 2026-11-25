using FluentValidation;
using GoTransport.Application.Dtos.Schedule;

namespace GoTransport.Application.Validators.Schedule;

public class ScheduleUpdateValidator : AbstractValidator<ScheduleUpdateDto>
{
    private static readonly TimeSpan MaximumTimeOfDay = new(23, 59, 59);

    public ScheduleUpdateValidator()
    {
        RuleFor(x => x.ScheduleId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.DepartureTime)
            .InclusiveBetween(TimeSpan.Zero, MaximumTimeOfDay);

        // An arrival earlier than the departure is on the next day (an overnight Schedule).
        RuleFor(x => x.ArrivalTime)
            .InclusiveBetween(TimeSpan.Zero, MaximumTimeOfDay)
            .NotEqual(x => x.DepartureTime);

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