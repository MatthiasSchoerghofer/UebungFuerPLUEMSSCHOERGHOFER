using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Model;
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
    public async Task GetProjectDashboard_WithValidQuery_ReturnsServiceResult()
    {
        var expected = new List<ProjectInfoDto>
        {
            new(
                "API Relaunch",
                "Demo Customer",
                "AT",
                ProjectStatus.Active,
                new DateTime(2026, 6, 30),
                4,
                2,
                1,
                3,
                1250m,
                ["CSharp", "SQL"])
        };

        _factory.ProjectServiceMock
            .GetProjectInfosAsync("Active", "AT", "CSharp", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var (response, result) = await _factory.Client
            .GetHttpContent<List<ProjectInfoDto>>("/api/projects/dashboard?status=Active&customerCountry=AT&skill=CSharp");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        var dto = Assert.Single(result);
        Assert.Equal("API Relaunch", dto.ProjectName);
        Assert.Equal(1250m, dto.UsedBudget);
        Assert.Contains("CSharp", dto.Skills);

        await _factory.ProjectServiceMock.Received(1)
            .GetProjectInfosAsync("Active", "AT", "CSharp", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetProjectDashboard_WithInvalidStatus_ReturnsBadRequest()
    {
        var (response, _) = await _factory.Client
            .GetHttpContent<List<ProjectInfoDto>>("/api/projects/dashboard?status=Unknown");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await _factory.ProjectServiceMock.DidNotReceive()
            .GetProjectInfosAsync(Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetEmployeeWorkload_WithValidQuery_ReturnsServiceResult()
    {
        var expected = new List<EmployeeWorkloadDto>
        {
            new(
                "Anna Berger",
                "Development",
                "Backend Developer",
                "AT",
                2,
                1,
                27.5m,
                9.166666666666666666666666667m,
                ["CSharp", "Testing"])
        };

        _factory.ProjectServiceMock
            .GetEmployeeWorkloadsAsync("Development", "AT", true, 20, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var (response, result) = await _factory.Client
            .GetHttpContent<List<EmployeeWorkloadDto>>("/api/employees/workload?department=Development&country=AT&onlyActive=true&minHours=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        var dto = Assert.Single(result);
        Assert.Equal("Anna Berger", dto.Name);
        Assert.Equal(27.5m, dto.BookedHours);

        await _factory.ProjectServiceMock.Received(1)
            .GetEmployeeWorkloadsAsync("Development", "AT", true, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCriticalTasks_WithInvalidTake_ReturnsBadRequest()
    {
        var (response, _) = await _factory.Client
            .GetHttpContent<List<CriticalTaskDto>>("/api/tasks/critical?take=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await _factory.ProjectServiceMock.DidNotReceive()
            .GetCriticalTasksAsync(Arg.Any<DateTime?>(), Arg.Any<TaskPriority?>(), Arg.Any<bool?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCriticalTasks_WithValidQuery_ReturnsServiceResult()
    {
        var expected = new List<CriticalTaskDto>
        {
            new(
                "Security review",
                "Mobile Banking Relaunch",
                "Alpine Bank",
                TaskPriority.Critical,
                TaskState.Blocked,
                new DateTime(2026, 5, 20),
                "Lukas Hofer",
                ["ASP.NET Core", "DevOps", "Testing"],
                ["ASP.NET Core"])
        };

        _factory.ProjectServiceMock
            .GetCriticalTasksAsync(new DateTime(2026, 12, 31), TaskPriority.Critical, true, 10, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        var (response, result) = await _factory.Client
            .GetHttpContent<List<CriticalTaskDto>>("/api/tasks/critical?dueBefore=2026-12-31&priority=Critical&missingSkillsOnly=true&take=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        var dto = Assert.Single(result);
        Assert.Equal("Security review", dto.Title);
        Assert.Contains("ASP.NET Core", dto.MissingSkills);

        await _factory.ProjectServiceMock.Received(1)
            .GetCriticalTasksAsync(new DateTime(2026, 12, 31), TaskPriority.Critical, true, 10, Arg.Any<CancellationToken>());
    }
}
