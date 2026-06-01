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

    [Fact]
    public async Task GetBlogs_ReturnsAllBlogs()
    {
        _factory.InitializeDatabase(db =>
        {
            db.Blogs.Add(new Blog("NET Blog"));
            db.Blogs.Add(new Blog("Java Blog"));
        });

        using HttpClient client = _factory.Client;

        var (status, result) =
            await client.GetHttpContent<List<BlogDto>>("/api/blogs");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetBlogs_WithSearch_ReturnsFilteredBlogs()
    {
        _factory.InitializeDatabase(db =>
        {
            db.Blogs.Add(new Blog("NET Blog"));
            db.Blogs.Add(new Blog("Java Blog"));
        });

        using HttpClient client = _factory.Client;

        var (status, result) =
            await client.GetHttpContent<List<BlogDto>>("/api/blogs?search=NET");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains("NET", result[0].Name);
    }

    [Fact]
    public async Task GetPaged_ReturnsCorrectPage()
    {
        _factory.InitializeDatabase(db =>
        {
            db.Blogs.Add(new Blog("Blog 1"));
            db.Blogs.Add(new Blog("Blog 2"));
            db.Blogs.Add(new Blog("Blog 3"));
        });

        using HttpClient client = _factory.Client;

        var (status, result) =
            await client.GetHttpContent<PagedBlogDto>(
                "/api/blogs/paged?page=1&pageSize=2");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Blogs.Count());
    }

    [Fact]
    public async Task GetPaged_InvalidPageSize_ReturnsBadRequest()
    {
        using HttpClient client = _factory.Client;

        var response =
            await client.GetAsync("/api/blogs/paged?page=1&pageSize=100");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostBlog_ValidBlog_CreatesBlog()
    {
        _factory.InitializeDatabase(db => { });

        using HttpClient client = _factory.Client;

        var dto = new CreateBlogDto("New Blog");

        var (status, result) =
            await client.PostHttpContent("/api/blogs", dto);

        Assert.Equal(HttpStatusCode.Created, status.StatusCode);

        int count = _factory.QueryDatabase(db => db.Blogs.Count());

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task PostBlog_EmptyTitle_ReturnsBadRequest()
    {
        _factory.InitializeDatabase(db => { });

        using HttpClient client = _factory.Client;

        var dto = new CreateBlogDto("");

        var (status, result) =
            await client.PostHttpContent("/api/blogs", dto);

        Assert.Equal(HttpStatusCode.BadRequest, status.StatusCode);
    }

    [Fact]
    public async Task DeleteBlog_ExistingBlog_RemovesBlog()
    {
        int id = 0;

        _factory.InitializeDatabase(db =>
        {
            Blog blog = new("Delete Me");
            db.Blogs.Add(blog);
            db.SaveChanges();

            id = blog.Id;
        });

        using HttpClient client = _factory.Client;

        var (status, result) =
            await client.DeleteHttpContent($"/api/blogs/{id}");

        Assert.Equal(HttpStatusCode.OK, status.StatusCode);

        int count = _factory.QueryDatabase(db => db.Blogs.Count());

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task AuthorsByCountry_WithoutCountry_ReturnsBadRequest()
    {
        using HttpClient client = _factory.Client;

        var response =
            await client.GetAsync("/api/authors/by-country");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}