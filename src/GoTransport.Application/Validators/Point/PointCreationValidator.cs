using FluentValidation;
using GoTransport.Application.Dtos.Point;

namespace GoTransport.Application.Validators.Point;

public class PointCreationValidator : AbstractValidator<PointCreationDto>
{
    public PointCreationValidator()
    {
        RuleFor(x => x.CityId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Detail)
            .NotNull()
            .NotEmpty()
            .MaximumLength(64);
    }
}