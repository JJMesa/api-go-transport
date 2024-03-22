using FluentValidation;
using GoTransport.Application.Dtos.Route;

namespace GoTransport.Application.Validators.Route;

public class RouteCreationValidator : AbstractValidator<RouteCreationDto>
{
    public RouteCreationValidator()
    {
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
    }
}