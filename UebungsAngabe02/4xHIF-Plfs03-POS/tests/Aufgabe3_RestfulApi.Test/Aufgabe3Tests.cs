using Aufgabe3_RestfulApi.Dtos;
using NSubstitute;
using SPG_Helper;
using System.Net;

namespace Aufgabe3_RestfulApi.Test;

[Collection("Sequential")]
public class Aufgabe3Tests : IDisposable
{
    private readonly TestWebApplicationFactory _factory = new();

    public void Dispose()
    {
        _factory.Dispose();
    }

}