using System.Net;
using System.Net.Http.Json;
using BugTracker.API.DTOs;
using BugTracker.Core.Enums;
using Xunit;

namespace BugTracker.IntegrationTests.Controllers;

/// <summary>
/// Integration tests for BugsController endpoints.
/// Tests full HTTP request/response cycle with in-memory SQLite.
/// </summary>
public class BugsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BugsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    #region GET /api/bugs/{id}

    [Fact]
    public async Task GetBug_ExistingId_Returns200OK()
    {
        // Arrange - create project and bug
        var projectRequest = new CreateProjectRequest { Name = "Project for Bug Get" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest
        {
            Title = "Bug to Get",
            Description = "Test Description",
            Severity = BugSeverity.Critical
        };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var createdBug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();

        // Act
        var response = await _client.GetAsync($"/api/bugs/{createdBug!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bug = await response.Content.ReadFromJsonAsync<BugResponse>();
        Assert.NotNull(bug);
        Assert.Equal(bugRequest.Title, bug.Title);
        Assert.Equal(BugSeverity.Critical, bug.Severity);
        Assert.Equal(BugStatus.Open, bug.Status);
    }

    [Fact]
    public async Task GetBug_NonExistentId_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/bugs/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region PUT /api/bugs/{id}

    [Fact]
    public async Task UpdateBug_ValidStatusTransition_Returns200OK()
    {
        // Arrange - create project and bug
        var projectRequest = new CreateProjectRequest { Name = "Project for Bug Update" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest
        {
            Title = "Bug to Update",
            Severity = BugSeverity.Medium
        };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var createdBug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();

        var updateRequest = new UpdateBugRequest { Status = BugStatus.InProgress };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/bugs/{createdBug!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bug = await response.Content.ReadFromJsonAsync<BugResponse>();
        Assert.NotNull(bug);
        Assert.Equal(BugStatus.InProgress, bug.Status);
    }

    [Fact]
    public async Task UpdateBug_InvalidStatusTransition_Returns400BadRequest()
    {
        // Arrange - create project and bug (status = Open)
        var projectRequest = new CreateProjectRequest { Name = "Project for Invalid Transition" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest
        {
            Title = "Bug for Invalid Transition",
            Severity = BugSeverity.High
        };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var createdBug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();

        // Try to skip from Open directly to Resolved (invalid)
        var updateRequest = new UpdateBugRequest { Status = BugStatus.Resolved };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/bugs/{createdBug!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBug_ClosedToOpen_Returns400BadRequest()
    {
        // Arrange - create and progress bug through full lifecycle
        var projectRequest = new CreateProjectRequest { Name = "Project for Closed Bug" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest { Title = "Bug to Close", Severity = BugSeverity.Low };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var createdBug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();

        // Progress through statuses: Open -> InProgress -> Resolved -> Closed
        await _client.PutAsJsonAsync($"/api/bugs/{createdBug!.Id}", new UpdateBugRequest { Status = BugStatus.InProgress });
        await _client.PutAsJsonAsync($"/api/bugs/{createdBug.Id}", new UpdateBugRequest { Status = BugStatus.Resolved });
        await _client.PutAsJsonAsync($"/api/bugs/{createdBug.Id}", new UpdateBugRequest { Status = BugStatus.Closed });

        // Try to transition back to Open (invalid)
        var updateRequest = new UpdateBugRequest { Status = BugStatus.Open };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/bugs/{createdBug.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBug_NonExistentId_Returns404NotFound()
    {
        // Arrange
        var updateRequest = new UpdateBugRequest { Title = "Updated Title" };

        // Act
        var response = await _client.PutAsJsonAsync("/api/bugs/99999", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBug_UpdateTitle_Returns200OK()
    {
        // Arrange
        var projectRequest = new CreateProjectRequest { Name = "Project for Title Update" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest { Title = "Original Title", Severity = BugSeverity.Medium };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var createdBug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();

        var updateRequest = new UpdateBugRequest { Title = "Updated Title" };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/bugs/{createdBug!.Id}", updateRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var bug = await response.Content.ReadFromJsonAsync<BugResponse>();
        Assert.Equal("Updated Title", bug!.Title);
    }

    #endregion

    #region DELETE /api/bugs/{id}

    [Fact]
    public async Task DeleteBug_ExistingId_Returns204NoContent()
    {
        // Arrange
        var projectRequest = new CreateProjectRequest { Name = "Project for Bug Delete" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest { Title = "Bug to Delete", Severity = BugSeverity.Low };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var createdBug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();

        // Act
        var response = await _client.DeleteAsync($"/api/bugs/{createdBug!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        // Verify bug is deleted
        var getResponse = await _client.GetAsync($"/api/bugs/{createdBug.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteBug_NonExistentId_Returns404NotFound()
    {
        // Act
        var response = await _client.DeleteAsync("/api/bugs/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region POST /api/bugs/{id}/comments

    [Fact]
    public async Task AddComment_OpenBug_Returns201Created()
    {
        // Arrange
        var projectRequest = new CreateProjectRequest { Name = "Project for Comment" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest { Title = "Bug for Comment", Severity = BugSeverity.Medium };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var createdBug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();

        var commentRequest = new CreateCommentRequest { Text = "This is a test comment" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/bugs/{createdBug!.Id}/comments", commentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var comment = await response.Content.ReadFromJsonAsync<CommentResponse>();
        Assert.NotNull(comment);
        Assert.Equal(commentRequest.Text, comment.Text);
    }

    [Fact]
    public async Task AddComment_ClosedBug_Returns400BadRequest()
    {
        // Arrange - create and close bug
        var projectRequest = new CreateProjectRequest { Name = "Project for Closed Bug Comment" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest { Title = "Bug to Close", Severity = BugSeverity.Low };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var createdBug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();

        // Close the bug
        await _client.PutAsJsonAsync($"/api/bugs/{createdBug!.Id}", new UpdateBugRequest { Status = BugStatus.InProgress });
        await _client.PutAsJsonAsync($"/api/bugs/{createdBug.Id}", new UpdateBugRequest { Status = BugStatus.Resolved });
        await _client.PutAsJsonAsync($"/api/bugs/{createdBug.Id}", new UpdateBugRequest { Status = BugStatus.Closed });

        var commentRequest = new CreateCommentRequest { Text = "Comment on closed bug" };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/bugs/{createdBug.Id}/comments", commentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AddComment_NonExistentBug_Returns404NotFound()
    {
        // Arrange
        var commentRequest = new CreateCommentRequest { Text = "Comment for non-existent bug" };

        // Act
        var response = await _client.PostAsJsonAsync("/api/bugs/99999/comments", commentRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region GET /api/bugs/{id}/comments

    [Fact]
    public async Task GetComments_ReturnsListOfComments()
    {
        // Arrange
        var projectRequest = new CreateProjectRequest { Name = "Project for Comment List" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest { Title = "Bug with Comments", Severity = BugSeverity.Medium };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var createdBug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();

        await _client.PostAsJsonAsync($"/api/bugs/{createdBug!.Id}/comments", new CreateCommentRequest { Text = "Comment 1" });
        await _client.PostAsJsonAsync($"/api/bugs/{createdBug.Id}/comments", new CreateCommentRequest { Text = "Comment 2" });

        // Act
        var response = await _client.GetAsync($"/api/bugs/{createdBug.Id}/comments");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var comments = await response.Content.ReadFromJsonAsync<List<CommentResponse>>();
        Assert.NotNull(comments);
        Assert.Equal(2, comments.Count);
    }

    [Fact]
    public async Task GetComments_NonExistentBug_Returns404NotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/bugs/99999/comments");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Full Status Lifecycle Test

    [Fact]
    public async Task BugStatusLifecycle_CompleteFlow_Succeeds()
    {
        // Arrange - create project and bug
        var projectRequest = new CreateProjectRequest { Name = "Project for Lifecycle Test" };
        var projectResponse = await _client.PostAsJsonAsync("/api/projects", projectRequest);
        var project = await projectResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var bugRequest = new CreateBugRequest
        {
            Title = "Lifecycle Bug",
            Description = "Testing complete lifecycle",
            Severity = BugSeverity.Critical
        };
        var bugCreateResponse = await _client.PostAsJsonAsync($"/api/projects/{project!.Id}/bugs", bugRequest);
        var bug = await bugCreateResponse.Content.ReadFromJsonAsync<BugResponse>();
        Assert.Equal(BugStatus.Open, bug!.Status);

        // Open -> InProgress
        var response1 = await _client.PutAsJsonAsync($"/api/bugs/{bug.Id}", new UpdateBugRequest { Status = BugStatus.InProgress });
        Assert.Equal(HttpStatusCode.OK, response1.StatusCode);
        var bug1 = await response1.Content.ReadFromJsonAsync<BugResponse>();
        Assert.Equal(BugStatus.InProgress, bug1!.Status);

        // InProgress -> Resolved
        var response2 = await _client.PutAsJsonAsync($"/api/bugs/{bug.Id}", new UpdateBugRequest { Status = BugStatus.Resolved });
        Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        var bug2 = await response2.Content.ReadFromJsonAsync<BugResponse>();
        Assert.Equal(BugStatus.Resolved, bug2!.Status);

        // Resolved -> Closed
        var response3 = await _client.PutAsJsonAsync($"/api/bugs/{bug.Id}", new UpdateBugRequest { Status = BugStatus.Closed });
        Assert.Equal(HttpStatusCode.OK, response3.StatusCode);
        var bug3 = await response3.Content.ReadFromJsonAsync<BugResponse>();
        Assert.Equal(BugStatus.Closed, bug3!.Status);
    }

    #endregion
}
