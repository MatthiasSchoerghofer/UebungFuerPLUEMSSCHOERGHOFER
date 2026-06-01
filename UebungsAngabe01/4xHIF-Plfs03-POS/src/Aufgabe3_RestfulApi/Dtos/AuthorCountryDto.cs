using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Dtos;

public record AuthorCountryDto (int Id,
    string Name,
    string Country,
    int BlogCount,
    List<String> Blogs
    );