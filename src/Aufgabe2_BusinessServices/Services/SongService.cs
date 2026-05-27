using Aufgabe1_ORMapping.Infrastructure;
using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;
using Aufgabe2_BusinessServices.Exceptions;
using Aufgabe2_BusinessServices.Mapper;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe2_BusinessServices.Services;

/// <summary>
/// SongService enthält die Businesslogik der Song-App.
/// Er arbeitet direkt mit dem EF-Core-DbContext und gibt nach außen nur DTOs zurück.
/// </summary>
public class SongService : ISongService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Der DbContext wird per Dependency Injection übergeben.
    /// Dadurch verwendet die API echte Datenbankdaten statt Mock-Listen.
    /// </summary>
    public SongService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lädt alle Songs und mappt sie zu Response-DTOs.
    /// </summary>
    public async Task<IReadOnlyList<SongResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Song> songs = await _db.Songs
            .Include(s => s.Artist)
            .OrderBy(s => s.Title)
            .ToListAsync(cancellationToken);

        return songs.Select(SongMapper.ToDto).ToList();
    }

    /// <summary>
    /// Lädt nur eine Seite der Songs.
    /// Wichtig für Pagination: immer zuerst stabil sortieren, dann Skip und Take anwenden.
    /// </summary>
    public async Task<PagedResultDto<SongResponseDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        if (page < 1)
        {
            throw new ArgumentException("Page must be at least 1.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            throw new ArgumentException("PageSize must be between 1 and 100.");
        }

        int totalCount = await _db.Songs.CountAsync(cancellationToken);
        List<Song> songs = await _db.Songs
            .Include(s => s.Artist)
            .OrderBy(s => s.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<SongResponseDto>(
            songs.Select(SongMapper.ToDto).ToList(),
            page,
            pageSize,
            totalCount);
    }

    /// <summary>
    /// Lädt einen Song per Id.
    /// Wenn kein Song existiert, wird eine fachliche NotFoundException geworfen.
    /// </summary>
    public async Task<SongResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Song song = await FindSongOrThrowAsync(id, cancellationToken);
        return SongMapper.ToDto(song);
    }

    /// <summary>
    /// Erstellt einen neuen Song.
    /// Der Service kümmert sich darum, ob der Artist in der Datenbank bereits existiert oder neu angelegt werden muss.
    /// </summary>
    public async Task<SongResponseDto> UploadSongAsync(UploadSongCmd cmd, CancellationToken cancellationToken = default)
    {
        ValidateUpload(cmd);

        Artist artist = await GetOrCreateArtistAsync(cmd.ArtistName, cmd.ArtistCountry, cancellationToken);
        Song song = SongMapper.ToEntity(cmd, artist, DateTime.UtcNow);

        _db.Songs.Add(song);
        await _db.SaveChangesAsync(cancellationToken);

        return SongMapper.ToDto(song);
    }

    /// <summary>
    /// Aktualisiert nur die Stream-Anzahl.
    /// Kleine, fokussierte Update-Methoden sind in Prüfungen leichter zu testen.
    /// </summary>
    public async Task<SongResponseDto> UpdateStreamsAsync(int id, UpdateStreamsCmd cmd, CancellationToken cancellationToken = default)
    {
        if (cmd.Streams < 0)
        {
            throw new ArgumentException("Streams must not be negative.");
        }

        Song song = await FindSongOrThrowAsync(id, cancellationToken);
        song.Streams = cmd.Streams;
        await _db.SaveChangesAsync(cancellationToken);

        return SongMapper.ToDto(song);
    }

    /// <summary>
    /// Löscht einen Song inklusive seiner Requests.
    /// </summary>
    public async Task DeleteSongAsync(int id, CancellationToken cancellationToken = default)
    {
        Song song = await _db.Songs
            .Include(s => s.Requests)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException($"Song with id {id} was not found.");

        _db.Songs.Remove(song);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Filtert Songs mit LINQ auf dem DbContext und mappt das Ergebnis auf ein kleines DTO.
    /// </summary>
    public async Task<IReadOnlyList<PopularSongDto>> GetPopularSongsAsync(long minStreams, CancellationToken cancellationToken = default)
    {
        List<Song> songs = await _db.QueryPopularSongs(minStreams).ToListAsync(cancellationToken);
        return songs.Select(SongMapper.ToPopularDto).ToList();
    }

    /// <summary>
    /// Filtert nicht explizite Songs eines Genres per LINQ auf dem DbContext.
    /// </summary>
    public async Task<IReadOnlyList<SongResponseDto>> GetCleanSongsByGenreAsync(MusicGenre genre, CancellationToken cancellationToken = default)
    {
        List<Song> songs = await _db.QueryCleanSongsByGenre(genre).ToListAsync(cancellationToken);
        return songs.Select(SongMapper.ToDto).ToList();
    }

    /// <summary>
    /// Erstellt einen Song-Request für einen vorhandenen Song.
    /// Die SongId kommt aus der Route, die restlichen Daten aus dem Request-DTO.
    /// </summary>
    public async Task<SongRequestResponseDto> RequestSongAsync(int songId, RequestSongCmd cmd, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.RequestedBy))
        {
            throw new ArgumentException("RequestedBy is required.");
        }

        Song song = await FindSongOrThrowAsync(songId, cancellationToken);
        SongRequest request = new()
        {
            Song = song,
            SongId = song.Id,
            RequestedBy = cmd.RequestedBy.Trim(),
            Message = string.IsNullOrWhiteSpace(cmd.Message) ? null : cmd.Message.Trim(),
            RequestedAt = DateTime.UtcNow,
            Status = SongRequestStatus.Pending
        };

        _db.SongRequests.Add(request);
        await _db.SaveChangesAsync(cancellationToken);

        return SongMapper.ToDto(request);
    }

    /// <summary>
    /// Liefert alle offenen Requests über LINQ auf dem DbContext.
    /// </summary>
    public async Task<IReadOnlyList<SongRequestResponseDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        List<SongRequest> requests = await _db.QueryPendingRequests().ToListAsync(cancellationToken);
        return requests.Select(SongMapper.ToDto).ToList();
    }

    /// <summary>
    /// Setzt einen Request von Pending auf Played.
    /// </summary>
    public async Task<SongRequestResponseDto> MarkRequestAsPlayedAsync(int requestId, CancellationToken cancellationToken = default)
    {
        SongRequest request = await _db.SongRequests
            .Include(r => r.Song)
            .ThenInclude(s => s.Artist)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken)
            ?? throw new NotFoundException($"Song request with id {requestId} was not found.");

        request.Status = SongRequestStatus.Played;
        await _db.SaveChangesAsync(cancellationToken);

        return SongMapper.ToDto(request);
    }

    /// <summary>
    /// Gemeinsame Hilfsmethode, damit "Song nicht gefunden" überall gleich behandelt wird.
    /// </summary>
    private async Task<Song> FindSongOrThrowAsync(int id, CancellationToken cancellationToken) =>
        await _db.Songs
            .Include(s => s.Artist)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
        ?? throw new NotFoundException($"Song with id {id} was not found.");

    /// <summary>
    /// Sucht einen Artist nach Namen in der Datenbank oder legt ihn an, wenn er noch nicht existiert.
    /// </summary>
    private async Task<Artist> GetOrCreateArtistAsync(string artistName, string? country, CancellationToken cancellationToken)
    {
        string normalizedName = artistName.Trim();
        Artist? artist = await _db.Artists
            .FirstOrDefaultAsync(a => a.Name == normalizedName, cancellationToken);

        if (artist is not null)
        {
            return artist;
        }

        return new Artist
        {
            Name = normalizedName,
            Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim()
        };
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
