using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;

namespace Aufgabe2_BusinessServices.Services;

/// <summary>
/// Interface für den SongService.
/// Die Minimal API hängt vom Interface ab, damit HTTP-Schicht und Businesslogik getrennt bleiben.
/// </summary>
public interface ISongService
{
    /// <summary>
    /// Liefert alle Songs.
    /// </summary>
    Task<IReadOnlyList<SongResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert Songs seitenweise mit Pagination-Metadaten.
    /// </summary>
    Task<PagedResultDto<SongResponseDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert einen Song über seine Id oder wirft NotFoundException.
    /// </summary>
    Task<SongResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Erstellt einen neuen Song und legt bei Bedarf auch den Artist an.
    /// </summary>
    Task<SongResponseDto> UploadSongAsync(UploadSongCmd cmd, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aktualisiert die Stream-Anzahl eines Songs.
    /// </summary>
    Task<SongResponseDto> UpdateStreamsAsync(int id, UpdateStreamsCmd cmd, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ersetzt die wichtigsten Song-Daten eines vorhandenen Songs.
    /// </summary>
    Task<SongResponseDto> UpdateSongAsync(int id, UploadSongCmd cmd, CancellationToken cancellationToken = default);

    /// <summary>
    /// Löscht einen Song.
    /// </summary>
    Task DeleteSongAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// LINQ-Use-Case: Liefert Songs ab einer bestimmten Stream-Anzahl.
    /// </summary>
    Task<IReadOnlyList<PopularSongDto>> GetPopularSongsAsync(long minStreams, CancellationToken cancellationToken = default);

    /// <summary>
    /// LINQ-Use-Case: Liefert nicht explizite Songs eines Genres.
    /// </summary>
    Task<IReadOnlyList<SongResponseDto>> GetCleanSongsByGenreAsync(MusicGenre genre, CancellationToken cancellationToken = default);

    /// <summary>
    /// LINQ-Use-Case: Kombinierte Suche mit optionalen Filtern und Pagination.
    /// </summary>
    Task<PagedResultDto<SongResponseDto>> SearchSongsAsync(
        string? term,
        MusicGenre? genre,
        long? minStreams,
        int? maxDurationSeconds,
        bool includeExplicit,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// LINQ-Use-Case: Gruppiert Songs nach Artist und berechnet Aggregate.
    /// </summary>
    Task<IReadOnlyList<ArtistStatisticsDto>> GetArtistStatisticsAsync(int minSongs, CancellationToken cancellationToken = default);

    /// <summary>
    /// LINQ-Use-Case: Gruppiert Songs nach Genre und berechnet Aggregate.
    /// </summary>
    Task<IReadOnlyList<GenreSummaryDto>> GetGenreSummariesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Erstellt einen Request für einen vorhandenen Song.
    /// </summary>
    Task<SongRequestResponseDto> RequestSongAsync(int songId, RequestSongCmd cmd, CancellationToken cancellationToken = default);

    /// <summary>
    /// Liefert alle offenen Requests.
    /// </summary>
    Task<IReadOnlyList<SongRequestResponseDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Markiert einen Request als gespielt.
    /// </summary>
    Task<SongRequestResponseDto> MarkRequestAsPlayedAsync(int requestId, CancellationToken cancellationToken = default);
}
