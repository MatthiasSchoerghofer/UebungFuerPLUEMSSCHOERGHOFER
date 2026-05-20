using Aufgabe2_BusinessServices.Cmds;
using Aufgabe2_BusinessServices.DTOs;
using Aufgabe2_BusinessServices.Exceptions;
using Aufgabe2_BusinessServices.Services;
using FluentValidation;
using FluentValidation.Results;
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
    private readonly IValidator<RequestSongCmd> _requestSongValidator;

    /// <summary>
    /// Der Controller bekommt den Service per Dependency Injection.
    /// </summary>
    public SongRequestsController(ISongService service, IValidator<RequestSongCmd> requestSongValidator)
    {
        _service = service;
        _requestSongValidator = requestSongValidator;
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
        ValidationResult validationResult = await _requestSongValidator.ValidateAsync(cmd, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(ToErrorDictionary(validationResult)));
        }

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

    /// <summary>
    /// Wandelt FluentValidation-Fehler in das ValidationProblem-Format von ASP.NET Core um.
    /// </summary>
    private static Dictionary<string, string[]> ToErrorDictionary(ValidationResult validationResult) =>
        validationResult.Errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());
}
