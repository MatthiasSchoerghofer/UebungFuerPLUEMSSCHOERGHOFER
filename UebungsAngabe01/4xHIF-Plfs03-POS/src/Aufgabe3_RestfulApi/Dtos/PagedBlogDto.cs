namespace Aufgabe3_RestfulApi.Dtos;

public record PagedBlogDto(
    int Page,
    int PageSize,
    int TotalCount,
    IEnumerable<BlogDto> Blogs
    );