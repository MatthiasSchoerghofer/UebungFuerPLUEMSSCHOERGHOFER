using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;

namespace Aufgabe2_BusinessServices.Mapper;

/// <summary>
/// SongMapper macht reine Objekt-Umwandlung.
/// Er enthält keine Datenbankzugriffe und keine HTTP-Logik.
/// </summary>
public static class SongMapper
{
    /// <summary>
    /// Wandelt ein Upload-Command plus Artist in eine Song-Entity um.
    /// Diese Entity kann danach vom Service gespeichert werden.
    /// </summary>
    public static Song ToEntity(UploadSongCmd cmd, Artist artist, DateTime uploadedAt) =>
        new()
        {
            Title = cmd.Title.Trim(),
            Artist = artist,
            DurationSeconds = cmd.DurationSeconds,
            Streams = cmd.Streams,
            Genre = cmd.Genre,
            IsExplicit = cmd.IsExplicit,
            UploadedAt = uploadedAt
        };

    /// <summary>
    /// Wandelt eine Song-Entity in ein DTO für API-Antworten um.
    /// Dadurch wird nie die EF-Entity direkt an den Client zurückgegeben.
    /// </summary>
    public static SongResponseDto ToDto(Song song) =>
        new(
            song.Id,
            song.Title,
            song.Artist.Name,
            song.DurationSeconds,
            song.Streams,
            song.Genre,
            song.IsExplicit,
            song.UploadedAt);

    /// <summary>
    /// Wandelt eine Song-Entity in ein kleines PopularSongDto für LINQ-Filter-Ergebnisse um.
    /// </summary>
    public static PopularSongDto ToPopularDto(Song song) =>
        new(song.Id, song.Title, song.Artist.Name, song.Streams);

    /// <summary>
    /// Wandelt eine SongRequest-Entity in ein Antwort-DTO um.
    /// Song und Artist müssen vorher im Query per Include geladen sein.
    /// </summary>
    public static SongRequestResponseDto ToDto(SongRequest request) =>
        new(
            request.Id,
            request.RequestedBy,
            request.Message,
            request.RequestedAt,
            request.Status,
            request.SongId,
            request.Song.Title,
            request.Song.Artist.Name);
}
