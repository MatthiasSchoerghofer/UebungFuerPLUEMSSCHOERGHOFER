using Aufgabe1_ORMapping.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe1_ORMapping.Test;

[CollectionDefinition("Sequential")]
public class SequentialCollection {
    // Intentionally empty. Using a fixture type is optional here.
    // The presence of this definition allows using [Collection("Sequential")] on test classes.
}

// Tests within the same collection never run in parallel!
[Collection("Sequential")]
public class Aufgabe1Test
{
    private readonly ITestOutputHelper output;

    public Aufgabe1Test(ITestOutputHelper output)
    {
        this.output = output;
    }

    private AppDbContext GetEmptyDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(@"Data Source=app.db")
            .LogTo(output.WriteLine)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public void CreateDatabaseTest()
    {
        using var db = GetEmptyDbContext();
    }
}
