namespace Aufgabe3_RestfulApi.Dtos;

public record EmployeeWorkloadDto(
    string Name,
    string Department,
    string Role,
    string OfficeCountry,
    int OpenTaskCount,
    int ActiveProjectCount,
    int BookedHours,
    int AvgHoursPerTask,
    List<string> Skills
    );