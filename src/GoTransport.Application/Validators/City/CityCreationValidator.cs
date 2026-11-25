using FluentValidation;
using GoTransport.Application.Dtos.City;

namespace GoTransport.Application.Validators.City;

public class CityCreationValidator : AbstractValidator<CityCreationDto>
{
    public CityCreationValidator()
    {
        // BAS_CITY.DESCRIPTION is varchar(32).
        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .NotNull();
    }
}