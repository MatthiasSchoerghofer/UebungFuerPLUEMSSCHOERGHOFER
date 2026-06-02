using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Mapper;

public class ProjectMapper
{
    public static ProjectInfoDto ToProjectInfoDto(Project project)
    {
        DateTime today = DateTime.Today;

        return new ProjectInfoDto(
            project.Name,
            project.Customer.Name,
            project.Customer.Country,
            project.Status,
            project.Deadline,
            project.Tasks.Count,
            project.Tasks.Count(t => t.State != TaskState.Done),
            project.Tasks.Count(t => t.State != TaskState.Done && t.DueDate < today),
            project.Assignments.Select(a => a.EmployeeId).Distinct().Count(),
            project.Tasks.SelectMany(t => t.TimeEntries).Where(te => te.Billable).Sum(te => te.Hours * te.Employee.HourlyRate),
            project.Tasks.SelectMany(t => t.RequiredSkills).Select(s => s.Name).Distinct().OrderBy(s => s).ToList()
            );
    }

    public static EmployeeWorkloadDto ToEmployeeWorkloadDto(Employee employee)
    {
        var bookedHours = employee.TimeEntries.Sum(t => t.Hours);
        var assignedTaskCount = employee.AssignedTasks.Count;
        var avgHoursPerTask = assignedTaskCount == 0 ? 0 : bookedHours / assignedTaskCount;

        return new EmployeeWorkloadDto(
            employee.FullName,
            employee.Department,
            employee.Role,
            employee.Office.Country,
            employee.AssignedTasks.Count(t => t.State is TaskState.InProgress or TaskState.Open),
            employee.ProjectAssignments.Count(p => p.Project.Status == ProjectStatus.Active),
            bookedHours,
            avgHoursPerTask,
            employee.Skills.Select(s => s.Name).Distinct().OrderBy(s => s).ToList()
            );
    }

    public static CriticalTaskDto ToCriticalTaskDto(TaskItem taskItem)
    {
        var requiredSkills = taskItem.RequiredSkills
            .Select(s => s.Name)
            .Distinct()
            .OrderBy(s => s)
            .ToList();

        var employeeSkills = taskItem.Assignee == null
            ? new List<string>()
            : taskItem.Assignee.Skills
                .Select(s => s.Name)
                .Distinct()
                .ToList();

        var missingSkills = requiredSkills
            .Except(employeeSkills)
            .OrderBy(s => s)
            .ToList();

        return new CriticalTaskDto(
            taskItem.Title,
            taskItem.Project.Name,
            taskItem.Project.Customer.Name,
            taskItem.Priority,
            taskItem.State,
            taskItem.DueDate,
            taskItem.Assignee?.FullName,
            requiredSkills,
            missingSkills
        );
    }
}
