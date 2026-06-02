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
                    if (!string.IsNullOrWhiteSpace(status) &&
                        !Enum.TryParse<ProjectStatus>(status, true, out _))
                    {
                        return Results.BadRequest("Invalid project status.");
                    }

                    var result = await projectService.GetProjectInfosAsync(status, customerCountry, skill, ct);

                    return Results.Ok(result);
                })
            .WithName("GetProjectDashboard")
            .WithTags("Projects")
            .Produces<List<ProjectInfoDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/api/employees/workload",
            async (string? department, string? country, bool? onlyActive, int? minHours, IProjectService projectService,CancellationToken ct) =>
            {
                if (minHours < 0)
                {
                    return Results.BadRequest("minHours must not be negative.");
                }

                var result = await projectService.GetEmployeeWorkloadsAsync(department, country, onlyActive, minHours, ct);
                
                return Results.Ok(result);
            })
            .WithName("GetEmployeeWorkloads")
            .WithTags("Employees")
            .Produces<List<EmployeeWorkloadDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        app.MapGet("/api/tasks/critical", async (DateTime? dueBefore,TaskPriority? priority, 
            bool? missingSkillsOnly, int? take,IProjectService service ,CancellationToken ct) =>
        {
            if (take <= 0)
            {
                return Results.BadRequest("take must be greater than zero.");
            }

            var result = await service.GetCriticalTasksAsync(dueBefore, priority, missingSkillsOnly, take, ct);
            
            return Results.Ok(result);
        })
            .WithName("GetCriticalTasks")
            .WithTags("Tasks")
            .Produces<List<CriticalTaskDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
        
        
        
        await app.RunAsync();
    }
}
