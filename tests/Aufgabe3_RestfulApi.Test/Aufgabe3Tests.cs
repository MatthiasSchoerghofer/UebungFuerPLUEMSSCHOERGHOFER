using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;
using Aufgabe2_BusinessServices.Exceptions;
using Aufgabe2_BusinessServices.Services;
using Aufgabe2_BusinessServices.TestFixtures;
using NSubstitute;
using SPG_Helper;
using System.Net;
using System.Net.Http.Json;

namespace Aufgabe3_RestfulApi.Test;

/// <summary>
/// Diese Collection verhindert parallele Tests mit derselben Test-App.
/// </summary>
[CollectionDefinition("Sequential")]
public class SequentialCollection { }

/// <summary>
/// Aufgabe3Tests prüfen die REST-Endpunkte mit Controller, Routing, JSON, Validation und HTTP-Statuscodes.
/// Die Service-Schicht wird gemockt, damit Aufgabe 3 unabhängig von Aufgabe 2 läuft.
/// </summary>
[Collection("Sequential")]
public class Aufgabe3Tests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    /// <summary>
    /// Pro Testklasse wird eine Factory erstellt und der Service-Mock mit Rückgabewerten vorbereitet.
    /// </summary>
    public Aufgabe3Tests()
    {
        _factory = new TestWebApplicationFactory();
        ConfigureServiceMock(_factory.SongServiceMock);
    }

    /// <summary>
    /// Dispose beendet die Test-App nach dem Test.
    /// </summary>
    public void Dispose()
    {
        _factory.Dispose();
    }

    /// <summary>
    /// Endpunkt-Test: GET /api/songs liefert 200 OK und eine Liste.
    /// </summary>
    [Fact]
    public async Task GetSongs_ReturnsAllSongs()
    {
        using HttpClient client = _factory.Client;

        var (status, result) = await client.GetHttpContent<SongResponseDto[]>("/api/songs");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Length >= 2);
    }

    /// <summary>
    /// Endpunkt-Test: GET /api/songs/paged liefert nur eine Seite und Pagination-Metadaten.
    /// </summary>
    [Fact]
    public async Task GetSongsPaged_ReturnsRequestedPage()
    {
        using HttpClient client = _factory.Client;

        var (status, result) = await client.GetHttpContent<PagedResultDto<SongResponseDto>>("/api/songs/paged?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.True(result.TotalCount >= 2);
        Assert.True(result.Items.Count <= 2);
    }

    /// <summary>
    /// Endpunkt-Test: Ungültige Pagination wird im Controller abgelehnt, bevor der Service aufgerufen wird.
    /// </summary>
    [Fact]
    public async Task GetSongsPaged_InvalidPage_ReturnsBadRequest()
    {
        using HttpClient client = _factory.Client;

        HttpResponseMessage response = await client.GetAsync("/api/songs/paged?page=0&pageSize=2");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _ = _factory.SongServiceMock.DidNotReceive().GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Endpunkt-Test: POST /api/songs legt einen Song an und liefert 201 Created.
    /// </summary>
    [Fact]
    public async Task PostSong_ValidPayload_ReturnsCreated()
    {
        using HttpClient client = _factory.Client;
        UploadSongCmd cmd = SongTestDataFactory.UploadSongCmd("Digital Love", "Daft Punk", 1_900_000);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/songs", cmd);
        SongResponseDto? result = await response.Content.ReadFromJsonAsync<SongResponseDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("Digital Love", result.Title);
    }

    /// <summary>
    /// Endpunkt-Test: POST /api/songs validiert im Controller und ruft den Service bei Fehlern nicht auf.
    /// </summary>
    [Fact]
    public async Task PostSong_InvalidPayload_ReturnsBadRequestWithoutCallingService()
    {
        using HttpClient client = _factory.Client;
        UploadSongCmd cmd = SongTestDataFactory.UploadSongCmd(title: "", artistName: "Daft Punk", streams: 1_900_000);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/songs", cmd);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _ = _factory.SongServiceMock.DidNotReceive().UploadSongAsync(Arg.Any<UploadSongCmd>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Endpunkt-Test: GET /api/songs/popular ist ein LINQ-Endpunkt aus Controller-Sicht.
    /// Die eigentliche Service-Logik ist gemockt.
    /// </summary>
    [Fact]
    public async Task GetPopularSongs_ReturnsOnlySongsAboveMinimum()
    {
        using HttpClient client = _factory.Client;

        var (status, result) = await client.GetHttpContent<PopularSongDto[]>("/api/songs/popular?minStreams=3000000");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.All(result, song => Assert.True(song.Streams >= 3_000_000));
    }

    /// <summary>
    /// Endpunkt-Test: GET /api/songs/clean/{genre} liefert DTOs aus dem gemockten Service.
    /// </summary>
    [Fact]
    public async Task GetCleanSongsByGenre_ReturnsOnlyCleanGenreSongs()
    {
        using HttpClient client = _factory.Client;

        var (status, result) = await client.GetHttpContent<SongResponseDto[]>($"/api/songs/clean/{MusicGenre.Rock}");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.All(result, song =>
        {
            Assert.Equal(MusicGenre.Rock, song.Genre);
            Assert.False(song.IsExplicit);
        });
    }

    /// <summary>
    /// Endpunkt-Test: Requesten eines Songs erstellt einen Pending-Request.
    /// </summary>
    [Fact]
    public async Task RequestSong_ExistingSong_ReturnsCreatedRequest()
    {
        using HttpClient client = _factory.Client;
        var (songsStatus, songs) = await client.GetHttpContent<SongResponseDto[]>("/api/songs");
        Assert.Equal(HttpStatusCode.OK, songsStatus.StatusCode);
        int songId = songs![0].Id;

        HttpResponseMessage response = await client.PostAsJsonAsync($"/api/songs/{songId}/requests", SongTestDataFactory.RequestSongCmd());
        SongRequestResponseDto? result = await response.Content.ReadFromJsonAsync<SongRequestResponseDto>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(SongRequestStatus.Pending, result.Status);
    }

    /// <summary>
    /// Endpunkt-Test: RequestSong validiert im Controller und ruft den Service bei Fehlern nicht auf.
    /// </summary>
    [Fact]
    public async Task RequestSong_InvalidPayload_ReturnsBadRequestWithoutCallingService()
    {
        using HttpClient client = _factory.Client;

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/songs/1/requests", new RequestSongCmd("", "Bitte spielen."));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        _ = _factory.SongServiceMock.DidNotReceive().RequestSongAsync(Arg.Any<int>(), Arg.Any<RequestSongCmd>(), Arg.Any<CancellationToken>());
    }

    /// <summary>
    /// Endpunkt-Test: Unbekannte Song-Id liefert 404 NotFound.
    /// </summary>
    [Fact]
    public async Task GetSong_UnknownId_ReturnsNotFound()
    {
        using HttpClient client = _factory.Client;

        HttpResponseMessage response = await client.GetAsync("/api/songs/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    /// <summary>
    /// Endpunkt-Test: DELETE entfernt einen Song und liefert 204 NoContent.
    /// </summary>
    [Fact]
    public async Task DeleteSong_ExistingSong_ReturnsNoContent()
    {
        using HttpClient client = _factory.Client;
        var (songsStatus, songs) = await client.GetHttpContent<SongResponseDto[]>("/api/songs");
        Assert.Equal(HttpStatusCode.OK, songsStatus.StatusCode);
        int songId = songs![0].Id;

        HttpResponseMessage response = await client.DeleteAsync($"/api/songs/{songId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    /// <summary>
    /// Konfiguriert die Rückgabewerte des Service-Mocks für die Controller-Tests.
    /// </summary>
    private static void ConfigureServiceMock(ISongService service)
    {
        SongResponseDto[] songs =
        [
            new(1, "One More Time", "Daft Punk", 320, 2_100_000, MusicGenre.Electronic, false, new DateTime(2026, 5, 17, 8, 0, 0, DateTimeKind.Utc)),
            new(2, "Bohemian Rhapsody", "Queen", 354, 3_500_000, MusicGenre.Rock, false, new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Utc)),
            new(3, "Explicit Test Song", "Queen", 180, 1_200_000, MusicGenre.Rock, true, new DateTime(2026, 5, 17, 11, 0, 0, DateTimeKind.Utc))
        ];

        service.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SongResponseDto>>(songs));

        service.GetPagedAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                int page = call.ArgAt<int>(0);
                int pageSize = call.ArgAt<int>(1);
                IReadOnlyList<SongResponseDto> items = songs
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Task.FromResult(new PagedResultDto<SongResponseDto>(items, page, pageSize, songs.Length));
            });

        service.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                int id = call.ArgAt<int>(0);
                SongResponseDto song = songs.FirstOrDefault(s => s.Id == id)
                    ?? throw new NotFoundException($"Song with id {id} was not found.");

                return Task.FromResult(song);
            });

        service.UploadSongAsync(Arg.Any<UploadSongCmd>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                UploadSongCmd cmd = call.ArgAt<UploadSongCmd>(0);
                SongResponseDto created = new(10, cmd.Title, cmd.ArtistName, cmd.DurationSeconds, cmd.Streams, cmd.Genre, cmd.IsExplicit, DateTime.UtcNow);
                return Task.FromResult(created);
            });

        service.DeleteSongAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        service.GetPopularSongsAsync(Arg.Any<long>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                long minStreams = call.ArgAt<long>(0);
                IReadOnlyList<PopularSongDto> result = songs
                    .Where(s => s.Streams >= minStreams)
                    .OrderByDescending(s => s.Streams)
                    .Select(s => new PopularSongDto(s.Id, s.Title, s.ArtistName, s.Streams))
                    .ToList();

                return Task.FromResult(result);
            });

        service.GetCleanSongsByGenreAsync(Arg.Any<MusicGenre>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                MusicGenre genre = call.ArgAt<MusicGenre>(0);
                IReadOnlyList<SongResponseDto> result = songs
                    .Where(s => s.Genre == genre && !s.IsExplicit)
                    .ToList();

                return Task.FromResult(result);
            });

        service.RequestSongAsync(Arg.Any<int>(), Arg.Any<RequestSongCmd>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                int songId = call.ArgAt<int>(0);
                RequestSongCmd cmd = call.ArgAt<RequestSongCmd>(1);
                SongResponseDto song = songs.FirstOrDefault(s => s.Id == songId)
                    ?? throw new NotFoundException($"Song with id {songId} was not found.");

                SongRequestResponseDto created = new(20, cmd.RequestedBy, cmd.Message, DateTime.UtcNow, SongRequestStatus.Pending, song.Id, song.Title, song.ArtistName);
                return Task.FromResult(created);
            });

        service.GetPendingRequestsAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<SongRequestResponseDto>>(
            [
                new(1, "Mahi", "Bitte spielen.", new DateTime(2026, 5, 17, 11, 0, 0, DateTimeKind.Utc), SongRequestStatus.Pending, 1, "One More Time", "Daft Punk")
            ]));
    }
}
