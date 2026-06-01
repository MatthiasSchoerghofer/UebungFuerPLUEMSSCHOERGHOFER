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
    /// Erstellt eine fertige Entity-Liste für Aufgabe-2-Tests.
    /// Diese Liste ersetzt bewusst eine Testdatenbank.
    /// </summary>
    public static List<Song> SongList()
    {
        Artist daftPunk = Artist("Daft Punk", "France");
        daftPunk.Id = 1;

        Artist queen = Artist("Queen", "United Kingdom");
        queen.Id = 2;

        Song oneMoreTime = Song("One More Time", daftPunk, 2_100_000, MusicGenre.Electronic, false, 320);
        oneMoreTime.Id = 1;
        oneMoreTime.ArtistId = daftPunk.Id;

        Song harderBetter = Song("Harder Better Faster Stronger", daftPunk, 950_000, MusicGenre.Electronic, false, 224);
        harderBetter.Id = 2;
        harderBetter.ArtistId = daftPunk.Id;

        Song bohemian = Song("Bohemian Rhapsody", queen, 3_500_000, MusicGenre.Rock, false, 354);
        bohemian.Id = 3;
        bohemian.ArtistId = queen.Id;

        Song explicitSong = Song("Explicit Test Song", queen, 1_200_000, MusicGenre.Rock, true, 180);
        explicitSong.Id = 4;
        explicitSong.ArtistId = queen.Id;

        daftPunk.Songs.AddRange([oneMoreTime, harderBetter]);
        queen.Songs.AddRange([bohemian, explicitSong]);

        oneMoreTime.Requests.Add(new SongRequest
        {
            Id = 1,
            Song = oneMoreTime,
            SongId = oneMoreTime.Id,
            RequestedBy = "Mahi",
            Message = "Bitte spielen.",
            RequestedAt = new DateTime(2026, 5, 17, 11, 0, 0, DateTimeKind.Utc),
            Status = SongRequestStatus.Pending
        });

        bohemian.Requests.Add(new SongRequest
        {
            Id = 2,
            Song = bohemian,
            SongId = bohemian.Id,
            RequestedBy = "Anna",
            RequestedAt = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc),
            Status = SongRequestStatus.Pending
        });

        harderBetter.Requests.Add(new SongRequest
        {
            Id = 3,
            Song = harderBetter,
            SongId = harderBetter.Id,
            RequestedBy = "Tom",
            RequestedAt = new DateTime(2026, 5, 17, 13, 0, 0, DateTimeKind.Utc),
            Status = SongRequestStatus.Played
        });

        return [oneMoreTime, harderBetter, bohemian, explicitSong];
    }

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
