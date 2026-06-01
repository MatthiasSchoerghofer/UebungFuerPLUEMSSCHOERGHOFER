using Aufgabe3_RestfulApi.Dtos;

namespace Aufgabe3_RestfulApi.Interfaces;

public interface IBlogService
{
    Task<IEnumerable<BlogDto>> GetAllAsync(
        string? search,
        CancellationToken cancellationToken = default);

    Task<BlogDto?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<BlogDto> CreateAsync(
        CreateBlogDto dto,
        CancellationToken cancellationToken = default);

    Task<BlogDto> UpdateAsync(
        int id,
        UpdateBlogDto dto,
        CancellationToken cancellationToken = default);

    Task<BlogDto> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default);

    Task<PagedBlogDto> GetAllPagedAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<BlogStatsDto>> GetAllBlogTagStatsAsync(
        int? minPosts,
        string? tag,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<RecentPostsDto>> GetRecentPostsAsync(
        DateTime? publishedFrom,
        string? tag,
        int? take,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<AuthorCountryDto>> GetAllAuthorCountriesAsync(
        string country,
        int? minPosts,
        CancellationToken cancellationToken = default);
}