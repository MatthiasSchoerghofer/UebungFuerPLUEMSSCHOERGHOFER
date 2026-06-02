using System;
using System.Net.Http;
using Xunit;

namespace Aufgabe3_RestfulApi.Test;

[CollectionDefinition("Sequential")]
public class SequentialCollection
{
}

[Collection("Sequential")]
public class Aufgabe3Tests : IDisposable
{
    private readonly TestWebApplicationFactory _factory;

    public Aufgabe3Tests()
    {
        _factory = new TestWebApplicationFactory();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public void WebApplicationFactory_CanCreateClient()
    {
        using HttpClient client = _factory.Client;
        Assert.NotNull(client);
    }
}
