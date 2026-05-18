using Aufgabe1_ORMapping.Infrastructure;
using Aufgabe2_BusinessServices.Services;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi;

/// <summary>
/// Program ist der Startpunkt der ASP.NET-Core-App.
/// Hier werden Dependency Injection, Controller, Swagger und die Datenbank konfiguriert.
/// </summary>
public class Program {
    /// <summary>
    /// Main baut die WebApplication auf und startet den HTTP-Server.
    /// In dieser Vorlage verwenden wir Controller und keine Minimal APIs.
    /// </summary>
    public static void Main(string[] args)
    {
        // STEP 1: Configuring ASP.NET Core Services
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Controller werden verwendet, damit HTTP-Logik klar von Service-Logik getrennt ist.
        builder.Services.AddControllers();

        builder.Services.AddValidation();
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=songapp.db"));
        builder.Services.AddScoped<ISongService, SongService>();
    
        // STEP 2: Configuring ASP.NET Core request pipeline
        var app = builder.Build();

        // Für die Übung wird die SQLite-Datenbank automatisch erstellt.
        // In größeren Projekten würde man dafür Migrationen verwenden.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Assign controllers to routes.
        app.MapControllers();

        app.Run();
    }
}
