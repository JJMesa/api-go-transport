namespace GoTransport.Application.Dtos.Department;

public class DepartmentDto
{
    public int DepartmentId { get; set; }

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; }
}