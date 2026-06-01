
using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Infrastructure;
using Aufgabe3_RestfulApi.Interfaces;
using Aufgabe3_RestfulApi.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi;

public class Program {
    public static async Task Main(string[] args)
    {
        // STEP 1: Configuring ASP.NET Core Services
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddValidation();
        builder.Services.AddDbContext<BlogsContext>(options => options
              .UseSqlite("Data Source=Blogs.db"));
        builder.Services.AddScoped<IBlogService, BlogService>();

        builder.Services.AddScoped<IDemoService, DemoService>();

        // STEP 2: Configuring ASP.NET Core request pipeline
        var app = builder.Build();
        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            using (var scope = app.Services.CreateScope())
            {
                // read the above configured db context
                var context = scope.ServiceProvider.GetRequiredService<BlogsContext>();
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                await context.SeedWithBogusAsync();
            }

        }

        app.MapGet("/api/demos", (IDemoService service) => Results.Ok(service.GetAll()))
            .WithTags("Demos");

        var blogs = app.MapGroup("/api/blogs")
            .WithTags("Blogs");

        blogs.MapGet("", async (IBlogService service, string? search) =>
            Results.Ok(await service.GetAllAsync(search)));

        blogs.MapGet("/paged", async (IBlogService service, int page = 1, int pageSize = 10, string? search = null) =>
            Results.Ok(await service.GetPagedAsync(page, pageSize, search)));

        blogs.MapGet("/stats", async (IBlogService service, int minPosts = 0, string? tag = null) =>
            Results.Ok(await service.GetStatisticsAsync(minPosts, tag)));

        blogs.MapGet("/recent-posts", async (IBlogService service, DateTime? publishedFrom = null, string? tag = null, int take = 10) =>
            Results.Ok(await service.GetRecentPostsAsync(publishedFrom, tag, take)));

        blogs.MapGet("/authors/by-country", async (IBlogService service, string country, int minPosts = 1) =>
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["country"] = ["Country is required."]
                });
            }

            return Results.Ok(await service.GetAuthorsByCountryAsync(country, minPosts));
        });

        blogs.MapGet("/{id:int}", async (IBlogService service, int id) =>
        {
            var blog = await service.GetByIdAsync(id);
            return blog is null ? Results.NotFound() : Results.Ok(blog);
        });

        blogs.MapPost("", async (IBlogService service, BlogCreateCmd cmd) =>
        {
            var validationError = ValidateName(cmd.Name);
            if (validationError is not null) return validationError;

            var created = await service.CreateAsync(cmd);
            return Results.Created($"/api/blogs/{created.Id}", created);
        });

        blogs.MapPut("/{id:int}", async (IBlogService service, int id, BlogUpdateCmd cmd) =>
        {
            var validationError = ValidateName(cmd.Name);
            if (validationError is not null) return validationError;

            var updated = await service.UpdateAsync(id, cmd);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        blogs.MapDelete("/{id:int}", async (IBlogService service, int id) =>
            await service.DeleteAsync(id) ? Results.NoContent() : Results.NotFound());

        app.Run();

        static IResult? ValidateName(string? name)
        {
            if (!string.IsNullOrWhiteSpace(name)) return null;

            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["name"] = ["Name is required."]
            });
        }
    }
}
