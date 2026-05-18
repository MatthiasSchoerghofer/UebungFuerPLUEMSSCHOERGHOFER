using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;

namespace Aufgabe2_BusinessServices.TestFixtures;

/// <summary>
/// TestDataFactory erzeugt fertige Objekte für Tests.
/// So musst du in Tests nicht jedes Mal viele Properties händisch zusammensetzen.
/// </summary>
public static class SongTestDataFactory
{
    /// <summary>
    /// Erstellt einen Artist für Tests.
    /// </summary>
    public static Artist Artist(string name = "Daft Punk", string? country = "France") =>
        new()
        {
            Name = name,
            Country = country
        };

    /// <summary>
    /// Erstellt einen Song für Tests.
    /// Der Artist kann übergeben werden, damit Beziehungen kontrolliert testbar sind.
    /// </summary>
    public static Song Song(
        string title = "One More Time",
        Artist? artist = null,
        long streams = 1_500_000,
        MusicGenre genre = MusicGenre.Electronic,
        bool isExplicit = false,
        int durationSeconds = 320)
    {
        Artist songArtist = artist ?? Artist();
        return new Song
        {
            Title = title,
            Artist = songArtist,
            DurationSeconds = durationSeconds,
            Streams = streams,
            Genre = genre,
            IsExplicit = isExplicit,
            UploadedAt = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Erstellt ein Upload-Command für Service- und API-Tests.
    /// </summary>
    public static UploadSongCmd UploadSongCmd(
        string title = "Around the World",
        string artistName = "Daft Punk",
        long streams = 2_000_000) =>
        new(
            title,
            artistName,
            "France",
            429,
            streams,
            MusicGenre.Electronic,
            false);

    /// <summary>
    /// Erstellt ein Request-Command für einen Song-Wunsch.
    /// </summary>
    public static RequestSongCmd RequestSongCmd(string requestedBy = "Mahi", string? message = "Bitte in der Pause spielen.") =>
        new(requestedBy, message);
}
