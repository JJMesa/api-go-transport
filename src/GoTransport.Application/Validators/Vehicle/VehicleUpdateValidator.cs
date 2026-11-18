using FluentValidation;
using GoTransport.Application.Dtos.Vehicle;

namespace GoTransport.Application.Validators.Vehicle;

public class VehicleUpdateValidator : AbstractValidator<VehicleUpdateDto>
{
    public VehicleUpdateValidator()
    {
        RuleFor(x => x.VehicleId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Capacity)
            .NotNull()
            .NotEmpty();
    }
}