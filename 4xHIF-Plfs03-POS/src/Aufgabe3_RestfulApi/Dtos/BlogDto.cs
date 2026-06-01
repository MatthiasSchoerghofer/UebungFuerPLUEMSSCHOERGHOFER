namespace Aufgabe3_RestfulApi.Dtos;

public record BlogDto(int Id, string Name, int PostCount);

public record BlogCreateCmd(string Name);

public record BlogUpdateCmd(string Name);

public record PostDto(
    int Id,
    string Title,
    DateTime PublishedOn,
    string BlogName,
    string? AuthorName,
    IReadOnlyList<string> Tags);

public record BlogStatsDto(
    int Id,
    string Name,
    int PostCount,
    int AuthorCount,
    DateTime? LastPublishedOn,
    IReadOnlyList<string> Tags);

public record AuthorActivityDto(
    int Id,
    string Name,
    string Country,
    int PostCount,
    IReadOnlyList<string> BlogNames);

public record PagedResultDto<T>(
    int Page,
    int PageSize,
    int TotalCount,
    IReadOnlyList<T> Items);
