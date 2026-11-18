using FluentValidation;
using GoTransport.Application.Dtos.Point;

namespace GoTransport.Application.Validators.Point;

public class PointUpdateValidator : AbstractValidator<PointUpdateDto>
{
    public PointUpdateValidator()
    {
        RuleFor(x => x.PointId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.CityId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Detail)
            .NotNull()
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.IsActive)
            .NotNull();
    }
}