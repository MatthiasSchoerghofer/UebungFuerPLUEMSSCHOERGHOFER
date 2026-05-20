using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.Exceptions;
using Aufgabe2_BusinessServices.Services;
using Aufgabe2_BusinessServices.TestFixtures;

namespace Aufgabe2_BusinessServices.Test;

/// <summary>
/// SongServiceTests prüfen die Businesslogik ohne HTTP.
/// Der Service verwendet eine fertige Entity-Liste statt einer Datenbank.
/// </summary>
public class SongServiceTests
{
    /// <summary>
    /// Erstellt Entity-Liste und Service für einen Test.
    /// </summary>
    private static (List<Song> Songs, SongService Service) CreateService(bool seeded = false)
    {
        List<Song> songs = seeded ? SongTestDataFactory.SongList() : [];
        return (songs, new SongService(songs));
    }

    /// <summary>
    /// Prüft, ob UploadSongAsync einen Song in der Entity-Liste anlegt und ein DTO zurückgibt.
    /// </summary>
    [Fact]
    public async Task UploadSongAsync_CreatesSongAndArtist()
    {
        var (songs, service) = CreateService();

        var result = await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd());

        Assert.True(result.Id > 0);
        Assert.Equal("Around the World", result.Title);
        Assert.Single(songs);
        Assert.Equal("Daft Punk", songs[0].Artist.Name);
    }

    /// <summary>
    /// Prüft, ob ein Request für einen vorhandenen Song erstellt wird.
    /// </summary>
    [Fact]
    public async Task RequestSongAsync_CreatesPendingRequest()
    {
        var (songs, service) = CreateService();

        var song = await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd());
        var request = await service.RequestSongAsync(song.Id, SongTestDataFactory.RequestSongCmd());

        Assert.Equal(SongRequestStatus.Pending, request.Status);
        Assert.Equal(song.Id, request.SongId);
        Assert.Single(songs[0].Requests);
    }

    /// <summary>
    /// Prüft den Service-LINQ-Use-Case für beliebte Songs.
    /// </summary>
    [Fact]
    public async Task GetPopularSongsAsync_ReturnsOnlySongsAboveMinimum()
    {
        var (_, service) = CreateService();

        await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Hit", "Artist A", 2_500_000));
        await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Small Song", "Artist B", 100));

        var result = await service.GetPopularSongsAsync(1_000_000);

        Assert.Single(result);
        Assert.All(result, song => Assert.True(song.Streams >= 1_000_000));
    }

    /// <summary>
    /// Prüft Pagination: Die zweite Seite mit pageSize 2 überspringt genau die ersten zwei Songs.
    /// </summary>
    [Fact]
    public async Task GetPagedAsync_ReturnsRequestedPage()
    {
        var (_, service) = CreateService();

        await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Alpha", "Artist A", 1));
        await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Beta", "Artist B", 2));
        await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd("Gamma", "Artist C", 3));

        var result = await service.GetPagedAsync(page: 2, pageSize: 2);

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
        var (_, service) = CreateService();

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(999));
    }

    /// <summary>
    /// Prüft, ob negative Stream-Zahlen fachlich abgelehnt werden.
    /// </summary>
    [Fact]
    public async Task UpdateStreamsAsync_NegativeStreams_ThrowsArgumentException()
    {
        var (_, service) = CreateService();

        var song = await service.UploadSongAsync(SongTestDataFactory.UploadSongCmd());

        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateStreamsAsync(song.Id, new UpdateStreamsCmd(-1)));
    }
}
