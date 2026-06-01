namespace Aufgabe3_RestfulApi.Model;

public class TaskItem
{
    public TaskItem(string title, string description, DateTime dueDate, TaskPriority priority)
    {
        Title = title;
        Description = description;
        DueDate = dueDate;
        Priority = priority;
        State = TaskState.Open;
    }

    public int Id { get; private set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime DueDate { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskState State { get; set; }
    public int EstimatedHours { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int? AssigneeId { get; set; }
    public Employee? Assignee { get; set; }

    public List<Skill> RequiredSkills { get; } = new();
    public List<TimeEntry> TimeEntries { get; } = new();
}
