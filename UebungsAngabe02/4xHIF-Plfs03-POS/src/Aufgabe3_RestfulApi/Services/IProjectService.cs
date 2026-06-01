using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Services;

public interface IProjectService
{
    Task<IQueryable<ProjectInfoDto>> GetProjectInfosAsync(
        string? status, string? country, string? skill, CancellationToken ct);
    Task<IQueryable<EmployeeWorkloadDto>> GetEmployeeWorkloadsAsync(
        string? department, string? country, bool? onlyActive, int? minHours, CancellationToken ct);
    Task<IQueryable<CriticalTaskDto>> GetCriticalTasksAsync(DateTime? dueBefore,TaskPriority? priority, bool? missingSkillsOnly, int? take,CancellationToken ct );
}