namespace Aufgabe3_RestfulApi.Model;

public class Project
{
    public Project(string name, DateTime startDate, DateTime deadline, decimal budget)
    {
        Name = name;
        StartDate = startDate;
        Deadline = deadline;
        Budget = budget;
        Status = ProjectStatus.Planned;
    }

    public int Id { get; private set; }
    public string Name { get; set; }
    public ProjectStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime Deadline { get; set; }
    public decimal Budget { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public List<TaskItem> Tasks { get; } = new();
    public List<ProjectAssignment> Assignments { get; } = new();
}