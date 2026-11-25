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

        // BAS_CITY.DESCRIPTION is varchar(32).
        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .NotNull();

        RuleFor(x => x.IsActive)
            .NotNull();
    }
}