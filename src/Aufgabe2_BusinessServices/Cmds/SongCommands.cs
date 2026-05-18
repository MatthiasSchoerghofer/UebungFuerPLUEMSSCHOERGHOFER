using Aufgabe1_ORMapping.Model;

namespace Aufgabe2_BusinessServices.Cmds;

/// <summary>
/// Command/Request-DTO zum Hochladen eines neuen Songs.
/// Der Client sendet diese Daten per POST an die API.
/// </summary>
public record UploadSongCmd(
    string Title,
    string ArtistName,
    string? ArtistCountry,
    int DurationSeconds,
    long Streams,
    MusicGenre Genre,
    bool IsExplicit);

/// <summary>
/// Command/Request-DTO zum Requesten eines vorhandenen Songs.
/// SongId kommt aus der Route, RequestedBy und Message kommen aus dem Body.
/// </summary>
public record RequestSongCmd(
    string RequestedBy,
    string? Message);

/// <summary>
/// Command/Request-DTO zum Ändern der Stream-Anzahl.
/// Das ist ein kleiner Update-Use-Case, damit PUT/PATCH-artige Logik übbar ist.
/// </summary>
public record UpdateStreamsCmd(long Streams);
