using Aufgabe1_ORMapping.Infrastructure;
using Aufgabe1_ORMapping.Model;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe1_ORMapping.Test;

/// <summary>
/// Diese Collection sorgt dafür, dass Datenbanktests nacheinander laufen.
/// Dadurch stören sich Tests nicht gegenseitig.
/// </summary>
[CollectionDefinition("Sequential")]
public class SequentialCollection { }

/// <summary>
/// Aufgabe1Test prüft OR-Mapping und LINQ-Abfragen direkt am DbContext.
/// Hier wird bewusst noch kein Service und kein Controller verwendet.
/// </summary>
[Collection("Sequential")]
public class Aufgabe1Test
{
    /// <summary>
    /// Erstellt eine frische SQLite-In-Memory-Datenbank für jeden Test.
    /// SQLite ist näher an einer echten Datenbank als der EF-InMemoryProvider.
    /// </summary>
    private static AppDbContext GetDbContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        Seed(db);
        return db;
    }

    /// <summary>
    /// Legt fixe Testdaten an, damit LINQ-Ergebnisse vorhersagbar sind.
    /// </summary>
    private static void Seed(AppDbContext db)
    {
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

        Song harderBetter = new()
        {
            Title = "Harder Better Faster Stronger",
            Artist = daftPunk,
            DurationSeconds = 224,
            Streams = 950_000,
            Genre = MusicGenre.Electronic,
            IsExplicit = false,
            UploadedAt = new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc)
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

        Song explicitSong = new()
        {
            Title = "Explicit Test Song",
            Artist = queen,
            DurationSeconds = 180,
            Streams = 1_200_000,
            Genre = MusicGenre.Rock,
            IsExplicit = true,
            UploadedAt = new DateTime(2026, 5, 17, 11, 0, 0, DateTimeKind.Utc)
        };

        db.Songs.AddRange(oneMoreTime, harderBetter, bohemian, explicitSong);
        db.SongRequests.AddRange(
            new SongRequest { Song = bohemian, RequestedBy = "Anna", RequestedAt = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc), Status = SongRequestStatus.Pending },
            new SongRequest { Song = oneMoreTime, RequestedBy = "Mahi", RequestedAt = new DateTime(2026, 5, 17, 11, 0, 0, DateTimeKind.Utc), Status = SongRequestStatus.Pending },
            new SongRequest { Song = harderBetter, RequestedBy = "Tom", RequestedAt = new DateTime(2026, 5, 17, 13, 0, 0, DateTimeKind.Utc), Status = SongRequestStatus.Played });
        db.SaveChanges();
    }

    /// <summary>
    /// Prüft, ob die Datenbank mit allen Tabellen erstellt werden kann.
    /// </summary>
    [Fact]
    public void CreateDatabaseTest()
    {
        using AppDbContext db = GetDbContext();
        Assert.True(db.Database.CanConnect());
    }

    /// <summary>
    /// LINQ-Test: Alle beliebten Songs müssen mindestens 1 Mio Streams haben und absteigend sortiert sein.
    /// </summary>
    [Fact]
    public void QueryPopularSongs_ReturnsOnlySongsAboveMinimum_OrderedByStreamsDescending()
    {
        using AppDbContext db = GetDbContext();

        List<Song> result = db.QueryPopularSongs(1_000_000).ToList();

        Assert.All(result, song => Assert.True(song.Streams >= 1_000_000));
        Assert.Equal(result.OrderByDescending(song => song.Streams).Select(song => song.Id), result.Select(song => song.Id));
    }

    /// <summary>
    /// LINQ-Test: Clean-Songs dürfen nicht explicit sein und müssen zum Genre passen.
    /// </summary>
    [Fact]
    public void QueryCleanSongsByGenre_ReturnsOnlyCleanSongsForGenre()
    {
        using AppDbContext db = GetDbContext();

        List<Song> result = db.QueryCleanSongsByGenre(MusicGenre.Rock).ToList();

        Assert.Single(result);
        Assert.All(result, song =>
        {
            Assert.Equal(MusicGenre.Rock, song.Genre);
            Assert.False(song.IsExplicit);
        });
    }

    /// <summary>
    /// LINQ-Test: Pending Requests müssen offen sein und nach RequestedAt sortiert werden.
    /// </summary>
    [Fact]
    public void QueryPendingRequests_ReturnsOnlyPendingRequests_OrderedByTime()
    {
        using AppDbContext db = GetDbContext();

        List<SongRequest> result = db.QueryPendingRequests().ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, request => Assert.Equal(SongRequestStatus.Pending, request.Status));
        Assert.Equal(result.OrderBy(request => request.RequestedAt).Select(request => request.Id), result.Select(request => request.Id));
    }
}
