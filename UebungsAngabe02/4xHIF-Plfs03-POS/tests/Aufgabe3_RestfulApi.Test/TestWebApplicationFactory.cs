using Aufgabe3_RestfulApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Aufgabe3_RestfulApi.Test;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public IProjectService ProjectServiceMock { get; } = Substitute.For<IProjectService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.First(d => d.ServiceType == typeof(IProjectService));
            services.Remove(descriptor);

            services.AddSingleton(ProjectServiceMock);
        });
    }

    public HttpClient Client => CreateClient();
}
