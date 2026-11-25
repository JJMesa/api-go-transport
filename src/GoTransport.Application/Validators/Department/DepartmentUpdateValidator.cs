using FluentValidation;
using GoTransport.Application.Dtos.Department;

namespace GoTransport.Application.Validators.Department;

public class DepartmentUpdateValidator : AbstractValidator<DepartmentUpdateDto>
{
    public DepartmentUpdateValidator()
    {
        RuleFor(x => x.DepartmentId)
            .NotNull()
            .NotEmpty();

        // BAS_DEPARTMENT.DESCRIPTION is varchar(32).
        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .MaximumLength(32);

        RuleFor(x => x.IsActive)
            .NotNull();
    }
}
