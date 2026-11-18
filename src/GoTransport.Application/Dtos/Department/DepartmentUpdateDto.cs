namespace GoTransport.Application.Dtos.Department;

public class DepartmentUpdateDto
{
    public int DepartmentId { get; set; }

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }
}