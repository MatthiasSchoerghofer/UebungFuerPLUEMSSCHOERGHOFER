using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Dtos;

public record CriticalTaskDto(
    string Title,
    string ProjectName,
    string CustomerName,
    TaskPriority Priority,
    TaskState State,
    DateTime DueDate,
    string? AssigneeName,
    List<string> RequiredSkills,
    List<string> MissingSkills
);