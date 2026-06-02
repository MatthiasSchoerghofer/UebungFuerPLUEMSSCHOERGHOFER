using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using Aufgabe3_RestfulApi.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aufgabe3_RestfulApi.Test;

public class TestWebApplicationFactoryDB : WebApplicationFactory<Program>
{
    private static readonly string TestDatabasePath =
        Path.Combine(Path.GetTempPath(), "Aufgabe3_Makerspace_Test.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.First(d => d.ServiceType == typeof(DbContextOptions<MakerspaceContext>));
            services.Remove(descriptor);
            services.AddDbContext<MakerspaceContext>(options =>
            {
                options.UseSqlite($"DataSource={TestDatabasePath}");
            });
        });
        builder.UseEnvironment("Testing");
    }

    public HttpClient Client => CreateClient();

    public void InitializeDatabase(Action<MakerspaceContext> action)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<MakerspaceContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        action(db);
    }

    public MakerspaceContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<MakerspaceContext>()
            .UseSqlite($"DataSource={TestDatabasePath}")
            .Options;

        return new MakerspaceContext(options);
    }
}
