using Aufgabe3_RestfulApi.Infrastructure;
using Aufgabe3_RestfulApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Aufgabe3_RestfulApi.Test;


public class TestWebApplicationFactoryDB : WebApplicationFactory<Program> {
    

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.First(d => d.ServiceType == typeof(DbContextOptions<BlogsContext>));
            services.Remove(descriptor);
            services.AddDbContext<BlogsContext>(options =>
            {
                options.UseSqlite("DataSource=Aufgabe3_Test.db");
            });
        });
        builder.UseEnvironment("Testing");
    }

    public HttpClient Client => CreateClient();

    public void InitializeDatabase(Action<BlogsContext> action)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<BlogsContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        // 
        action(db);
    }

    public Tout QueryDatabase<Tout>(Func<BlogsContext, Tout> query)
    {
        using var scope = Services.CreateScope();
        using var db = scope.ServiceProvider.GetRequiredService<BlogsContext>();
        return query(db);
    }
}

