using Aufgabe3_RestfulApi.Infrastructure;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aufgabe3_RestfulApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();

        builder.Services.AddValidation();
        builder.Services.AddDbContext<MakerspaceContext>(options => options
            .UseSqlite("Data Source=Makerspace.db"));

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MakerspaceContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SeedMakerspace();
        }

        if (app.Environment.IsEnvironment("Testing"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Add your controllers or minimal API endpoints for Aufgabe 3 here.
        app.MapControllers();

        app.Run();
    }
}
