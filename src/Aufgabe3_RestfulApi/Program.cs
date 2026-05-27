using Aufgabe1_ORMapping.Infrastructure;
using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;
using Aufgabe2_BusinessServices.Exceptions;
using Aufgabe2_BusinessServices.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi;

/// <summary>
/// Program ist der Startpunkt der ASP.NET-Core-App.
/// Hier werden Dependency Injection, Minimal API, Swagger und die Datenbank konfiguriert.
/// </summary>
public class Program
{
    /// <summary>
    /// Main baut die WebApplication auf und startet den HTTP-Server.
    /// Die HTTP-Schicht wird bewusst nur mit Minimal APIs umgesetzt.
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddValidation();
        builder.Services.AddValidatorsFromAssemblyContaining<Program>();

        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=songs.db";

        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));
        builder.Services.AddScoped<ISongService, SongService>();

        var app = builder.Build();

        using (IServiceScope scope = app.Services.CreateScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedDatabase(db);
        }

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        RouteGroupBuilder api = app.MapGroup("/api");

        api.MapGet("/songs", async (ISongService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetAllAsync(cancellationToken)));

        api.MapGet("/songs/paged", async (
            [FromQuery] int? page,
            [FromQuery] int? pageSize,
            ISongService service,
            CancellationToken cancellationToken) =>
        {
            int requestedPage = page ?? 1;
            int requestedPageSize = pageSize ?? 10;

            if (requestedPage < 1)
            {
                return Results.BadRequest(new ProblemDetails { Detail = "Page must be at least 1." });
            }

            if (requestedPageSize < 1 || requestedPageSize > 100)
            {
                return Results.BadRequest(new ProblemDetails { Detail = "PageSize must be between 1 and 100." });
            }

            try
            {
                return Results.Ok(await service.GetPagedAsync(requestedPage, requestedPageSize, cancellationToken));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new ProblemDetails { Detail = ex.Message });
            }
        });

        api.MapGet("/songs/{id:int}", async (int id, ISongService service, CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await service.GetByIdAsync(id, cancellationToken));
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new ProblemDetails { Detail = ex.Message });
            }
        });

        api.MapPost("/songs", async (
            UploadSongCmd cmd,
            IValidator<UploadSongCmd> validator,
            ISongService service,
            CancellationToken cancellationToken) =>
        {
            ValidationResult validationResult = await validator.ValidateAsync(cmd, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(ToErrorDictionary(validationResult));
            }

            try
            {
                SongResponseDto created = await service.UploadSongAsync(cmd, cancellationToken);
                return Results.Created($"/api/songs/{created.Id}", created);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new ProblemDetails { Detail = ex.Message });
            }
        });

        api.MapPut("/songs/{id:int}/streams", async (
            int id,
            UpdateStreamsCmd cmd,
            IValidator<UpdateStreamsCmd> validator,
            ISongService service,
            CancellationToken cancellationToken) =>
        {
            ValidationResult validationResult = await validator.ValidateAsync(cmd, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(ToErrorDictionary(validationResult));
            }

            try
            {
                return Results.Ok(await service.UpdateStreamsAsync(id, cmd, cancellationToken));
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new ProblemDetails { Detail = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new ProblemDetails { Detail = ex.Message });
            }
        });

        api.MapDelete("/songs/{id:int}", async (int id, ISongService service, CancellationToken cancellationToken) =>
        {
            try
            {
                await service.DeleteSongAsync(id, cancellationToken);
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new ProblemDetails { Detail = ex.Message });
            }
        });

        api.MapGet("/songs/popular", async (
            [FromQuery] long? minStreams,
            ISongService service,
            CancellationToken cancellationToken) =>
        {
            long requestedMinStreams = minStreams ?? 1_000_000;

            if (requestedMinStreams < 0)
            {
                return Results.BadRequest(new ProblemDetails { Detail = "MinStreams must not be negative." });
            }

            return Results.Ok(await service.GetPopularSongsAsync(requestedMinStreams, cancellationToken));
        });

        api.MapGet("/songs/clean/{genre}", async (
            MusicGenre genre,
            ISongService service,
            CancellationToken cancellationToken) =>
            Results.Ok(await service.GetCleanSongsByGenreAsync(genre, cancellationToken)));

        api.MapPost("/songs/{songId:int}/requests", async (
            int songId,
            RequestSongCmd cmd,
            IValidator<RequestSongCmd> validator,
            ISongService service,
            CancellationToken cancellationToken) =>
        {
            ValidationResult validationResult = await validator.ValidateAsync(cmd, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Results.ValidationProblem(ToErrorDictionary(validationResult));
            }

            try
            {
                SongRequestResponseDto created = await service.RequestSongAsync(songId, cmd, cancellationToken);
                return Results.Created($"/api/requests/{created.Id}", created);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new ProblemDetails { Detail = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new ProblemDetails { Detail = ex.Message });
            }
        });

        api.MapGet("/requests/pending", async (ISongService service, CancellationToken cancellationToken) =>
            Results.Ok(await service.GetPendingRequestsAsync(cancellationToken)));

        api.MapPut("/requests/{requestId:int}/played", async (
            int requestId,
            ISongService service,
            CancellationToken cancellationToken) =>
        {
            try
            {
                return Results.Ok(await service.MarkRequestAsPlayedAsync(requestId, cancellationToken));
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new ProblemDetails { Detail = ex.Message });
            }
        });

        app.Run();
    }

    /// <summary>
    /// Wandelt FluentValidation-Fehler in das ValidationProblem-Format von ASP.NET Core um.
    /// </summary>
    private static Dictionary<string, string[]> ToErrorDictionary(ValidationResult validationResult) =>
        validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

    /// <summary>
    /// Legt kleine Startdaten an, damit die API sofort echte DbContext-Daten liefert.
    /// </summary>
    private static void SeedDatabase(AppDbContext db)
    {
        if (db.Songs.Any())
        {
            return;
        }

        Artist daftPunk = new() { Name = "Daft Punk", Country = "France" };
        Artist queen = new() { Name = "Queen", Country = "United Kingdom" };

        Song oneMoreTime = new()
        {
            Title = "One More Time",
            Artist = daftPunk,
            DurationSeconds = 320,
            Streams = 2_100_000,
            Genre = MusicGenre.Electronic,
            IsExplicit = false,
            UploadedAt = new DateTime(2026, 5, 17, 8, 0, 0, DateTimeKind.Utc)
        };

        Song harderBetter = new()
        {
            Title = "Harder Better Faster Stronger",
            Artist = daftPunk,
            DurationSeconds = 224,
            Streams = 950_000,
            Genre = MusicGenre.Electronic,
            IsExplicit = false,
            UploadedAt = new DateTime(2026, 5, 17, 9, 0, 0, DateTimeKind.Utc)
        };

        Song bohemian = new()
        {
            Title = "Bohemian Rhapsody",
            Artist = queen,
            DurationSeconds = 354,
            Streams = 3_500_000,
            Genre = MusicGenre.Rock,
            IsExplicit = false,
            UploadedAt = new DateTime(2026, 5, 17, 10, 0, 0, DateTimeKind.Utc)
        };

        Song explicitSong = new()
        {
            Title = "Explicit Test Song",
            Artist = queen,
            DurationSeconds = 180,
            Streams = 1_200_000,
            Genre = MusicGenre.Rock,
            IsExplicit = true,
            UploadedAt = new DateTime(2026, 5, 17, 11, 0, 0, DateTimeKind.Utc)
        };

        db.Songs.AddRange(oneMoreTime, harderBetter, bohemian, explicitSong);
        db.SongRequests.AddRange(
            new SongRequest { Song = bohemian, RequestedBy = "Anna", RequestedAt = new DateTime(2026, 5, 17, 12, 0, 0, DateTimeKind.Utc), Status = SongRequestStatus.Pending },
            new SongRequest { Song = oneMoreTime, RequestedBy = "Mahi", Message = "Bitte spielen.", RequestedAt = new DateTime(2026, 5, 17, 11, 0, 0, DateTimeKind.Utc), Status = SongRequestStatus.Pending },
            new SongRequest { Song = harderBetter, RequestedBy = "Tom", RequestedAt = new DateTime(2026, 5, 17, 13, 0, 0, DateTimeKind.Utc), Status = SongRequestStatus.Played });

        db.SaveChanges();
    }
}
