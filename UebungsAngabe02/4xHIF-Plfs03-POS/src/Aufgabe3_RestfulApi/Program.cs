using Aufgabe3_RestfulApi.Model;
using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Infrastructure;
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

        builder.Services.AddDbContext<ProjectDBContext>(options =>
            options.UseSqlite("Data Source=Blogs.db"));

        builder.Services.AddScoped<IProjectService, ProjectService>();
        builder.Services.AddScoped<IDemoService, DemoService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ProjectDBContext>();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            await context.SeedAsync();
        }
        
        app.MapGet("/api/projects/dashboard",
                async (string? status, string? customerCountry, string? skill, IProjectService projectService, CancellationToken ct) =>
                {
                    var result = await projectService.GetProjectInfosAsync(status, customerCountry, skill, ct);

                    return Results.Ok(result);
                })
            .WithName("GetProjectDashboard")
            .WithTags("Projects")
            .Produces<IQueryable<ProjectInfoDto>>(StatusCodes.Status200OK);

        app.MapGet("/api/employees/workload",
            async (string? department, string? country, bool? onlyActive, int? minHours, IProjectService projectService,CancellationToken ct) =>
            {
                var result = await projectService.GetEmployeeWorkloadsAsync(department, country, onlyActive, minHours, ct);
                
                return Results.Ok(result);
            })
            .WithName("GetEmployeeWorkloads")
            .WithTags("Employees")
            .Produces<IQueryable<EmployeeWorkloadDto>>(StatusCodes.Status200OK);

        app.MapGet("/api/tasks/critical", async () =>
        {
            
        });
        
        
        
        await app.RunAsync();
    }
}