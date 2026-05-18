using Aufgabe1_ORMapping.Model;

namespace Aufgabe2_BusinessServices.DTOs;

/// <summary>
/// DTO für eine Song-Antwort an den Client.
/// Es enthält nur API-relevante Daten und keine EF-Core Navigation Properties.
/// </summary>
public record SongResponseDto(
    int Id,
    string Title,
    string ArtistName,
    int DurationSeconds,
    long Streams,
    MusicGenre Genre,
    bool IsExplicit,
    DateTime UploadedAt);

/// <summary>
/// DTO für die Song-Suche nach Popularität.
/// Es zeigt dieselben Daten wie SongResponseDto, wird aber bewusst getrennt gehalten,
/// damit du siehst: DTOs können pro Use Case unterschiedlich sein.
/// </summary>
public record PopularSongDto(
    int Id,
    string Title,
    string ArtistName,
    long Streams);

/// <summary>
/// DTO für einen Song-Request in API-Antworten.
/// Die Antwort enthält bereits Songtitel und Artistname, damit der Client nicht extra nachladen muss.
/// </summary>
public record SongRequestResponseDto(
    int Id,
    string RequestedBy,
    string? Message,
    DateTime RequestedAt,
    SongRequestStatus Status,
    int SongId,
    string SongTitle,
    string ArtistName);
