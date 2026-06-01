using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Infrastructure;
using Aufgabe3_RestfulApi.Model;
using SPG_Helper;
using System.Net;

namespace Aufgabe3_RestfulApi.Test;

[Collection("Sequential")]
public class Aufgabe3TestsDB : IClassFixture<TestWebApplicationFactoryDB> {
    private readonly TestWebApplicationFactoryDB _factory;

    public Aufgabe3TestsDB(TestWebApplicationFactoryDB factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_ReturnsAll()
    {
        _factory.InitializeDatabase(db =>
        {
            db.Blogs.Add(new Blog("Schule"));
            db.SaveChanges();
        });

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<List<BlogDto>>("/api/blogs");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Schule", result[0].Name);
    }

    [Fact]
    public async Task Post_Put_Delete_UsesBlogServiceAndDatabase()
    {
        _factory.InitializeDatabase(db => { });

        using HttpClient client = _factory.Client;

        var (createStatus, created) = await client.PostHttpContent("/api/blogs", new BlogCreateCmd("POS Blog"));
        Assert.Equal(HttpStatusCode.Created, createStatus.StatusCode);
        int id = created.GetProperty("id").GetInt32();

        var (updateStatus, updated) = await client.PutHttpContent($"/api/blogs/{id}", new BlogUpdateCmd("POS Blog Updated"));
        Assert.Equal(HttpStatusCode.OK, updateStatus.StatusCode);
        Assert.Equal("POS Blog Updated", updated.GetProperty("name").GetString());

        var (getStatus, result) = await client.GetHttpContent<BlogDto>($"/api/blogs/{id}");
        Assert.Equal(HttpStatusCode.OK, getStatus.StatusCode);
        Assert.NotNull(result);
        Assert.Equal("POS Blog Updated", result.Name);

        var (deleteStatus, _) = await client.DeleteHttpContent($"/api/blogs/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteStatus.StatusCode);
        Assert.Equal(0, _factory.QueryDatabase(db => db.Blogs.Count()));
    }

    [Fact]
    public async Task Get_Paged_ReturnsRequestedPage()
    {
        _factory.InitializeDatabase(db =>
        {
            db.Blogs.AddRange(new Blog("Gamma"), new Blog("Alpha"), new Blog("Beta"));
            db.SaveChanges();
        });

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<PagedResultDto<BlogDto>>("/api/blogs/paged?page=2&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Beta", result.Items[0].Name);
    }

    [Fact]
    public async Task Get_Paged_ReturnsAllItemsAcrossPages()
    {
        _factory.InitializeDatabase(db =>
        {
            db.Blogs.AddRange(new Blog("Gamma"), new Blog("Alpha"), new Blog("Beta"));
            db.SaveChanges();
        });

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
        Assert.All([page1, page2, page3], page => Assert.Equal(3, page.TotalCount));
    }

    [Fact]
    public async Task Get_Stats_FiltersByMinPostsAndTag()
    {
        _factory.InitializeDatabase(db => db.Seed());

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<List<BlogStatsDto>>("/api/blogs/stats?minPosts=4&tag=Entity%20Framework");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, x => Assert.True(x.PostCount >= 4));
        Assert.All(result, x => Assert.Contains("Entity Framework", x.Tags));
    }

    [Fact]
    public async Task Get_RecentPosts_FiltersByTagAndLimitsResult()
    {
        _factory.InitializeDatabase(db => db.Seed());

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<List<PostDto>>("/api/blogs/recent-posts?tag=Entity%20Framework&take=3");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.InRange(result.Count, 1, 3);
        Assert.All(result, x => Assert.Contains("Entity Framework", x.Tags));
    }

    [Fact]
    public async Task Get_AuthorsByCountry_FiltersOwnedAddressAndPostCount()
    {
        _factory.InitializeDatabase(db => db.Seed());

        using HttpClient client = _factory.Client;
        var (status, result) = await client.GetHttpContent<List<AuthorActivityDto>>("/api/blogs/authors/by-country?country=UK&minPosts=1");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.All(result, x => Assert.Equal("UK", x.Country));
        Assert.All(result, x => Assert.True(x.PostCount >= 1));
    }
}
