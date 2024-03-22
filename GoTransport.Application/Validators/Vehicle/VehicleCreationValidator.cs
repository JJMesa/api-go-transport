using FluentValidation;
using GoTransport.Application.Dtos.Vehicle;

namespace GoTransport.Application.Validators.Vehicle;

public class VehicleCreationValidator : AbstractValidator<VehicleCreationDto>
{
    public VehicleCreationValidator()
    {
        RuleFor(x => x.ManufacturerId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.LicensePlate)
            .NotNull()
            .NotEmpty()
            .MaximumLength(8);

        RuleFor(x => x.Model)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Capacity)
            .NotNull()
            .NotEmpty();
    }
}