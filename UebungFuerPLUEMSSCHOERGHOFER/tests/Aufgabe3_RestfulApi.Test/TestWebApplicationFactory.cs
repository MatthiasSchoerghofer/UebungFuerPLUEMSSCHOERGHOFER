using Aufgabe1_ORMapping.Infrastructure;
using Aufgabe3_RestfulApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;

namespace Aufgabe3_RestfulApi.Test;

/// <summary>
/// TestWebApplicationFactory startet die REST-API im Speicher für Minimal-API-Tests.
/// Die App verwendet dabei einen echten AppDbContext mit SQLite-In-Memory.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    /// <summary>
    /// Client ist der HTTP-Client, mit dem Tests echte Requests an die Minimal API schicken.
    /// </summary>
    public HttpClient Client => CreateClient();

    /// <summary>
    /// ConfigureWebHost überschreibt nur die Datenbankverbindung für Tests.
    /// Der echte SongService bleibt registriert und arbeitet mit dem Test-DbContext.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (_connection.State != ConnectionState.Open)
        {
            _connection.Open();
        }

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });

        builder.UseEnvironment("Testing");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
