using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Infrastructure;
using Aufgabe3_RestfulApi.Interfaces;
using Aufgabe3_RestfulApi.Model;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi.Services;

public class BlogService(BlogsContext db) : IBlogService {
    public async Task<List<BlogDto>> GetAllAsync(string? search = null)
    {
        IQueryable<Blog> query = db.Blogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Name, pattern));
        }

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new BlogDto(x.Id, x.Name, x.Posts.Count))
            .ToListAsync();
    }

    public async Task<BlogDto?> GetByIdAsync(int id)
    {
        return await db.Blogs
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BlogDto(x.Id, x.Name, x.Posts.Count))
            .FirstOrDefaultAsync();
    }

    public async Task<BlogDto> CreateAsync(BlogCreateCmd cmd)
    {
        var blog = new Blog(cmd.Name.Trim());
        db.Blogs.Add(blog);
        await db.SaveChangesAsync();

        return new BlogDto(blog.Id, blog.Name, 0);
    }

    public async Task<BlogDto?> UpdateAsync(int id, BlogUpdateCmd cmd)
    {
        var blog = await db.Blogs.FirstOrDefaultAsync(x => x.Id == id);
        if (blog is null) return null;

        blog.Name = cmd.Name.Trim();
        await db.SaveChangesAsync();

        int postCount = await db.Posts.CountAsync(x => x.Blog.Id == id);
        return new BlogDto(blog.Id, blog.Name, postCount);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var blog = await db.Blogs.FirstOrDefaultAsync(x => x.Id == id);
        if (blog is null) return false;

        db.Blogs.Remove(blog);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResultDto<BlogDto>> GetPagedAsync(int page, int pageSize, string? search = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        IQueryable<Blog> query = db.Blogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.Like(x.Name, pattern));
        }

        int totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(x => x.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new BlogDto(x.Id, x.Name, x.Posts.Count))
            .ToListAsync();

        return new PagedResultDto<BlogDto>(page, pageSize, totalCount, items);
    }

    public async Task<List<BlogStatsDto>> GetStatisticsAsync(int minPosts = 0, string? tag = null)
    {
        minPosts = Math.Max(0, minPosts);
        IQueryable<Blog> query = db.Blogs
            .AsNoTracking()
            .Where(x => x.Posts.Count >= minPosts);

        if (!string.IsNullOrWhiteSpace(tag))
        {
            string tagSearch = tag.Trim();
            query = query.Where(x => x.Posts.Any(p =>
                p.Tags.Any(t => t.Id == tagSearch || t.Text.Contains(tagSearch))));
        }

        var blogs = await query
            .AsSplitQuery()
            .Include(x => x.Posts)
            .ThenInclude(x => x.Author)
            .Include(x => x.Posts)
            .ThenInclude(x => x.Tags)
            .OrderByDescending(x => x.Posts.Count)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return blogs
            .Select(x => new BlogStatsDto(
                x.Id,
                x.Name,
                x.Posts.Count,
                x.Posts.Where(p => p.Author != null).Select(p => p.Author!.Id).Distinct().Count(),
                x.Posts.Count == 0 ? null : x.Posts.Max(p => p.PublishedOn),
                x.Posts
                    .SelectMany(p => p.Tags)
                    .Select(t => t.Text)
                    .Distinct()
                    .OrderBy(t => t)
                    .ToList()))
            .ToList();
    }

    public async Task<List<PostDto>> GetRecentPostsAsync(DateTime? publishedFrom = null, string? tag = null, int take = 10)
    {
        take = Math.Clamp(take, 1, 25);
        IQueryable<Post> query = db.Posts.AsNoTracking();

        if (publishedFrom.HasValue)
        {
            query = query.Where(x => x.PublishedOn >= publishedFrom.Value);
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            string tagSearch = tag.Trim();
            query = query.Where(x => x.Tags.Any(t => t.Id == tagSearch || t.Text.Contains(tagSearch)));
        }

        return await query
            .OrderByDescending(x => x.PublishedOn)
            .ThenBy(x => x.Title)
            .Take(take)
            .Select(x => new PostDto(
                x.Id,
                x.Title,
                x.PublishedOn,
                x.Blog.Name,
                x.Author == null ? null : x.Author.Name,
                x.Tags.Select(t => t.Text).OrderBy(t => t).ToList()))
            .ToListAsync();
    }

    public async Task<List<AuthorActivityDto>> GetAuthorsByCountryAsync(string country, int minPosts = 1)
    {
        minPosts = Math.Max(0, minPosts);
        string countrySearch = country.Trim();

        var authors = await db.Authors
            .AsNoTracking()
            .Include(x => x.Posts)
            .ThenInclude(x => x.Blog)
            .Where(x => x.Contact.Country == countrySearch && x.Posts.Count >= minPosts)
            .OrderByDescending(x => x.Posts.Count)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return authors
            .Select(x => new AuthorActivityDto(
                x.Id,
                x.Name,
                x.Contact.Country,
                x.Posts.Count,
                x.Posts.Select(p => p.Blog.Name).Distinct().OrderBy(n => n).ToList()))
            .ToList();
    }
}
