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

        // The model year: no older than 1950, and at most next year's model.
        RuleFor(x => x.Model)
            .InclusiveBetween(1950, DateTime.Today.Year + 1);

        RuleFor(x => x.Capacity)
            .GreaterThan(0);
    }
}