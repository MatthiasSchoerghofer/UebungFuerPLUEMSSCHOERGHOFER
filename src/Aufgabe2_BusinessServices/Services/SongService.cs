using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;
using Aufgabe2_BusinessServices.Exceptions;
using Aufgabe2_BusinessServices.Mapper;

namespace Aufgabe2_BusinessServices.Services;

/// <summary>
/// SongService enthält die Businesslogik der Song-App.
/// Er verwendet Entities intern, gibt nach außen aber nur DTOs zurück.
/// Für Aufgabe 2 arbeitet er bewusst ohne DbContext auf einer übergebenen Entity-Liste.
/// </summary>
public class SongService : ISongService
{
    private readonly IList<Song> _songs;

    /// <summary>
    /// Die Entity-Liste wird von außen übergeben.
    /// Dadurch kann Aufgabe 2 ohne echte Datenbank und ohne EF-Core-Testsetup geprüft werden.
    /// </summary>
    public SongService(IList<Song> songs)
    {
        _songs = songs;
    }

    /// <summary>
    /// Lädt alle Songs und mappt sie zu Response-DTOs.
    /// </summary>
    public Task<IReadOnlyList<SongResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<SongResponseDto> result = _songs
            .OrderBy(s => s.Title)
            .Select(SongMapper.ToDto)
            .ToList();

        return Task.FromResult(result);
    }

    /// <summary>
    /// Lädt nur eine Seite der Songs.
    /// Wichtig für Pagination: immer zuerst stabil sortieren, dann Skip und Take anwenden.
    /// </summary>
    public Task<PagedResultDto<SongResponseDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (page < 1)
        {
            throw new ArgumentException("Page must be at least 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("PageSize must be between 1 and 100.");
        }

        int totalCount = _songs.Count;
        List<SongResponseDto> items = _songs
            .OrderBy(s => s.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(SongMapper.ToDto)
            .ToList();

        return Task.FromResult(new PagedResultDto<SongResponseDto>(items, page, pageSize, totalCount));
    }

    /// <summary>
    /// Lädt einen Song per Id.
    /// Wenn kein Song existiert, wird eine fachliche NotFoundException geworfen.
    /// </summary>
    public Task<SongResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Song song = FindSongOrThrow(id);
        return Task.FromResult(SongMapper.ToDto(song));
    }

    /// <summary>
    /// Erstellt einen neuen Song.
    /// Der Service kümmert sich darum, ob der Artist in der Liste bereits existiert oder neu angelegt werden muss.
    /// </summary>
    public Task<SongResponseDto> UploadSongAsync(UploadSongCmd cmd, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateUpload(cmd);

        Artist artist = GetOrCreateArtist(cmd.ArtistName, cmd.ArtistCountry);
        Song song = SongMapper.ToEntity(cmd, artist, DateTime.UtcNow);
        song.Id = NextSongId();
        song.ArtistId = artist.Id;

        _songs.Add(song);
        artist.Songs.Add(song);

        return Task.FromResult(SongMapper.ToDto(song));
    }

    /// <summary>
    /// Aktualisiert nur die Stream-Anzahl.
    /// Kleine, fokussierte Update-Methoden sind in Prüfungen leichter zu testen.
    /// </summary>
    public Task<SongResponseDto> UpdateStreamsAsync(int id, UpdateStreamsCmd cmd, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (cmd.Streams < 0)
        {
            throw new ArgumentException("Streams must not be negative.");
        }

        Song song = FindSongOrThrow(id);
        song.Streams = cmd.Streams;

        return Task.FromResult(SongMapper.ToDto(song));
    }

    /// <summary>
    /// Löscht einen Song inklusive seiner Requests aus der In-Memory-Liste.
    /// </summary>
    public Task DeleteSongAsync(int id, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Song song = FindSongOrThrow(id);
        song.Artist.Songs.Remove(song);
        _songs.Remove(song);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Filtert die Entity-Liste mit LINQ und mappt das Ergebnis auf ein kleines DTO.
    /// </summary>
    public Task<IReadOnlyList<PopularSongDto>> GetPopularSongsAsync(long minStreams, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<PopularSongDto> result = _songs
            .Where(s => s.Streams >= minStreams)
            .OrderByDescending(s => s.Streams)
            .Select(SongMapper.ToPopularDto)
            .ToList();

        return Task.FromResult(result);
    }

    /// <summary>
    /// Filtert die Entity-Liste mit LINQ für saubere Songs eines Genres.
    /// </summary>
    public Task<IReadOnlyList<SongResponseDto>> GetCleanSongsByGenreAsync(MusicGenre genre, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<SongResponseDto> result = _songs
            .Where(s => s.Genre == genre && !s.IsExplicit)
            .OrderBy(s => s.DurationSeconds)
            .Select(SongMapper.ToDto)
            .ToList();

        return Task.FromResult(result);
    }

    /// <summary>
    /// Erstellt einen Song-Request für einen vorhandenen Song.
    /// Die SongId kommt aus der Route, die restlichen Daten aus dem Request-DTO.
    /// </summary>
    public Task<SongRequestResponseDto> RequestSongAsync(int songId, RequestSongCmd cmd, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(cmd.RequestedBy))
        {
            throw new ArgumentException("RequestedBy is required.");
        }

        Song song = FindSongOrThrow(songId);
        SongRequest request = new()
        {
            Id = NextRequestId(),
            Song = song,
            SongId = song.Id,
            RequestedBy = cmd.RequestedBy.Trim(),
            Message = string.IsNullOrWhiteSpace(cmd.Message) ? null : cmd.Message.Trim(),
            RequestedAt = DateTime.UtcNow,
            Status = SongRequestStatus.Pending
        };

        song.Requests.Add(request);

        return Task.FromResult(SongMapper.ToDto(request));
    }

    /// <summary>
    /// Liefert alle offenen Requests über LINQ auf der Entity-Liste.
    /// </summary>
    public Task<IReadOnlyList<SongRequestResponseDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        IReadOnlyList<SongRequestResponseDto> result = AllRequests()
            .Where(r => r.Status == SongRequestStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .Select(SongMapper.ToDto)
            .ToList();

        return Task.FromResult(result);
    }

    /// <summary>
    /// Setzt einen Request von Pending auf Played.
    /// </summary>
    public Task<SongRequestResponseDto> MarkRequestAsPlayedAsync(int requestId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SongRequest request = AllRequests().FirstOrDefault(r => r.Id == requestId)
            ?? throw new NotFoundException($"Song request with id {requestId} was not found.");

        request.Status = SongRequestStatus.Played;

        return Task.FromResult(SongMapper.ToDto(request));
    }

    /// <summary>
    /// Gemeinsame Hilfsmethode, damit "Song nicht gefunden" überall gleich behandelt wird.
    /// </summary>
    private Song FindSongOrThrow(int id) =>
        _songs.FirstOrDefault(s => s.Id == id)
        ?? throw new NotFoundException($"Song with id {id} was not found.");

    /// <summary>
    /// Sucht einen Artist nach Namen in der Liste oder legt ihn an, wenn er noch nicht existiert.
    /// </summary>
    private Artist GetOrCreateArtist(string artistName, string? country)
    {
        string normalizedName = artistName.Trim();
        Artist? artist = _songs
            .Select(s => s.Artist)
            .DistinctBy(a => a.Id)
            .FirstOrDefault(a => a.Name == normalizedName);

        if (artist is not null)
        {
            return artist;
        }

        return new Artist
        {
            Id = NextArtistId(),
            Name = normalizedName,
            Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim()
        };
    }

    /// <summary>
    /// Sammelt alle Requests aus den Songs.
    /// </summary>
    private IEnumerable<SongRequest> AllRequests() => _songs.SelectMany(s => s.Requests);

    /// <summary>
    /// Simuliert Identity-IDs für neue Songs.
    /// </summary>
    private int NextSongId() => _songs.Count == 0 ? 1 : _songs.Max(s => s.Id) + 1;

    /// <summary>
    /// Simuliert Identity-IDs für neue Artists.
    /// </summary>
    private int NextArtistId()
    {
        List<Artist> artists = _songs.Select(s => s.Artist).DistinctBy(a => a.Id).ToList();
        return artists.Count == 0 ? 1 : artists.Max(a => a.Id) + 1;
    }

    /// <summary>
    /// Simuliert Identity-IDs für neue Requests.
    /// </summary>
    private int NextRequestId()
    {
        List<SongRequest> requests = AllRequests().ToList();
        return requests.Count == 0 ? 1 : requests.Max(r => r.Id) + 1;
    }

    /// <summary>
    /// Prüft die wichtigsten fachlichen Regeln vor dem Speichern eines Songs.
    /// </summary>
    private static void ValidateUpload(UploadSongCmd cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.Title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (string.IsNullOrWhiteSpace(cmd.ArtistName))
        {
            throw new ArgumentException("ArtistName is required.");
        }

        if (cmd.DurationSeconds <= 0)
        {
            throw new ArgumentException("DurationSeconds must be greater than zero.");
        }

        if (cmd.Streams < 0)
        {
            throw new ArgumentException("Streams must not be negative.");
        }
    }
}
