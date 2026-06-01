namespace Aufgabe3_RestfulApi.Dtos;

public record RecentPostsDto(
    int Id,
    string Title,
    DateTime PublishedOn,
    string BlogName,
    List<string> Tags
    );