using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;
using Aufgabe2_BusinessServices.TestFixtures;
using SPG_Helper;
using System.Net;
using System.Net.Http.Json;

namespace Aufgabe3_RestfulApi.Test;

/// <summary>
/// Diese Collection verhindert parallele Integrationstests mit derselben Testdatenbank.
/// </summary>
[CollectionDefinition("Sequential")]
public class SequentialCollection { }

/// <summary>
/// Aufgabe3Tests prüfen die REST-Endpunkte mit Controller, Routing, JSON und HTTP-Statuscodes.
/// Das sind Integrationstests, keine reinen Unit Tests.
/// </summary>
[Collection("Sequential")]
public class Aufgabe3Tests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    /// <summary>
    /// Pro Testklasse wird eine Factory erstellt und mit Testdaten befüllt.
    /// </summary>
    public Aufgabe3Tests()
    {
        _factory = new TestWebApplicationFactory();
        _factory.SeedDatabase();
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
    /// Endpunkt-Test: GET /api/songs/popular ist ein LINQ-Endpunkt.
    /// Alle Songs im Ergebnis müssen mindestens minStreams erreichen.
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
    /// Endpunkt-Test: GET /api/songs/clean/{genre} filtert nach Genre und explicit=false.
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
}
