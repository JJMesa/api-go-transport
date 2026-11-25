using FluentValidation;
using GoTransport.Application.Dtos.Department;

namespace GoTransport.Application.Validators.Department;

public class DepartmentCreationValidator : AbstractValidator<DepartmentCreationDto>
{
    public DepartmentCreationValidator()
    {
        // BAS_DEPARTMENT.DESCRIPTION is varchar(32).
        RuleFor(x => x.Description)
            .NotNull()
            .NotEmpty()
            .MaximumLength(32);
    }
}
