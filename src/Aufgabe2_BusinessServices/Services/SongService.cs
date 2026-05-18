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
/// Er verwendet Entities und DbContext intern, gibt nach außen aber nur DTOs zurück.
/// </summary>
public class SongService : ISongService
{
    private readonly AppDbContext _db;

    /// <summary>
    /// Der DbContext wird per Dependency Injection übergeben.
    /// So bleibt der Service testbar und kennt keine konkrete Datenbankdatei.
    /// </summary>
    public SongService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Lädt alle Songs inklusive Artist und mappt sie zu Response-DTOs.
    /// </summary>
    public async Task<IReadOnlyList<SongResponseDto>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await _db.Songs
            .Include(s => s.Artist)
            .OrderBy(s => s.Title)
            .Select(s => SongMapper.ToDto(s))
            .ToListAsync(cancellationToken);

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
    /// Der Service kümmert sich darum, ob der Artist bereits existiert oder neu angelegt werden muss.
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
    /// Löscht einen Song inklusive seiner Requests, weil im DbContext Cascade Delete eingestellt ist.
    /// </summary>
    public async Task DeleteSongAsync(int id, CancellationToken cancellationToken = default)
    {
        Song song = await FindSongOrThrowAsync(id, cancellationToken);
        _db.Songs.Remove(song);
        await _db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Verwendet die LINQ-Query aus dem DbContext und mappt das Ergebnis auf ein kleines DTO.
    /// </summary>
    public async Task<IReadOnlyList<PopularSongDto>> GetPopularSongsAsync(long minStreams, CancellationToken cancellationToken = default) =>
        await _db.QueryPopularSongs(minStreams)
            .Select(s => SongMapper.ToPopularDto(s))
            .ToListAsync(cancellationToken);

    /// <summary>
    /// Verwendet die LINQ-Query aus dem DbContext für saubere Songs eines Genres.
    /// </summary>
    public async Task<IReadOnlyList<SongResponseDto>> GetCleanSongsByGenreAsync(MusicGenre genre, CancellationToken cancellationToken = default) =>
        await _db.QueryCleanSongsByGenre(genre)
            .Select(s => SongMapper.ToDto(s))
            .ToListAsync(cancellationToken);

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
    /// Liefert alle offenen Requests über eine zentrale LINQ-Query.
    /// </summary>
    public async Task<IReadOnlyList<SongRequestResponseDto>> GetPendingRequestsAsync(CancellationToken cancellationToken = default) =>
        await _db.QueryPendingRequests()
            .Select(r => SongMapper.ToDto(r))
            .ToListAsync(cancellationToken);

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
    /// Sucht einen Artist nach Namen oder legt ihn an, wenn er noch nicht existiert.
    /// </summary>
    private async Task<Artist> GetOrCreateArtistAsync(string artistName, string? country, CancellationToken cancellationToken)
    {
        string normalizedName = artistName.Trim();
        Artist? artist = await _db.Artists.FirstOrDefaultAsync(a => a.Name == normalizedName, cancellationToken);
        if (artist is not null)
        {
            return artist;
        }

        artist = new Artist
        {
            Name = normalizedName,
            Country = string.IsNullOrWhiteSpace(country) ? null : country.Trim()
        };
        _db.Artists.Add(artist);
        return artist;
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
