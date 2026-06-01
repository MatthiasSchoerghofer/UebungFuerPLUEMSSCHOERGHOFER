using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Model;
using SPG_Helper;
using System.Net;

namespace Aufgabe3_RestfulApi.Test;

[Collection("Sequential")]
public class Aufgabe3TestsDB : IClassFixture<TestWebApplicationFactoryDB>
{
    private readonly TestWebApplicationFactoryDB _factory;

    public Aufgabe3TestsDB(TestWebApplicationFactoryDB factory)
    {
        _factory = factory;
    }
    
}