using Aufgabe3_RestfulApi.Dtos;

namespace Aufgabe3_RestfulApi.Interfaces;

public interface IBlogService {
    Task<List<BlogDto>> GetAllAsync(string? search = null);
    Task<BlogDto?> GetByIdAsync(int id);
    Task<BlogDto> CreateAsync(BlogCreateCmd cmd);
    Task<BlogDto?> UpdateAsync(int id, BlogUpdateCmd cmd);
    Task<bool> DeleteAsync(int id);
    Task<PagedResultDto<BlogDto>> GetPagedAsync(int page, int pageSize, string? search = null);
    Task<List<BlogStatsDto>> GetStatisticsAsync(int minPosts = 0, string? tag = null);
    Task<List<PostDto>> GetRecentPostsAsync(DateTime? publishedFrom = null, string? tag = null, int take = 10);
    Task<List<AuthorActivityDto>> GetAuthorsByCountryAsync(string country, int minPosts = 1);
}
