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
                d.ServiceType == typeof(DbContextOptions<BlogsContext>));

            services.Remove(descriptor);

            services.AddDbContext<BlogsContext>(options =>
            {
                options.UseSqlite("Data Source=Aufgabe3_Test.db");
            });
        });
    }

    public HttpClient Client => CreateClient();

    public void InitializeDatabase(Action<BlogsContext> action)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<BlogsContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        action(db);

        db.SaveChanges();
    }

    public Tout QueryDatabase<Tout>(Func<BlogsContext, Tout> query)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<BlogsContext>();

        return query(db);
    }
}