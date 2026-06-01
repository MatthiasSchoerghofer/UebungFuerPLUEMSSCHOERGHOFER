namespace Aufgabe3_RestfulApi.Model;

public class Employee
{
    public Employee(string fullName, string department, string role, decimal hourlyRate)
    {
        FullName = fullName;
        Department = department;
        Role = role;
        HourlyRate = hourlyRate;
        IsActive = true;
    }

    public int Id { get; private set; }
    public string FullName { get; set; }
    public string Department { get; set; }
    public string Role { get; set; }
    public bool IsActive { get; set; }
    public decimal HourlyRate { get; set; }

    public OfficeAddress Office { get; set; } = null!;

    public List<TaskItem> AssignedTasks { get; } = new();
    public List<ProjectAssignment> ProjectAssignments { get; } = new();
    public List<Skill> Skills { get; } = new();
    public List<TimeEntry> TimeEntries { get; } = new();
}
