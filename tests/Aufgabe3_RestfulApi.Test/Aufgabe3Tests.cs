using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;
using Aufgabe2_BusinessServices.TestFixtures;
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
/// Aufgabe3Tests prüfen die REST-Endpunkte mit Minimal API, Routing, JSON, Validation,
/// HTTP-Statuscodes und echtem DbContext.
/// </summary>
[Collection("Sequential")]
public class Aufgabe3Tests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    /// <summary>
    /// Pro Test wird eine Factory mit eigener SQLite-In-Memory-Datenbank erstellt.
    /// </summary>
    public Aufgabe3Tests()
    {
        _factory = new TestWebApplicationFactory();
    }

    /// <summary>
    /// Dispose beendet die Test-App nach dem Test.
    /// </summary>
    public void Dispose()
    {
        _factory.Dispose();
    }

    /// <summary>
    /// Endpunkt-Test: GET /api/songs liefert 200 OK und eine Liste aus der Datenbank.
    /// </summary>
    [Fact]
    public async Task GetSongs_ReturnsAllSongsFromDbContext()
    {
        using HttpClient client = _factory.Client;

        var (status, result) = await client.GetHttpContent<SongResponseDto[]>("/api/songs");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.True(result.Length >= 4);
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
        Assert.True(result.TotalCount >= 4);
        Assert.True(result.Items.Count <= 2);
    }

    /// <summary>
    /// Endpunkt-Test: Ungültige Pagination wird von der Minimal API abgelehnt.
    /// </summary>
    [Fact]
    public async Task GetSongsPaged_InvalidPage_ReturnsBadRequest()
    {
        using HttpClient client = _factory.Client;

        HttpResponseMessage response = await client.GetAsync("/api/songs/paged?page=0&pageSize=2");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Endpunkt-Test: POST /api/songs legt einen Song in der Datenbank an und liefert 201 Created.
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

        var (_, songs) = await client.GetHttpContent<SongResponseDto[]>("/api/songs");
        Assert.Contains(songs!, song => song.Title == "Digital Love");
    }

    /// <summary>
    /// Endpunkt-Test: POST /api/songs validiert Commands und liefert bei Fehlern 400 BadRequest.
    /// </summary>
    [Fact]
    public async Task PostSong_InvalidPayload_ReturnsBadRequest()
    {
        using HttpClient client = _factory.Client;
        UploadSongCmd cmd = SongTestDataFactory.UploadSongCmd(title: "", artistName: "Daft Punk", streams: 1_900_000);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/songs", cmd);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Endpunkt-Test: GET /api/songs/popular verwendet LINQ und liefert nur Songs ab dem Minimum.
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
    /// Endpunkt-Test: GET /api/songs/clean/{genre} liefert nur nicht explizite Songs des Genres.
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
    /// Endpunkt-Test: Requesten eines Songs erstellt einen Pending-Request in der Datenbank.
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
        Assert.Equal(songId, result.SongId);
    }

    /// <summary>
    /// Endpunkt-Test: RequestSong validiert Commands und liefert bei Fehlern 400 BadRequest.
    /// </summary>
    [Fact]
    public async Task RequestSong_InvalidPayload_ReturnsBadRequest()
    {
        using HttpClient client = _factory.Client;

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/songs/1/requests", new RequestSongCmd("", "Bitte spielen."));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

        HttpResponseMessage afterDelete = await client.GetAsync($"/api/songs/{songId}");
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }
}
