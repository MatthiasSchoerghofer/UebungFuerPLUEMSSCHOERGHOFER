namespace Aufgabe2_BusinessServices.Exceptions;

/// <summary>
/// Eigene Exception für "nicht gefunden".
/// Die Minimal API übersetzt diese Exception später in HTTP 404 NotFound.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Erzeugt eine neue NotFoundException mit einer verständlichen Fehlermeldung.
    /// </summary>
    public NotFoundException(string message) : base(message) { }
}
