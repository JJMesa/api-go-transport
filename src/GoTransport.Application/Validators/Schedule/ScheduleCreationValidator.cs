using FluentValidation;
using GoTransport.Application.Dtos.Schedule;

namespace GoTransport.Application.Validators.Schedule;

public class ScheduleCreationValidator : AbstractValidator<ScheduleCreationDto>
{
    public ScheduleCreationValidator()
    {
        RuleFor(x => x.DepartureTime)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.ArrivalTime)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.VehicleId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.RouteId)
            .NotNull()
            .NotEmpty();
    }
}