using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Infrastructure;
using Aufgabe3_RestfulApi.Interfaces;
using Aufgabe3_RestfulApi.Mapper;
using Aufgabe3_RestfulApi.Model;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi.Services;

public class BlogService(BlogsContext db) : IBlogService
{
    public async Task<IEnumerable<BlogDto>> GetAllAsync(string? search, CancellationToken cancellationToken = default)
    {
        IQueryable<Blog> blogs = db.Blogs;

        if (!string.IsNullOrWhiteSpace(search))
        {
            blogs = blogs.Where(b => b.Name.Contains(search));
        }

        List<Blog> blogList = await blogs.ToListAsync(cancellationToken);

        return blogList.Select(BlogMapper.ToDto).ToList();
    }
        
    public async Task<BlogDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Blog? blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        return blog == null ? null : BlogMapper.ToDto(blog);
    }

    public async Task<BlogDto> CreateAsync(CreateBlogDto dto, CancellationToken cancellationToken = default)
    {   
        Blog blog = BlogMapper.ToEntity(dto);

        db.Blogs.Add(blog);

        await db.SaveChangesAsync(cancellationToken);

        return BlogMapper.ToDto(blog);
    }

    public async Task<BlogDto> UpdateAsync(int id, UpdateBlogDto dto, CancellationToken cancellationToken = default)
    {
        Blog? blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (blog == null)
        {
            throw new Exception("Blog to update not found");
        }

        blog.Name = dto.Title;

        await db.SaveChangesAsync(cancellationToken);

        return BlogMapper.ToDto(blog);
    }

    public async Task<BlogDto> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        
        Blog? blog = await db.Blogs.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (blog == null)
        {
            throw new Exception("Blog to delete not found");
        }

        db.Blogs.Remove(blog);

        await db.SaveChangesAsync(cancellationToken);

        return BlogMapper.ToDto(blog);
    }

    public async Task<PagedBlogDto> GetAllPagedAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Blog> blogs = db.Blogs;

        if (!string.IsNullOrWhiteSpace(search))
        {
            blogs = blogs.Where(b => b.Name.Contains(search));
        }

        int totalCount = await blogs.CountAsync(cancellationToken);

        List<Blog> blogsForDto = await blogs
            .OrderBy(b => b.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        List<BlogDto> blogDtos = blogsForDto
            .Select(BlogMapper.ToDto)
            .ToList();

        return new PagedBlogDto(
            page,
            pageSize,
            totalCount,
            blogDtos
        );
    }

    public async Task<IEnumerable<BlogStatsDto>> GetAllBlogTagStatsAsync(
        int? minPosts,
        string? tag,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Blog> blogs = db.Blogs
            .Include(b => b.Posts)
            .ThenInclude(p => p.Tags);

        if (minPosts.HasValue)
        {
            blogs = blogs.Where(b => b.Posts.Count >= minPosts.Value);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            blogs = blogs.Where(b => b.Posts.Any(p =>
                p.Tags.Any(t => t.Text.ToLower() == tag.ToLower())));
        }

        List<Blog> result = await blogs.ToListAsync(cancellationToken);

        return result
            .Select(BlogMapper.ToStatsDto)
            .ToList();
    }

    public async Task<IEnumerable<RecentPostsDto>> GetRecentPostsAsync(
        DateTime? publishedFrom,
        string? tag,
        int? take,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Post> posts = db.Posts
            .Include(p => p.Blog)
            .Include(p => p.Tags);

        if (publishedFrom.HasValue)
        {
            posts = posts.Where(p => p.PublishedOn >= publishedFrom.Value);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            posts = posts.Where(p =>
                p.Tags.Any(t => t.Text.ToLower() == tag.ToLower()));
        }

        posts = posts.OrderByDescending(p => p.PublishedOn);

        if (take.HasValue && take.Value > 0)
        {
            posts = posts.Take(take.Value);
        }

        return await posts
            .Select(p => p.ToRecentPostsDto())
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AuthorCountryDto>> GetAllAuthorCountriesAsync(
        string country,
        int? minPosts,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Author> authors = db.Authors
            .Include(a => a.Posts)
            .ThenInclude(p => p.Blog);

        authors = authors.Where(a =>
            a.Contact.Country.ToLower() == country.ToLower());

        if (minPosts.HasValue && minPosts.Value > 0)
        {
            authors = authors.Where(a =>
                a.Posts.Count >= minPosts.Value);
        }

        return await authors
            .Select(a => a.ToAuthorCountryDto())
            .ToListAsync(cancellationToken);
    }
}