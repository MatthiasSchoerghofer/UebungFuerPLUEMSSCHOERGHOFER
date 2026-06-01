namespace Aufgabe3_RestfulApi.Model;

public class ProjectAssignment
{
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public string RoleInProject { get; set; } = string.Empty;
    public DateTime AssignedFrom { get; set; }
    public DateTime? AssignedTo { get; set; }
    public int AllocationPercent { get; set; }
}