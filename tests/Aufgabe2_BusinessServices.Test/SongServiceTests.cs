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
/// Der Service verwendet eine echte SQLite-In-Memory-Datenbank.
/// </summary>
public class SongServiceTests
{
    /// <summary>
    /// Erstellt DbContext und Service für einen Test.
    /// </summary>
    private static (AppDbContext Db, SongService Service) CreateService()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        return (db, new SongService(db));
    }

    /// <summary>
    /// Prüft, ob UploadSongAsync Song und Artist speichert und ein DTO zurückgibt.
    /// </summary>
    [Fact]
    public async Task UploadSongAsync_CreatesSongAndArtist()
    {
        var (db, service) = CreateService();
        await using (db)
        {
            var result = await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd());

            Assert.True(result.Id > 0);
            Assert.Equal("Around the World", result.Title);
            Assert.Single(db.Artists);
            Assert.Single(db.Songs);
        }
    }

    /// <summary>
    /// Prüft, ob ein Request für einen vorhandenen Song erstellt wird.
    /// </summary>
    [Fact]
    public async Task RequestSongAsync_CreatesPendingRequest()
    {
        var (db, service) = CreateService();
        await using (db)
        {
            var song = await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd());

            var request = await service.RequestSongAsync(song.Id, SongTestDataFactory.RequestSongCmd());

            Assert.Equal(SongRequestStatus.Pending, request.Status);
            Assert.Equal(song.Id, request.SongId);
            Assert.Single(db.SongRequests);
        }
    }

    /// <summary>
    /// Prüft den Service-LINQ-Use-Case für beliebte Songs.
    /// </summary>
    [Fact]
    public async Task GetPopularSongsAsync_ReturnsOnlySongsAboveMinimum()
    {
        var (db, service) = CreateService();
        await using (db)
        {
            await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Hit", "Artist A", 2_500_000));
            await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Small Song", "Artist B", 100));

            var result = await service.GetPopularSongsAsync(1_000_000);

            Assert.Single(result);
            Assert.All(result, song => Assert.True(song.Streams >= 1_000_000));
        }
    }

    /// <summary>
    /// Prüft, dass fehlende Songs eine NotFoundException erzeugen.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_UnknownId_ThrowsNotFoundException()
    {
        var (db, service) = CreateService();
        await using (db)
        {
            await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999));
        }
    }

    /// <summary>
    /// Prüft, ob negative Stream-Zahlen fachlich abgelehnt werden.
    /// </summary>
    [Fact]
    public async Task UpdateStreamsAsync_NegativeStreams_ThrowsArgumentException()
    {
        var (db, service) = CreateService();
        await using (db)
        {
            var song = await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd());

            await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateStreamsAsync(song.Id, new UpdateStreamsCmd(-1)));
        }
    }
}
