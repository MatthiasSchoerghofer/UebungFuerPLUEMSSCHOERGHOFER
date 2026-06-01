using Aufgabe3_RestfulApi.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aufgabe3_RestfulApi.Test;

public class TestWebApplicationFactoryDB : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.First(d =>
                d.ServiceType == typeof(DbContextOptions<ProjectDBContext>));

            services.Remove(descriptor);

            services.AddDbContext<ProjectDBContext>(options =>
            {
                options.UseSqlite("Data Source=Aufgabe3_Test.db");
            });
        });
    }

    public HttpClient Client => CreateClient();

    public void InitializeDatabase(Action<ProjectDBContext> action)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<ProjectDBContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        action(db);

        db.SaveChanges();
    }

    public Tout QueryDatabase<Tout>(Func<ProjectDBContext, Tout> query)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<ProjectDBContext>();

        return query(db);
    }
}