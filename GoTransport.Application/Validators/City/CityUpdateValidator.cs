using FluentValidation;
using GoTransport.Application.Dtos.City;

namespace GoTransport.Application.Validators.City;

public class CityUpdateValidator : AbstractValidator<CityUpdateDto>
{
    public CityUpdateValidator()
    {
        RuleFor(x => x.CityId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .NotNull();

        RuleFor(x => x.IsActive)
            .NotNull();
    }
}