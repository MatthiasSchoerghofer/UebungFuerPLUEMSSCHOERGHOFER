using Aufgabe3_RestfulApi.Interfaces;
using Aufgabe3_RestfulApi.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Aufgabe3_RestfulApi.Test;

public class TestWebApplicationFactory : WebApplicationFactory<Program> {
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var demoServiceDescriptor = services.First(d => d.ServiceType == typeof(IDemoService));
            services.Remove(demoServiceDescriptor);
            services.AddSingleton(DemoServiceMock);

            var blogServiceDescriptor = services.First(d => d.ServiceType == typeof(IBlogService));
            services.Remove(blogServiceDescriptor);
            services.AddSingleton(BlogServiceMock);
        });

        builder.UseEnvironment("MockTesting");
    }

    public HttpClient Client => CreateClient();

    public IDemoService DemoServiceMock { get; private set; } = Substitute.For<IDemoService>();
    public IBlogService BlogServiceMock { get; private set; } = Substitute.For<IBlogService>();
}
