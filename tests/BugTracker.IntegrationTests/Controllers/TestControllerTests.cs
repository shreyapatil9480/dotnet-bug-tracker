using System.Net;
using System.Net.Http.Json;
using BugTracker.API.DTOs;
using FluentAssertions;
using Xunit;

namespace BugTracker.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for test-only reset endpoint used by BDD suites.
/// </summary>
public class TestControllerTests
{
    [Fact]
    public async Task ResetDatabase_InTestEnvironment_Returns204AndClearsData()
    {
        await using var factory = new TestEnvironmentWebApplicationFactory();
        var client = factory.CreateClient();

        var createResponse = await client.PostAsJsonAsync("/api/projects", new CreateProjectRequest
        {
            Name = "Reset Test Project",
            Description = "Should be cleared by reset"
        });
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var resetResponse = await client.DeleteAsync("/api/test/reset");

        resetResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var listResponse = await client.GetAsync("/api/projects");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var projects = await listResponse.Content.ReadFromJsonAsync<List<ProjectResponse>>();
        projects.Should().NotBeNull();
        projects!.Should().BeEmpty();
    }

    [Fact]
    public async Task ResetDatabase_OutsideTestEnvironment_Returns404()
    {
        await using var factory = new CustomWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.DeleteAsync("/api/test/reset");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
