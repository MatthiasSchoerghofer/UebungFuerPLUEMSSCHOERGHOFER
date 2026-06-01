namespace Aufgabe1_ORMapping.Model;

/// <summary>
/// Genre ist ein Value-ähnlicher Enum: Ein Song kann nur eine dieser fixen Kategorien haben.
/// In einer PLÜ ist ein Enum praktisch, wenn Werte nicht frei erfunden werden sollen.
/// </summary>
public enum MusicGenre
{
    Pop = 1,
    Rock = 2,
    HipHop = 3,
    Electronic = 4,
    Classical = 5
}

/// <summary>
/// Der Status zeigt, was mit einem Song-Wunsch passiert ist.
/// Pending bedeutet: Der Wunsch ist offen. Played bedeutet: Der Song wurde abgespielt.
/// </summary>
public enum SongRequestStatus
{
    Pending = 1,
    Played = 2,
    Rejected = 3
}

/// <summary>
/// Artist ist eine EF-Core-Entity und wird als eigene Tabelle gespeichert.
/// Ein Artist kann mehrere Songs haben, daher gibt es die Navigation Property Songs.
/// </summary>
public class Artist
{
    /// <summary>
    /// Primärschlüssel. EF Core erkennt Id automatisch als Key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name des Artists. Die genaue Pflicht und Länge wird im DbContext konfiguriert.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Land des Artists. Das Feld ist optional, deshalb nullable.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Navigation Property: Alle Songs, die zu diesem Artist gehören.
    /// </summary>
    public List<Song> Songs { get; set; } = [];
}

/// <summary>
/// Song ist die zentrale Entity der App.
/// Sie beschreibt hochgeladene Songs, die später über die API gesucht und requested werden können.
/// </summary>
public class Song
{
    /// <summary>
    /// Primärschlüssel des Songs.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Titel des Songs.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Länge in Sekunden. Dadurch kann man gut LINQ-Abfragen für kurze/lange Songs üben.
    /// </summary>
    public int DurationSeconds { get; set; }

    /// <summary>
    /// Stream-Anzahl. Typische Prüfungsabfrage: Gib alle Songs mit mehr als X Streams zurück.
    /// </summary>
    public long Streams { get; set; }

    /// <summary>
    /// Genre des Songs als Enum.
    /// </summary>
    public MusicGenre Genre { get; set; }

    /// <summary>
    /// Gibt an, ob der Song explizite Inhalte hat. Das eignet sich für Filter-Endpunkte.
    /// </summary>
    public bool IsExplicit { get; set; }

    /// <summary>
    /// Zeitpunkt, wann der Song in die App hochgeladen wurde.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// Fremdschlüssel zum Artist.
    /// </summary>
    public int ArtistId { get; set; }

    /// <summary>
    /// Navigation Property zum zugehörigen Artist.
    /// </summary>
    public Artist Artist { get; set; } = null!;

    /// <summary>
    /// Navigation Property: Alle Wünsche/Requests für diesen Song.
    /// </summary>
    public List<SongRequest> Requests { get; set; } = [];
}

/// <summary>
/// SongRequest ist die Entity für einen Song-Wunsch.
/// Damit kann jemand einen vorhandenen Song requesten und optional eine Nachricht dazuschreiben.
/// </summary>
public class SongRequest
{
    /// <summary>
    /// Primärschlüssel des Requests.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name der Person, die den Song requested hat.
    /// </summary>
    public string RequestedBy { get; set; } = string.Empty;

    /// <summary>
    /// Optionale Nachricht, zum Beispiel "bitte für die nächste Pause".
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Zeitpunkt, wann der Wunsch erstellt wurde.
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// Status des Requests. Standardmäßig startet ein Request als Pending.
    /// </summary>
    public SongRequestStatus Status { get; set; } = SongRequestStatus.Pending;

    /// <summary>
    /// Fremdschlüssel zum gewünschten Song.
    /// </summary>
    public int SongId { get; set; }

    /// <summary>
    /// Navigation Property zum gewünschten Song.
    /// </summary>
    public Song Song { get; set; } = null!;
}
