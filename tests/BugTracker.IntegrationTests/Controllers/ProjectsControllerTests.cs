using System.Net;
using System.Net.Http.Json;
using BugTracker.API.DTOs;
using BugTracker.Core.Enums;
using FluentAssertions;
using Xunit;

namespace BugTracker.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for ProjectsController endpoints.
/// Tests full HTTP request/response cycle with in-memory SQLite.
/// </summary>
public class ProjectsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProjectsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region POST /api/projects

    [Fact]
    public async Task CreateProject_ValidPayload_Returns201CreatedWithLocationHeader()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Integration Test Project",
            Description = "Created during integration test"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        project.Should().NotBeNull();
        project!.Name.Should().Be(request.Name);
        project.Description.Should().Be(request.Description);
        project.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task CreateProject_MissingName_Returns400BadRequest()
    {
        // Arrange
        var request = new { Description = "No name provided" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProject_EmptyBody_Returns400BadRequest()
    {
        // Arrange
        var request = new { };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GET /api/projects

    [Fact]
    public async Task GetProjects_ReturnsListOfProjects()
    {
        // Arrange - create a project first
        var createRequest = new CreateProjectRequest
        {
            Name = "Project for List Test",
            Description = "Test"
        };
        await _client.PostAsJsonAsync("/api/projects", createRequest);

        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var projects = await response.Content.ReadFromJsonAsync<List<ProjectResponse>>();
        projects.Should().NotBeNull();
        projects.Should().NotBeEmpty();
    }

    #endregion

    #region GET /api/projects/{id}

    [Fact]
    public async Task GetProject_ExistingId_Returns200OK()
    {
        // Arrange - create a project first
        var createRequest = new CreateProjectRequest
        {
            Name = "Project for Get Test",
            Description = "Test Description"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/projects", createRequest);
        var createdProject = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        // Act
        var response = await _client.GetAsync($"/api/projects/{createdProject!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var project = await response.Content.ReadFromJsonAsync<ProjectResponse>();
        project.Should().NotBeNull();
        project!.Name.Should().Be(createRequest.Name);
    }

    [Fact]
    public async Task GetProject_NonExistentId_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/projects/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/projects/{id} - Cascade Delete

    [Fact]
    public async Task DeleteProject_CascadesDeleteToBugsAndComments()
    {
        // Arrange - create project, bug, and comment
        var projectRequest = new CreateProjectRequest { Name = "Project to Delete" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest
        {
            Title = "Bug to be deleted",
            Description = "Will be cascade deleted",
            Severity = BugSeverity.High
        };
        var bugResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var bug = await bugResponse.Content.ReadFromJsonAsync<BugResponse>();

        var commentRequest = new CreateCommentRequest { Text = "Comment to be deleted" };
        await _client.PostAsJsonAsync($"/api/bugs/{bug!.Id}/comments", commentRequest);

        // Act - delete the project
        var deleteResponse = await _client.DeleteAsync($"/api/projects/{project.Id}");

        // Assert - project deleted
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify project is gone
        var getProjectResponse = await _client.GetAsync($"/api/projects/{project.Id}");
        getProjectResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Verify bug is gone (cascade delete)
        var getBugResponse = await _client.GetAsync($"/api/bugs/{bug.Id}");
        getBugResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProject_NonExistentId_Returns404NotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/projects/99999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/projects/{id}/bugs

    [Fact]
    public async Task CreateBug_ValidBug_Returns201Created()
    {
        // Arrange - create project first
        var projectRequest = new CreateProjectRequest { Name = "Project for Bug" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest
        {
            Title = "Test Bug",
            Description = "Test Description",
            Severity = BugSeverity.High
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var bug = await response.Content.ReadFromJsonAsync<BugResponse>();
        bug.Should().NotBeNull();
        bug!.Title.Should().Be(bugRequest.Title);
        bug.Status.Should().Be(BugStatus.Open);
    }

    [Fact]
    public async Task CreateBug_InvalidSeverity_Returns400BadRequest()
    {
        // Arrange - create project first
        var projectRequest = new CreateProjectRequest { Name = "Project for Invalid Bug" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        // Using an invalid severity value via anonymous object
        var bugRequest = new { Title = "Test Bug", Severity = 99 };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateBug_NonExistentProject_Returns404NotFound()
    {
        // Arrange
        var bugRequest = new CreateBugRequest
        {
            Title = "Test Bug",
            Severity = BugSeverity.Medium
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/projects/99999/bugs", bugRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region GET /api/projects/{id}/bugs

    [Fact]
    public async Task GetBugs_ReturnsListOfBugsForProject()
    {
        // Arrange - create project and bugs
        var projectRequest = new CreateProjectRequest { Name = "Project with Bugs" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest1 = new CreateBugRequest { Title = "Bug 1", Severity = BugSeverity.Low };
        var bugRequest2 = new CreateBugRequest { Title = "Bug 2", Severity = BugSeverity.High };
        await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest1);
        await _client.PostAsJsonAsync($"/api/projects/{project.Id}/bugs", bugRequest2);

        // Act
        var response = await _client.GetAsync($"/api/projects/{project.Id}/bugs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var bugs = await response.Content.ReadFromJsonAsync<List<BugResponse>>();
        bugs.Should().NotBeNull();
        bugs.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBugs_NonExistentProject_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/projects/99999/bugs");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
