using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Infrastructure;
using Aufgabe3_RestfulApi.Mapper;
using Aufgabe3_RestfulApi.Model;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi.Services;

public class ProjectService : IProjectService
{
    private readonly ProjectDBContext _context;
    
    public ProjectService(ProjectDBContext context)
    {
        _context = context;
    }
    
    public async Task<List<ProjectInfoDto>> GetProjectInfosAsync(string? status, string? country, string? skill, CancellationToken ct)
    {
        IQueryable<Project> projects = _context.Projects
            .AsNoTracking()
            .Include(p => p.Customer)
            .Include(p => p.Tasks)
            .ThenInclude(t => t.RequiredSkills)
            .Include(p => p.Tasks)
            .ThenInclude(t => t.TimeEntries)
            .ThenInclude(te => te.Employee)
            .Include(p => p.Assignments)
            .ThenInclude(a => a.Employee);

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (Enum.TryParse<ProjectStatus>(status, true, out var parsedStatus))
            {
                projects = projects.Where(p => p.Status == parsedStatus);
            }        
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            projects = projects.Where(p => p.Customer.Country == country);
        }

        if (!string.IsNullOrWhiteSpace(skill))
        {
            projects = projects.Where(p =>
                p.Tasks.Any(t =>
                    t.RequiredSkills.Any(s =>
                        s.Name == skill || s.Code == skill)));
        }

        var result = await projects
            .OrderBy(p => p.Deadline)
            .ToListAsync(ct);

        return result
            .Select(ProjectMapper.ToProjectInfoDto)
            .ToList();
    }

    
    public async Task<List<EmployeeWorkloadDto>> GetEmployeeWorkloadsAsync(string? department, string? country, bool? onlyActive, int? minHours,
        CancellationToken ct)
    {
        IQueryable<Employee> employees = _context.Employees
            .AsNoTracking()
            .Include(e => e.AssignedTasks)
            .Include(e => e.ProjectAssignments)
            .ThenInclude(pa => pa.Project)
            .Include(e => e.Skills)
            .Include(e => e.TimeEntries);

        if (!string.IsNullOrWhiteSpace(department))
        {
            employees = employees.Where(e => e.Department == department);
        }
        
        if (!string.IsNullOrWhiteSpace(country))
        {
            employees = employees.Where(e => e.Office.Country == country);
        }

        if (onlyActive is true)
        {
            employees = employees.Where(e => e.IsActive == true);
        }

        if (minHours > 0)
        {
            employees = employees.Where(e => e.TimeEntries.Sum(t => t.Hours) >= minHours);
        }

        var result = await employees
            .OrderBy(e => e.FullName)
            .ToListAsync(ct);
        
        return result.Select(ProjectMapper.ToEmployeeWorkloadDto)
            .ToList();
    }

    public async Task<List<CriticalTaskDto>> GetCriticalTasksAsync(DateTime? dueBefore, TaskPriority? priority, bool? missingSkillsOnly, int? take,
        CancellationToken ct)
    {
        var takeCount = Math.Min(take ?? 10, 50);
        var today = DateTime.Today;

        var tasks = _context.TaskItems
            .AsNoTracking()
            .Include(t => t.Project)
            .ThenInclude(p => p.Customer)
            .Include(t => t.Assignee)
            .ThenInclude(a => a!.Skills)
            .Include(t => t.RequiredSkills)
            .Where(t => t.State != TaskState.Done || t.Priority == TaskPriority.Critical || t.DueDate < today)
            .AsQueryable();

        if (dueBefore.HasValue)
        {
            tasks = tasks.Where( t => t.DueDate <= dueBefore);
        }
        
        if (priority.HasValue)
        {
            tasks = tasks.Where( t => t.Priority == priority);
        }
        
        if (missingSkillsOnly.HasValue && (bool)missingSkillsOnly)
        {
            tasks = tasks.Where(t =>
                t.RequiredSkills.Any() &&
                (t.Assignee == null ||
                 t.RequiredSkills.Any(s => t.Assignee.Skills.All(ps => ps.Id != s.Id))));
        }
        
        var result = await tasks
            .OrderBy(t => t.DueDate)
            .ThenByDescending(t => t.Priority)
            .Take(takeCount)
            .ToListAsync(cancellationToken: ct);

        return result.Select(ProjectMapper.ToCriticalTaskDto).ToList();
    } 
}   
