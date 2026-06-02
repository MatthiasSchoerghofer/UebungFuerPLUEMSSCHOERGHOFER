namespace Aufgabe3_RestfulApi.Dtos;

public record EmployeeWorkloadDto(
    string Name,
    string Department,
    string Role,
    string OfficeCountry,
    int OpenTaskCount,
    int ActiveProjectCount,
    decimal BookedHours,
    decimal AvgHoursPerTask,
    List<string> Skills
    );
