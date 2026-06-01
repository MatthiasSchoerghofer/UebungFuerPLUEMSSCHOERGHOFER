using Aufgabe3_RestfulApi.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Aufgabe3_RestfulApi.Test;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public IBlogService BlogServiceMock { get; } = Substitute.For<IBlogService>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.First(d => d.ServiceType == typeof(IBlogService));
            services.Remove(descriptor);

            services.AddSingleton(BlogServiceMock);
        });
    }

    public HttpClient Client => CreateClient();
}