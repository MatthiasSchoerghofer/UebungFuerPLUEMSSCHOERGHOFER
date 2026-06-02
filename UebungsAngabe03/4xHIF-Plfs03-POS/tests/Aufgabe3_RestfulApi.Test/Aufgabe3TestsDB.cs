using System.Linq;
using Aufgabe3_RestfulApi.Infrastructure;
using Xunit;

namespace Aufgabe3_RestfulApi.Test;

[Collection("Sequential")]
public class Aufgabe3TestsDB : IClassFixture<TestWebApplicationFactoryDB>
{
    private readonly TestWebApplicationFactoryDB _factory;

    public Aufgabe3TestsDB(TestWebApplicationFactoryDB factory)
    {
        _factory = factory;
    }

    [Fact]
    public void TestDatabase_CanBeInitializedWithMakerspaceData()
    {
        _factory.InitializeDatabase(db => db.SeedMakerspace());

        using var db = _factory.CreateDbContext();

        Assert.True(db.Devices.Any());
        Assert.True(db.Reservations.Any());
    }
}
