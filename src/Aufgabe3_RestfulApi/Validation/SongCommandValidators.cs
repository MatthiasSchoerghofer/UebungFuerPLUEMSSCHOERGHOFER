using Aufgabe2_BusinessServices.Cmds;
using FluentValidation;

namespace Aufgabe3_RestfulApi.Validation;

/// <summary>
/// Validierung für POST /api/songs.
/// Diese Regeln gehören zu Aufgabe 3, weil der Controller auch mit gemocktem Service korrekt antworten muss.
/// </summary>
public class UploadSongCmdValidator : AbstractValidator<UploadSongCmd>
{
    public UploadSongCmdValidator()
    {
        RuleFor(cmd => cmd.Title)
            .NotEmpty()
            .MaximumLength(160);

        RuleFor(cmd => cmd.ArtistName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(cmd => cmd.ArtistCountry)
            .MaximumLength(80);

        RuleFor(cmd => cmd.DurationSeconds)
            .GreaterThan(0);

        RuleFor(cmd => cmd.Streams)
            .GreaterThanOrEqualTo(0);
    }
}

/// <summary>
/// Validierung für PUT /api/songs/{id}/streams.
/// </summary>
public class UpdateStreamsCmdValidator : AbstractValidator<UpdateStreamsCmd>
{
    public UpdateStreamsCmdValidator()
    {
        RuleFor(cmd => cmd.Streams)
            .GreaterThanOrEqualTo(0);
    }
}

/// <summary>
/// Validierung für POST /api/songs/{songId}/requests.
/// </summary>
public class RequestSongCmdValidator : AbstractValidator<RequestSongCmd>
{
    public RequestSongCmdValidator()
    {
        RuleFor(cmd => cmd.RequestedBy)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(cmd => cmd.Message)
            .MaximumLength(300);
    }
}
