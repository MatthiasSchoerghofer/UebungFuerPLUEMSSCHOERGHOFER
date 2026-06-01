namespace Aufgabe3_RestfulApi.Model;

public class TimeEntry
{
    public TimeEntry(DateTime workDate, decimal hours, string description)
    {
        WorkDate = workDate;
        Hours = hours;
        Description = description;
    }

    public int Id { get; private set; }
    public DateTime WorkDate { get; set; }
    public decimal Hours { get; set; }
    public string Description { get; set; }
    public bool Billable { get; set; }

    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = null!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
}