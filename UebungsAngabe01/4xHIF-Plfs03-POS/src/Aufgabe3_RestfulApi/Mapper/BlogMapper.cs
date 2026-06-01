using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Mapper;

public static class BlogMapper
{
    public static BlogDto ToDto(Blog blog)
    {
        return new  BlogDto(
            blog.Id,
            blog.Name
            );
    }

    public static Blog ToEntity(CreateBlogDto dto)
    {
        return new Blog(
            dto.Title);
    }

    public static BlogStatsDto ToStatsDto(Blog blog)
    {
        return new BlogStatsDto(
            blog.Name,
            blog.Posts.Count,
            blog.Posts
                .Select(p => p.Author)
                .Distinct()
                .Count(),
            blog.Posts
                .OrderByDescending(p => p.PublishedOn)
                .Select(p => (DateTime?)p.PublishedOn)
                .FirstOrDefault(),
            blog.Posts
                .SelectMany(p => p.Tags)
                .Select(t => t)
                .Distinct()
                .ToList()
        );
    }

    public static RecentPostsDto ToRecentPostsDto(this Post post)
    {
        return new RecentPostsDto(
            post.Id,
            post.Title,
            post.PublishedOn,
            post.Blog.Name,
            post.Tags.Select(t => t.Text).ToList()
            );
    }
    
    public static AuthorCountryDto ToAuthorCountryDto(this Author author)
    {
        return new AuthorCountryDto(
            author.Id,
            author.Name,
            author.Contact.Country,
            author.Posts.Count,
            author.Posts
                .Select(p => p.Blog.Name)
                .Distinct()
                .ToList()
        );
    }
}