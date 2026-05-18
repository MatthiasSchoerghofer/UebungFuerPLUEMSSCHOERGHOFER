using Aufgabe1_ORMapping.Model;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe1_ORMapping.Infrastructure;

/// <summary>
/// AppDbContext ist die EF-Core-Datenbankklasse.
/// Sie definiert Tabellen über DbSets, konfiguriert Constraints und enthält LINQ-Abfragen zum Üben.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Der Konstruktor bekommt die Datenbankoptionen von außen.
    /// Dadurch können App und Tests unterschiedliche Datenbanken verwenden.
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    /// <summary>
    /// Tabelle für Artists.
    /// </summary>
    public DbSet<Artist> Artists => Set<Artist>();

    /// <summary>
    /// Tabelle für Songs.
    /// </summary>
    public DbSet<Song> Songs => Set<Song>();

    /// <summary>
    /// Tabelle für Song-Requests.
    /// </summary>
    public DbSet<SongRequest> SongRequests => Set<SongRequest>();

    /// <summary>
    /// OnModelCreating beschreibt das Datenbankmodell genauer als nur über C# Properties.
    /// Hier werden Pflichtfelder, Längen, Beziehungen und Default-Werte festgelegt.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Artist>(entity =>
        {
            entity.Property(a => a.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(a => a.Country)
                .HasMaxLength(80);

            entity.HasIndex(a => a.Name)
                .IsUnique();
        });

        modelBuilder.Entity<Song>(entity =>
        {
            entity.Property(s => s.Title)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(s => s.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(s => s.Artist)
                .WithMany(a => a.Songs)
                .HasForeignKey(s => s.ArtistId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(s => new { s.ArtistId, s.Title })
                .IsUnique();
        });

        modelBuilder.Entity<SongRequest>(entity =>
        {
            entity.Property(r => r.RequestedBy)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(r => r.Message)
                .HasMaxLength(300);

            entity.Property(r => r.Status)
                .HasDefaultValue(SongRequestStatus.Pending);

            entity.HasOne(r => r.Song)
                .WithMany(s => s.Requests)
                .HasForeignKey(r => r.SongId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// LINQ-Query: Liefert Songs mit mindestens minStreams Streams, sortiert nach Streams absteigend.
    /// In Prüfungen werden solche Methoden oft direkt getestet.
    /// </summary>
    public IQueryable<Song> QueryPopularSongs(long minStreams) =>
        Songs
            .Include(s => s.Artist)
            .Where(s => s.Streams >= minStreams)
            .OrderByDescending(s => s.Streams);

    /// <summary>
    /// LINQ-Query: Liefert nicht explizite Songs eines Genres, zuerst die kürzesten.
    /// Das zeigt Where, OrderBy und Enum-Filter in einer realistischen Abfrage.
    /// </summary>
    public IQueryable<Song> QueryCleanSongsByGenre(MusicGenre genre) =>
        Songs
            .Include(s => s.Artist)
            .Where(s => s.Genre == genre && !s.IsExplicit)
            .OrderBy(s => s.DurationSeconds);

    /// <summary>
    /// LINQ-Query: Liefert offene Requests inklusive Song und Artist.
    /// Include ist wichtig, wenn man in der Antwort verschachtelte Daten braucht.
    /// </summary>
    public IQueryable<SongRequest> QueryPendingRequests() =>
        SongRequests
            .Include(r => r.Song)
            .ThenInclude(s => s.Artist)
            .Where(r => r.Status == SongRequestStatus.Pending)
            .OrderBy(r => r.RequestedAt);
}
