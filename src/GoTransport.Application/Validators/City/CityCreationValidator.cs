using FluentValidation;
using GoTransport.Application.Dtos.City;

namespace GoTransport.Application.Validators.City;

public class CityCreationValidator : AbstractValidator<CityCreationDto>
{
    public CityCreationValidator()
    {
        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.DepartmentId)
            .NotEmpty()
            .NotNull();
    }
}