using Aufgabe2_BusinessServices.Services;
using Aufgabe3_RestfulApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Aufgabe3_RestfulApi.Test;

/// <summary>
/// TestWebApplicationFactory startet die REST-API im Speicher für Controller-Tests.
/// Der echte SongService wird durch ein Mock-Objekt ersetzt.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>
    /// Mock für die Service-Schicht.
    /// Damit testen Aufgabe-3-Tests nur Controller, Routing, JSON, Statuscodes und Validation.
    /// </summary>
    public ISongService SongServiceMock { get; } = Substitute.For<ISongService>();

    /// <summary>
    /// Client ist der HTTP-Client, mit dem Tests echte Requests an die Controller schicken.
    /// </summary>
    public HttpClient Client => CreateClient();

    /// <summary>
    /// ConfigureWebHost überschreibt Services nur für Tests.
    /// Wichtig: Es wird keine Datenbank und keine echte Aufgabe-2-Serviceklasse verwendet.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ISongService>();
            services.AddSingleton(SongServiceMock);
        });

        builder.UseEnvironment("Testing");
    }
}
