using FluentValidation;
using GoTransport.Application.Dtos.Route;

namespace GoTransport.Application.Validators.Route;

public class RouteUpdateValidator : AbstractValidator<RouteUpdateDto>
{
    public RouteUpdateValidator()
    {
        RuleFor(x => x.RouteId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.OriginPointId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.DestinationPointId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.IsActive)
            .NotNull();
    }
}