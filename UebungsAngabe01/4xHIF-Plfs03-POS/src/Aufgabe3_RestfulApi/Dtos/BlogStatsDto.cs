using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Dtos;

public record BlogStatsDto(
    string BlogName,
    int BlogPostCount,
    int TotalAuthors,
    DateTime? NewestPostDate,
    List<Tag> Tags
    );