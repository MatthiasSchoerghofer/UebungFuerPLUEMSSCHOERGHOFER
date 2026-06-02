using Aufgabe3_RestfulApi.Dtos;
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
    public async Task GetProjectDashboard_WithFilters_ReturnsMatchingSeedProjects()
    {
        var (response, result) = await _factory.Client
            .GetHttpContent<List<ProjectInfoDto>>("/api/projects/dashboard?status=Active&customerCountry=AT&skill=SQL");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, dto =>
        {
            Assert.Equal("AT", dto.CustomerCountry);
            Assert.Equal("Active", dto.ProjectStatus.ToString());
            Assert.Contains("SQL", dto.Skills);
            Assert.True(dto.UsedBudget > 0);
        });
    }

    [Fact]
    public async Task GetEmployeeWorkload_WithMinHours_ReturnsEmployeesByTotalBookedHours()
    {
        var (response, result) = await _factory.Client
            .GetHttpContent<List<EmployeeWorkloadDto>>("/api/employees/workload?department=Development&country=AT&onlyActive=true&minHours=20");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(["Anna Berger", "Max Steiner"], result.Select(e => e.Name).OrderBy(n => n).ToList());
        Assert.All(result, dto =>
        {
            Assert.Equal("Development", dto.Department);
            Assert.Equal("AT", dto.OfficeCountry);
            Assert.True(dto.BookedHours >= 20);
            Assert.True(dto.AvgHoursPerTask > 0);
        });
    }

    [Fact]
    public async Task GetCriticalTasks_WithMissingSkillsOnly_ReturnsTasksWithRealSkillGap()
    {
        var (response, result) = await _factory.Client
            .GetHttpContent<List<CriticalTaskDto>>("/api/tasks/critical?priority=Critical&missingSkillsOnly=true&take=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(["Import product catalog", "Security review"], result.Select(t => t.Title).OrderBy(t => t).ToList());
        Assert.All(result, dto =>
        {
            Assert.Equal("Critical", dto.Priority.ToString());
            Assert.NotEmpty(dto.MissingSkills);
        });
    }

    [Fact]
    public async Task GetCriticalTasks_WithTake_LimitsResultCount()
    {
        var (response, result) = await _factory.Client
            .GetHttpContent<List<CriticalTaskDto>>("/api/tasks/critical?take=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Single(result);
    }
}
