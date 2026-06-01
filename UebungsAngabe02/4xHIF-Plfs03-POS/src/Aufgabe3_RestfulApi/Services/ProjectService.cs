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
    
    public async Task<IQueryable<ProjectInfoDto>> GetProjectInfosAsync(string? status, string? country, string? skill, CancellationToken ct)
    {
        IQueryable<Project> projects = _context.Projects
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

        var result = await projects.ToListAsync(ct);

        return result
            .Select(ProjectMapper.ToProjectInfoDto)
            .AsQueryable();    }

    
    public async Task<IQueryable<EmployeeWorkloadDto>> GetEmployeeWorkloadsAsync(string? department, string? country, bool? onlyActive, int? minHours,
        CancellationToken ct)
    {
        IQueryable<Employee> employees = _context.Employees
            .Include(e => e.Office).ThenInclude(o => o.Country)
            .Include(e => e.AssignedTasks).ThenInclude(a => a.State)
            .Include(e => e.ProjectAssignments).ThenInclude(pa => pa.Project).ThenInclude(p => p.Status)
            .Include(e => e.Skills).ThenInclude(s => s.Name);

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
            employees = employees.Where(e => e.TimeEntries.Any(t => t.Hours >= minHours));
        }

        var result = await employees.ToListAsync(ct);
        
        return result.Select(ProjectMapper.ToEmployeeWorkloadDto)
            .AsQueryable();
    }

    public async Task<IQueryable<CriticalTaskDto>> GetCriticalTasksAsync(DateTime? dueBefore, TaskPriority? priority, bool? missingSkillsOnly, int? take,
        CancellationToken ct)
    {
        take = take <= 0 ? 10 : take;

        var tasks = _context.TaskItems
            .Include(t => t.Project)
            .Include(t => t.Assignee)
            .ThenInclude(a => a.Skills)
            .Include(t => t.RequiredSkills)
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
            tasks = tasks.Where( t => t.Assignee != null && t.RequiredSkills
                .Any(s => t.Assignee.Skills
                    .Any(ps => ps.Id == s.Id)));
        }
        
        if (take.HasValue)
        {
            var result = await tasks.Take((int)take).ToListAsync<TaskItem>(cancellationToken: ct);
            return result.Select(ProjectMapper.ToCriticalTaskDto).AsQueryable();
        }
        else
        {
            var result = await tasks.ToListAsync(cancellationToken: ct);
            return result.Select(ProjectMapper.ToCriticalTaskDto).AsQueryable();
        }
    } 
}   