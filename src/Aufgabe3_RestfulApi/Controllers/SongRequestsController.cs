using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;
using Aufgabe2_BusinessServices.Exceptions;
using Aufgabe2_BusinessServices.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aufgabe3_RestfulApi.Controllers;

/// <summary>
/// SongRequestsController ist für Song-Wünsche zuständig.
/// Requests sind eine eigene Ressource, deshalb bekommen sie einen eigenen Controller.
/// </summary>
[Route("api")]
[ApiController]
public class SongRequestsController : ControllerBase
{
    private readonly ISongService _service;

    /// <summary>
    /// Der Controller bekommt den Service per Dependency Injection.
    /// </summary>
    public SongRequestsController(ISongService service)
    {
        _service = service;
    }

    /// <summary>
    /// POST /api/songs/{songId}/requests
    /// Erstellt einen neuen Wunsch für einen vorhandenen Song.
    /// </summary>
    [HttpPost("songs/{songId:int}/requests")]
    [ProducesResponseType(typeof(SongRequestResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SongRequestResponseDto>> RequestSong(int songId, RequestSongCmd cmd, CancellationToken cancellationToken)
    {
        try
        {
            SongRequestResponseDto created = await _service.RequestSongAsync(songId, cmd, cancellationToken);
            return CreatedAtAction(nameof(GetPendingRequests), new { id = created.Id }, created);
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
    /// GET /api/requests/pending
    /// LINQ-Endpunkt: Liefert alle offenen Song-Wünsche in zeitlicher Reihenfolge.
    /// </summary>
    [HttpGet("requests/pending")]
    [ProducesResponseType(typeof(IReadOnlyList<SongRequestResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SongRequestResponseDto>>> GetPendingRequests(CancellationToken cancellationToken)
    {
        return Ok(await _service.GetPendingRequestsAsync(cancellationToken));
    }

    /// <summary>
    /// PUT /api/requests/{requestId}/played
    /// Markiert einen Request als abgespielt.
    /// </summary>
    [HttpPut("requests/{requestId:int}/played")]
    [ProducesResponseType(typeof(SongRequestResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SongRequestResponseDto>> MarkAsPlayed(int requestId, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await _service.MarkRequestAsPlayedAsync(requestId, cancellationToken));
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails { Detail = ex.Message });
        }
    }
}
