using Aufgabe1_ORMapping.Model;
using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;
using Aufgabe2_BusinessServices.Exceptions;
using Aufgabe2_BusinessServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aufgabe3_RestfulApi.Controllers;

/// <summary>
/// SongsController ist die HTTP-Schicht für Songs.
/// Er arbeitet nur mit DTOs/Commands und delegiert Businesslogik an den SongService.
/// </summary>
[Route("api/songs")]
[ApiController]
public class SongsController : ControllerBase
{
    private readonly ISongService _service;

    /// <summary>
    /// Der Service wird per Dependency Injection übergeben.
    /// Der Controller kennt dadurch keine Datenbank und keine Entity-Erstellung.
    /// </summary>
    public SongsController(ISongService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/songs
    /// Liefert alle Songs als DTO-Liste.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SongResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SongResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    /// <summary>
    /// GET /api/songs/{id}
    /// Liefert einen bestimmten Song oder 404, wenn die Id nicht existiert.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SongResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SongResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.GetByIdAsync(id, cancellationToken));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Detail = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/songs
    /// Lädt einen neuen Song hoch und gibt 201 Created zurück.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SongResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SongResponseDto>> Upload(UploadSongCmd cmd, CancellationToken cancellationToken)
    {
        try
        {
            SongResponseDto created = await _service.UploadSongAsync(cmd, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }

    /// <summary>
    /// PUT /api/songs/{id}/streams
    /// Aktualisiert die Stream-Anzahl eines Songs.
    /// </summary>
    [HttpPut("{id:int}/streams")]
    [ProducesResponseType(typeof(SongResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SongResponseDto>> UpdateStreams(int id, UpdateStreamsCmd cmd, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.UpdateStreamsAsync(id, cmd, cancellationToken));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails { Detail = ex.Message });
        }
    }

    /// <summary>
    /// DELETE /api/songs/{id}
    /// Löscht einen Song und gibt 204 NoContent zurück.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteSongAsync(id, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Detail = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/songs/popular?minStreams=1000000
    /// LINQ-Endpunkt: Liefert Songs ab einer Mindestanzahl an Streams.
    /// </summary>
    [HttpGet("popular")]
    [ProducesResponseType(typeof(IReadOnlyList<PopularSongDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PopularSongDto>>> GetPopular([FromQuery] long minStreams = 1_000_000, CancellationToken cancellationToken = default)
    {
        return Ok(await _service.GetPopularSongsAsync(minStreams, cancellationToken));
    }

    /// <summary>
    /// GET /api/songs/clean/{genre}
    /// LINQ-Endpunkt: Liefert nicht explizite Songs eines Genres.
    /// </summary>
    [HttpGet("clean/{genre}")]
    [ProducesResponseType(typeof(IReadOnlyList<SongResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SongResponseDto>>> GetCleanByGenre(MusicGenre genre, CancellationToken cancellationToken)
    {
        return Ok(await _service.GetCleanSongsByGenreAsync(genre, cancellationToken));
    }
}
