using BugTracker.API.DTOs;
using BugTracker.API.Services;
using BugTracker.Core.Entities;
using BugTracker.Core.Exceptions;
using BugTracker.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugTracker.UnitTests.Services;

/// <summary>
/// Unit tests for ProjectService covering project-related business rules.
/// </summary>
public class ProjectServiceTests
{
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly Mock<IBugRepository> _mockBugRepository;
    private readonly Mock<ICommentRepository> _mockCommentRepository;
    private readonly ProjectService _projectService;

    public ProjectServiceTests()
    {
        _mockProjectRepository = new Mock<IProjectRepository>();
        _mockBugRepository = new Mock<IBugRepository>();
        _mockCommentRepository = new Mock<ICommentRepository>();
        _projectService = new ProjectService(
            _mockProjectRepository.Object,
            _mockBugRepository.Object,
            _mockCommentRepository.Object);
    }

    #region CreateProject Tests

    [Fact]
    public async Task CreateProject_ValidName_ReturnsProjectWithId()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "Test Project",
            Description = "Test Description"
        };

        _mockProjectRepository.Setup(r => r.AddAsync(It.IsAny<Project>()))
            .ReturnsAsync((Project p) =>
            {
                p.Id = 1;
                return p;
            });

        // Act
        var result = await _projectService.CreateProjectAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(request.Name);
        result.Description.Should().Be(request.Description);
        _mockProjectRepository.Verify(r => r.AddAsync(It.IsAny<Project>()), Times.Once);
    }

    [Fact]
    public async Task CreateProject_EmptyName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "   ",
            Description = "Test Description"
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _projectService.CreateProjectAsync(request));
        exception.Message.ToLower().Should().Contain("name");
    }

    [Fact]
    public async Task CreateProject_NullName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = null!,
            Description = "Test Description"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _projectService.CreateProjectAsync(request));
    }

    [Fact]
    public async Task CreateProject_TrimsWhitespace()
    {
        // Arrange
        var request = new CreateProjectRequest
        {
            Name = "  Test Project  ",
            Description = "  Test Description  "
        };

        Project? capturedProject = null;
        _mockProjectRepository.Setup(r => r.AddAsync(It.IsAny<Project>()))
            .Callback<Project>(p => capturedProject = p)
            .ReturnsAsync((Project p) =>
            {
                p.Id = 1;
                return p;
            });

        // Act
        await _projectService.CreateProjectAsync(request);

        // Assert
        capturedProject.Should().NotBeNull();
        capturedProject!.Name.Should().Be("Test Project");
        capturedProject.Description.Should().Be("Test Description");
    }

    #endregion

    #region GetAllProjects Tests

    [Fact]
    public async Task GetAllProjects_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        _mockProjectRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<Project>());

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllProjects_WithProjects_ReturnsAllProjects()
    {
        // Arrange
        var projects = new List<Project>
        {
            new Project { Id = 1, Name = "Project 1", Bugs = new List<Bug>() },
            new Project { Id = 2, Name = "Project 2", Bugs = new List<Bug>() }
        };

        _mockProjectRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(projects);

        // Act
        var result = await _projectService.GetAllProjectsAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetProjectById Tests

    [Fact]
    public async Task GetProjectById_ExistingId_ReturnsProject()
    {
        // Arrange
        var project = new Project
        {
            Id = 1,
            Name = "Test Project",
            Description = "Description",
            Bugs = new List<Bug>()
        };

        _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

        // Act
        var result = await _projectService.GetProjectByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(project.Name);
    }

    [Fact]
    public async Task GetProjectById_NonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        _mockProjectRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Project?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _projectService.GetProjectByIdAsync(999));
        exception.Message.Should().Contain("Project");
    }

    #endregion

    #region DeleteProject Tests

    [Fact]
    public async Task DeleteProject_ExistingId_CascadesDeleteToBugsAndComments()
    {
        // Arrange
        _mockProjectRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockProjectRepository.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        await _projectService.DeleteProjectAsync(1);

        // Assert
        _mockProjectRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteProject_NonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        _mockProjectRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _projectService.DeleteProjectAsync(999));
    }

    #endregion

    #region BugCount Tests

    [Fact]
    public async Task GetProjectById_WithBugs_ReturnsBugCount()
    {
        // Arrange
        var project = new Project
        {
            Id = 1,
            Name = "Test Project",
            Bugs = new List<Bug>
            {
                new Bug { Id = 1, Title = "Bug 1" },
                new Bug { Id = 2, Title = "Bug 2" },
                new Bug { Id = 3, Title = "Bug 3" }
            }
        };

        _mockProjectRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(project);

        // Act
        var result = await _projectService.GetProjectByIdAsync(1);

        // Assert
        result.BugCount.Should().Be(3);
    }

    #endregion
}
