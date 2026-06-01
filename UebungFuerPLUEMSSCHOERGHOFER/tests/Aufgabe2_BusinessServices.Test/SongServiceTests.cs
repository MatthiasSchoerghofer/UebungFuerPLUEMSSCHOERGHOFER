using Aufgabe1_ORMapping.Infrastructure;
using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.Exceptions;
using Aufgabe2_BusinessServices.Services;
using Aufgabe2_BusinessServices.TestFixtures;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe2_BusinessServices.Test;

/// <summary>
/// SongServiceTests prüfen die Businesslogik ohne HTTP.
/// Der Service verwendet dafür einen echten EF-Core-DbContext mit SQLite-In-Memory.
/// </summary>
public class SongServiceTests
{
    /// <summary>
    /// Erstellt DbContext und Service für einen Test.
    /// </summary>
    private static ServiceTestFixture CreateService(bool seeded = false)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        if (seeded)
        {
            db.Songs.AddRange(SongTestDataFactory.SongList());
            db.SaveChanges();
        }

        return new ServiceTestFixture(connection, db, new SongService(db));
    }

    /// <summary>
    /// Prüft, ob UploadSongAsync einen Song in der Datenbank anlegt und ein DTO zurückgibt.
    /// </summary>
    [Fact]
    public async Task UploadSongAsync_CreatesSongAndArtist()
    {
        using ServiceTestFixture fixture = CreateService();

        var result = await fixture.Service.UploadSongAsync(SongTestDataFactory.UploadSongCmd());

        Assert.True(result.Id > 0);
        Assert.Equal("Around the World", result.Title);
        Assert.Equal(1, await fixture.Db.Songs.CountAsync());
        Assert.Equal("Daft Punk", (await fixture.Db.Artists.SingleAsync()).Name);
    }

    /// <summary>
    /// Prüft, ob ein Request für einen vorhandenen Song erstellt wird.
    /// </summary>
    [Fact]
    public async Task RequestSongAsync_CreatesPendingRequest()
    {
        using ServiceTestFixture fixture = CreateService();

        var song = await fixture.Service.UploadSongAsync(SongTestDataFactory.UploadSongCmd());
        var request = await fixture.Service.RequestSongAsync(song.Id, SongTestDataFactory.RequestSongCmd());

        Assert.Equal(SongRequestStatus.Pending, request.Status);
        Assert.Equal(song.Id, request.SongId);
        Assert.Equal(1, await fixture.Db.SongRequests.CountAsync());
    }

    /// <summary>
    /// Prüft den Service-LINQ-Use-Case für beliebte Songs.
    /// </summary>
    [Fact]
    public async Task GetPopularSongsAsync_ReturnsOnlySongsAboveMinimum()
    {
        using ServiceTestFixture fixture = CreateService();

        await fixture.Service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Hit", "Artist A", 2_500_000));
        await fixture.Service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Small Song", "Artist B", 100));

        var result = await fixture.Service.GetPopularSongsAsync(1_000_000);

        Assert.Single(result);
        Assert.All(result, song => Assert.True(song.Streams >= 1_000_000));
    }

    /// <summary>
    /// Prüft Pagination: Die zweite Seite mit pageSize 2 überspringt genau die ersten zwei Songs.
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_ReturnsRequestedPage()
    {
        using ServiceTestFixture fixture = CreateService();

        await fixture.Service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Alpha", "Artist A", 1));
        await fixture.Service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Beta", "Artist B", 2));
        await fixture.Service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Gamma", "Artist C", 3));

        var result = await fixture.Service.GetPagedAsync(page: 2, pageSize: 2);

        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Single(result.Items);
        Assert.Equal("Gamma", result.Items[0].Title);
        Assert.True(result.HasPreviousPage);
        Assert.False(result.HasNextPage);
    }

    /// <summary>
    /// Prüft, dass fehlende Songs eine NotFoundException erzeugen.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_UnknownId_ThrowsNotFoundException()
    {
        using ServiceTestFixture fixture = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => fixture.Service.GetByIdAsync(999));
    }

    /// <summary>
    /// Prüft, ob negative Stream-Zahlen fachlich abgelehnt werden.
    /// </summary>
    [Fact]
    public async Task UpdateStreamsAsync_NegativeStreams_ThrowsArgumentException()
    {
        using ServiceTestFixture fixture = CreateService();

        var song = await fixture.Service.UploadSongAsync(SongTestDataFactory.UploadSongCmd());

        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.UpdateStreamsAsync(song.Id, new UpdateStreamsCmd(-1)));
    }

    private sealed class ServiceTestFixture : IDisposable
    {
        private readonly SqliteConnection _connection;

        public ServiceTestFixture(SqliteConnection connection, AppDbContext db, SongService service)
        {
            _connection = connection;
            Db = db;
            Service = service;
        }

        public AppDbContext Db { get; }
        public SongService Service { get; }

        public void Dispose()
        {
            Db.Dispose();
            _connection.Dispose();
        }
    }
}
