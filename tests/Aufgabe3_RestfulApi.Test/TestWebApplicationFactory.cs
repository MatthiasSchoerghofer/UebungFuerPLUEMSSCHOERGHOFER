using Aufgabe1_ORMapping.Infrastructure;
using Aufgabe1_ORMapping.Model;
using Aufgabe3_RestfulApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aufgabe3_RestfulApi.Test;

/// <summary>
/// TestWebApplicationFactory startet die REST-API im Speicher für Integrationstests.
/// Die Produktivdatenbank wird durch eine SQLite-In-Memory-Datenbank ersetzt.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    /// <summary>
    /// Client ist der HTTP-Client, mit dem Tests echte Requests an die Controller schicken.
    /// </summary>
    public HttpClient Client => CreateClient();

    /// <summary>
    /// ConfigureWebHost überschreibt Services nur für Tests.
    /// Wichtig: Controller und echter SongService bleiben aktiv, nur die Datenbank wird ersetzt.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            ServiceDescriptor? descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });

        builder.UseEnvironment("Testing");
    }

    /// <summary>
    /// SeedDatabase löscht die Testdatenbank und füllt sie mit vorhersagbaren Testdaten.
    /// Jeder Integrationstest kann damit frisch starten.
    /// </summary>
    public void SeedDatabase()
    {
        using IServiceScope scope = Services.CreateScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        Artist daftPunk = new() { Name = "Daft Punk", Country = "France" };
        Artist queen = new() { Name = "Queen", Country = "United Kingdom" };

        Song oneMoreTime = new()
        {
            Title = "One More Time",
            Artist = daftPunk,
            DurationSeconds = 320,
            Streams = 2_100_000,
            Genre = MusicGenre.Electronic,
            IsExplicit = false,
            UploadedAt = new DateTime(2026, 5, 17, 8, 0, 0, DateTimeKind.Utc)
        };

        Song bohemian = new()
        {
            Title = "Bohemian Rhapsody",
            Artist = queen,
            DurationSeconds = 354,
            Streams = 3_500_000,
            Genre = MusicGenre.Rock,
            IsExplicit = false,
            UploadedAt = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Utc)
        };

        db.Songs.AddRange(oneMoreTime, bohemian);
        db.SongRequests.Add(new SongRequest
        {
            Song = oneMoreTime,
            RequestedBy = "Mahi",
            Message = "Bitte spielen.",
            RequestedAt = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc),
            Status = SongRequestStatus.Pending
        });
        db.SaveChanges();
    }

    /// <summary>
    /// Dispose räumt die offene SQLite-Verbindung nach den Tests auf.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection.Dispose();
    }
}
