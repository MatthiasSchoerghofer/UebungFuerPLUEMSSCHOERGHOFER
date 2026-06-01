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

    [Fact]
    public async Task GetBlogs_ReturnsAllBlogs()
    {
        List<BlogDto> blogs =
        [
            new BlogDto(1, "NET Blog"),
            new BlogDto(2, "Java Blog")
        ];

        _factory.BlogServiceMock
            .GetAllAsync(null, Arg.Any<CancellationToken>())
            .Returns(blogs);

        using HttpClient client = _factory.Client;

        var (status, result) =
            await client.GetHttpContent<List<BlogDto>>("/api/blogs");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetBlogs_WithSearch_CallsServiceWithSearch()
    {
        List<BlogDto> blogs =
        [
            new BlogDto(1, "NET Blog")
        ];

        _factory.BlogServiceMock
            .GetAllAsync("net", Arg.Any<CancellationToken>())
            .Returns(blogs);

        using HttpClient client = _factory.Client;

        var (status, result) =
            await client.GetHttpContent<List<BlogDto>>("/api/blogs?search=net");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);

        await _factory.BlogServiceMock
            .Received(1)
            .GetAllAsync("net", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBlogById_NotExisting_ReturnsNotFound()
    {
        _factory.BlogServiceMock
            .GetByIdAsync(999, Arg.Any<CancellationToken>())
            .Returns((BlogDto?)null);

        using HttpClient client = _factory.Client;

        var response = await client.GetAsync("/api/blogs/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPaged_InvalidPage_ReturnsBadRequest()
    {
        using HttpClient client = _factory.Client;

        var response =
            await client.GetAsync("/api/blogs/paged?page=0&pageSize=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await _factory.BlogServiceMock
            .DidNotReceive()
            .GetAllPagedAsync(
                Arg.Any<string?>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetRecentPosts_ReturnsOk()
    {
        List<RecentPostsDto> posts =
        [
            new RecentPostsDto(
                1,
                "Post 1",
                new DateTime(2024, 1, 1),
                "NET Blog",
                ["NET"])
        ];

        _factory.BlogServiceMock
            .GetRecentPostsAsync(
                new DateTime(2022, 1, 1),
                "NET",
                10,
                Arg.Any<CancellationToken>())
            .Returns(posts);

        using HttpClient client = _factory.Client;

        var (status, result) =
            await client.GetHttpContent<List<RecentPostsDto>>(
                "/api/posts/recent?publishedFrom=2022-01-01&tag=NET&take=10");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task AuthorsByCountry_WithoutCountry_ReturnsBadRequest()
    {
        using HttpClient client = _factory.Client;

        var response = await client.GetAsync("/api/authors/by-country");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}