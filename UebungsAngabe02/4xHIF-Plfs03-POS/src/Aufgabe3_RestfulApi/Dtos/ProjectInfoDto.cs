using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Dtos;

public record ProjectInfoDto(
    string ProjectName,
    string CustomerName,
    string CustomerCountry,
    ProjectStatus ProjectStatus,
    DateTime Deadline,
    int AllTasksCount,
    int OpenTasksCount,
    int OverDueTasksCount,
    int EmployeeCount,
    decimal UsedBudget,
    List<string> Skills
);