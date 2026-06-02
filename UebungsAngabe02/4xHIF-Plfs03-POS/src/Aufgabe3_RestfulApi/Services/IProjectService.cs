using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Services;

public interface IProjectService
{
    Task<List<ProjectInfoDto>> GetProjectInfosAsync(
        string? status, string? country, string? skill, CancellationToken ct);
    Task<List<EmployeeWorkloadDto>> GetEmployeeWorkloadsAsync(
        string? department, string? country, bool? onlyActive, int? minHours, CancellationToken ct);
    Task<List<CriticalTaskDto>> GetCriticalTasksAsync(DateTime? dueBefore,TaskPriority? priority, bool? missingSkillsOnly, int? take,CancellationToken ct );
}
