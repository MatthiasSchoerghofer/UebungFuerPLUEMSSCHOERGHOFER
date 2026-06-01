using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Infrastructure;
using Aufgabe3_RestfulApi.Interfaces;
using Aufgabe3_RestfulApi.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddValidation();

        builder.Services.AddDbContext<BlogsContext>(options =>
            options.UseSqlite("Data Source=Blogs.db"));

        builder.Services.AddScoped<IBlogService, BlogService>();
        builder.Services.AddScoped<IDemoService, DemoService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<BlogsContext>();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            await context.SeedWithBogusAsync();
        }

        app.MapGet("/api/blogs", async (IBlogService service, string? search, CancellationToken cancellationToken) =>
        {
            var blogs = await service.GetAllAsync(search, cancellationToken);

            return Results.Ok(blogs);
        });

        app.MapGet("/api/blogs/{id:int}", async (IBlogService service, int id, CancellationToken cancellationToken) =>
        {
            var blog = await service.GetByIdAsync(id, cancellationToken);

            return blog == null
                ? Results.NotFound()
                : Results.Ok(blog);
        });

        app.MapPost("/api/blogs", async (IBlogService service, CreateBlogDto dto, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return Results.BadRequest();
            }

            var created = await service.CreateAsync(dto, cancellationToken);

            return Results.Created($"/api/blogs/{created.Id}", created);
        });

        app.MapPut("/api/blogs/{id:int}", async (IBlogService service, int id, UpdateBlogDto dto, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                return Results.BadRequest();
            }

            try
            {
                BlogDto updated = await service.UpdateAsync(id, dto, cancellationToken);

                return Results.Ok(updated);
            }
            catch (Exception ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        app.MapDelete("/api/blogs/{id:int}", async (IBlogService service, int id, CancellationToken cancellationToken) =>
        {
            try
            {
                BlogDto deleted = await service.DeleteAsync(id, cancellationToken);

                return Results.Ok(deleted);
            }
            catch (Exception ex)
            {
                return Results.NotFound(ex.Message);
            }
        });

        app.MapGet("/api/blogs/paged", async (IBlogService service, int page, int pageSize, string? search, CancellationToken cancellationToken) =>
        {
            if (page < 1 || pageSize < 1 || pageSize > 50)
            {
                return Results.BadRequest();
            }

            PagedBlogDto result = await service.GetAllPagedAsync(
                search,
                page,
                pageSize,
                cancellationToken);

            return Results.Ok(result);
        });

        app.MapGet("/api/blogs/stats", async (IBlogService service, int? minPosts, string? tag, CancellationToken cancellationToken) =>
        {
            var stats = await service.GetAllBlogTagStatsAsync(
                minPosts,
                tag,
                cancellationToken);

            return Results.Ok(stats);
        });

        app.MapGet("/api/posts/recent", async (IBlogService service, DateTime? publishedFrom, string? tag, int? take, CancellationToken cancellationToken) =>
        {
            var posts = await service.GetRecentPostsAsync(
                publishedFrom,
                tag,
                take,
                cancellationToken);

            return Results.Ok(posts);
        });

        app.MapGet("/api/authors/by-country", async (IBlogService service, string? country, int? minPosts, CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return Results.BadRequest("Der Parameter country ist verpflichtend.");
            }

            var authors = await service.GetAllAuthorCountriesAsync(
                country,
                minPosts,
                cancellationToken);

            return Results.Ok(authors);
        });

        await app.RunAsync();
    }
}