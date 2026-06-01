using Aufgabe3_RestfulApi.Dtos;
using NSubstitute;
using SPG_Helper;
using System.Net;

namespace Aufgabe3_RestfulApi.Test;

[CollectionDefinition("Sequential")]
public class SequentialCollection {
    // Intentionally empty. Using a fixture type is optional here.
    // The presence of this definition allows using [Collection("Sequential")] on test classes.
}

[Collection("Sequential")]
public class Aufgabe3Tests : IDisposable {
    private readonly TestWebApplicationFactory _factory;

    public Aufgabe3Tests()
    {
        _factory = new TestWebApplicationFactory();
    }

    public void Dispose()
    {
        _factory?.Dispose();
    }

    [Fact]
    public async Task Get_Blogs_UsesMockedService()
    {
        List<BlogDto> data = [new(1, "Mock Blog", 2)];
        _factory.BlogServiceMock.GetAllAsync(null).Returns(Task.FromResult(data));

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<List<BlogDto>>("/api/blogs");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Mock Blog", result[0].Name);
        await _factory.BlogServiceMock.Received(1).GetAllAsync(null);
    }

    [Fact]
    public async Task Get_ById_UsesMockedService()
    {
        var blog = new BlogDto(7, "Mock Blog", 3);
        _factory.BlogServiceMock.GetByIdAsync(7).Returns(Task.FromResult<BlogDto?>(blog));

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<BlogDto>("/api/blogs/7");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("Mock Blog", result.Name);
        Assert.Equal(3, result.PostCount);
        await _factory.BlogServiceMock.Received(1).GetByIdAsync(7);
    }

    [Fact]
    public async Task Post_Put_Delete_UsesMockedService()
    {
        _factory.BlogServiceMock
            .CreateAsync(Arg.Any<BlogCreateCmd>())
            .Returns(Task.FromResult(new BlogDto(12, "POS Blog", 0)));
        _factory.BlogServiceMock
            .UpdateAsync(12, Arg.Any<BlogUpdateCmd>())
            .Returns(Task.FromResult<BlogDto?>(new BlogDto(12, "POS Blog Updated", 0)));
        _factory.BlogServiceMock
            .DeleteAsync(12)
            .Returns(Task.FromResult(true));

        using HttpClient client = _factory.Client;

        var (createStatus, created) = await client.PostHttpContent("/api/blogs", new BlogCreateCmd("POS Blog"));
        Assert.Equal(HttpStatusCode.Created, createStatus.StatusCode);
        Assert.Equal(12, created.GetProperty("id").GetInt32());

        var (updateStatus, updated) = await client.PutHttpContent("/api/blogs/12", new BlogUpdateCmd("POS Blog Updated"));
        Assert.Equal(HttpStatusCode.OK, updateStatus.StatusCode);
        Assert.Equal("POS Blog Updated", updated.GetProperty("name").GetString());

        var (deleteStatus, _) = await client.DeleteHttpContent("/api/blogs/12");
        Assert.Equal(HttpStatusCode.NoContent, deleteStatus.StatusCode);

        await _factory.BlogServiceMock.Received(1).CreateAsync(Arg.Is<BlogCreateCmd>(x => x.Name == "POS Blog"));
        await _factory.BlogServiceMock.Received(1).UpdateAsync(12, Arg.Is<BlogUpdateCmd>(x => x.Name == "POS Blog Updated"));
        await _factory.BlogServiceMock.Received(1).DeleteAsync(12);
    }

    [Fact]
    public async Task Get_Paged_UsesMockedService()
    {
        var page = new PagedResultDto<BlogDto>(
            2,
            1,
            3,
            [new BlogDto(2, "Beta", 0)]);

        _factory.BlogServiceMock.GetPagedAsync(2, 1, null).Returns(Task.FromResult(page));

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<PagedResultDto<BlogDto>>("/api/blogs/paged?page=2&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Beta", result.Items[0].Name);
        await _factory.BlogServiceMock.Received(1).GetPagedAsync(2, 1, null);
    }

    [Fact]
    public async Task Get_Paged_ReturnsAllMockedItemsAcrossPages()
    {
        _factory.BlogServiceMock.GetPagedAsync(1, 1, null)
            .Returns(Task.FromResult(new PagedResultDto<BlogDto>(1, 1, 3, [new BlogDto(1, "Alpha", 0)])));
        _factory.BlogServiceMock.GetPagedAsync(2, 1, null)
            .Returns(Task.FromResult(new PagedResultDto<BlogDto>(2, 1, 3, [new BlogDto(2, "Beta", 0)])));
        _factory.BlogServiceMock.GetPagedAsync(3, 1, null)
            .Returns(Task.FromResult(new PagedResultDto<BlogDto>(3, 1, 3, [new BlogDto(3, "Gamma", 0)])));

        using HttpClient client = _factory.Client;
        var (_, page1) = await client.GetHttpContent<PagedResultDto<BlogDto>>("/api/blogs/paged?page=1&pageSize=1");
        var (_, page2) = await client.GetHttpContent<PagedResultDto<BlogDto>>("/api/blogs/paged?page=2&pageSize=1");
        var (_, page3) = await client.GetHttpContent<PagedResultDto<BlogDto>>("/api/blogs/paged?page=3&pageSize=1");

        Assert.NotNull(page1);
        Assert.NotNull(page2);
        Assert.NotNull(page3);

        var names = page1.Items
            .Concat(page2.Items)
            .Concat(page3.Items)
            .Select(x => x.Name)
            .ToList();

        Assert.Equal(["Alpha", "Beta", "Gamma"], names);
        await _factory.BlogServiceMock.Received(1).GetPagedAsync(1, 1, null);
        await _factory.BlogServiceMock.Received(1).GetPagedAsync(2, 1, null);
        await _factory.BlogServiceMock.Received(1).GetPagedAsync(3, 1, null);
    }

    [Fact]
    public async Task Get_Stats_UsesMockedService()
    {
        List<BlogStatsDto> stats =
        [
            new(1, ".NET Blog", 4, 3, new DateTime(2024, 1, 5), ["Entity Framework", ".NET"])
        ];

        _factory.BlogServiceMock
            .GetStatisticsAsync(4, "Entity Framework")
            .Returns(Task.FromResult(stats));

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<List<BlogStatsDto>>("/api/blogs/stats?minPosts=4&tag=Entity%20Framework");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(4, result[0].PostCount);
        Assert.Contains("Entity Framework", result[0].Tags);
        await _factory.BlogServiceMock.Received(1).GetStatisticsAsync(4, "Entity Framework");
    }

    [Fact]
    public async Task Get_RecentPosts_UsesMockedService()
    {
        List<PostDto> posts =
        [
            new(5, "LINQ Post", new DateTime(2024, 2, 1), ".NET Blog", "Mock Author", ["Entity Framework"])
        ];

        _factory.BlogServiceMock
            .GetRecentPostsAsync(null, "Entity Framework", 3)
            .Returns(Task.FromResult(posts));

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<List<PostDto>>("/api/blogs/recent-posts?tag=Entity%20Framework&take=3");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("Entity Framework", result[0].Tags);
        await _factory.BlogServiceMock.Received(1).GetRecentPostsAsync(null, "Entity Framework", 3);
    }

    [Fact]
    public async Task Get_AuthorsByCountry_UsesMockedService()
    {
        List<AuthorActivityDto> authors =
        [
            new(4, "Mock Author", "UK", 2, [".NET Blog", "POS Blog"])
        ];

        _factory.BlogServiceMock
            .GetAuthorsByCountryAsync("UK", 1)
            .Returns(Task.FromResult(authors));

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<List<AuthorActivityDto>>("/api/blogs/authors/by-country?country=UK&minPosts=1");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("UK", result[0].Country);
        Assert.Equal(2, result[0].PostCount);
        await _factory.BlogServiceMock.Received(1).GetAuthorsByCountryAsync("UK", 1);
    }
}
