using BugTracker.API.DTOs;
using BugTracker.API.Services;
using BugTracker.Core.Entities;
using BugTracker.Core.Enums;
using BugTracker.Core.Exceptions;
using BugTracker.Core.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace BugTracker.UnitTests.Services;

/// <summary>
/// Unit tests for BugService covering all bug-related business rules.
/// </summary>
public class BugServiceTests
{
    private readonly Mock<IBugRepository> _mockBugRepository;
    private readonly Mock<IProjectRepository> _mockProjectRepository;
    private readonly BugService _bugService;

    public BugServiceTests()
    {
        _mockBugRepository = new Mock<IBugRepository>();
        _mockProjectRepository = new Mock<IProjectRepository>();
        _bugService = new BugService(_mockBugRepository.Object, _mockProjectRepository.Object);
    }

    #region CreateBug Tests

    [Fact]
    public async Task CreateBug_ValidInput_ReturnsBugWithId()
    {
        // Arrange
        var projectId = 1;
        var request = new CreateBugRequest
        {
            Title = "Test Bug",
            Description = "Test Description",
            Severity = BugSeverity.High
        };

        _mockProjectRepository.Setup(r => r.ExistsAsync(projectId)).ReturnsAsync(true);
        _mockBugRepository.Setup(r => r.AddAsync(It.IsAny<Bug>()))
            .ReturnsAsync((Bug b) =>
            {
                b.Id = 1;
                return b;
            });

        // Act
        var result = await _bugService.CreateBugAsync(projectId, request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Title.Should().Be(request.Title);
        result.Severity.Should().Be(request.Severity);
        result.Status.Should().Be(BugStatus.Open);
        _mockBugRepository.Verify(r => r.AddAsync(It.IsAny<Bug>()), Times.Once);
    }

    [Fact]
    public async Task CreateBug_ProjectNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var projectId = 999;
        var request = new CreateBugRequest
        {
            Title = "Test Bug",
            Severity = BugSeverity.Medium
        };

        _mockProjectRepository.Setup(r => r.ExistsAsync(projectId)).ReturnsAsync(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _bugService.CreateBugAsync(projectId, request));
        exception.Message.Should().Contain("Project");
    }

    [Fact]
    public async Task CreateBug_EmptyTitle_ThrowsValidationException()
    {
        // Arrange
        var projectId = 1;
        var request = new CreateBugRequest
        {
            Title = "   ",
            Severity = BugSeverity.Medium
        };

        _mockProjectRepository.Setup(r => r.ExistsAsync(projectId)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bugService.CreateBugAsync(projectId, request));
        exception.Message.Should().Contain("Title");
    }

    [Fact]
    public async Task CreateBug_InvalidSeverity_ThrowsValidationException()
    {
        // Arrange
        var projectId = 1;
        var request = new CreateBugRequest
        {
            Title = "Test Bug",
            Severity = (BugSeverity)99
        };

        _mockProjectRepository.Setup(r => r.ExistsAsync(projectId)).ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _bugService.CreateBugAsync(projectId, request));
        exception.Message.Should().Contain("Severity");
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public async Task UpdateStatus_OpenToInProgress_Succeeds()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Open,
            Comments = new List<Comment>()
        };
        var request = new UpdateBugRequest { Status = BugStatus.InProgress };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);
        _mockBugRepository.Setup(r => r.UpdateAsync(It.IsAny<Bug>())).Returns(Task.CompletedTask);

        // Act
        var result = await _bugService.UpdateBugAsync(1, request);

        // Assert
        result.Status.Should().Be(BugStatus.InProgress);
        _mockBugRepository.Verify(r => r.UpdateAsync(It.Is<Bug>(b => b.Status == BugStatus.InProgress)), Times.Once);
    }

    [Fact]
    public async Task UpdateStatus_InProgressToResolved_Succeeds()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.InProgress,
            Comments = new List<Comment>()
        };
        var request = new UpdateBugRequest { Status = BugStatus.Resolved };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);
        _mockBugRepository.Setup(r => r.UpdateAsync(It.IsAny<Bug>())).Returns(Task.CompletedTask);

        // Act
        var result = await _bugService.UpdateBugAsync(1, request);

        // Assert
        result.Status.Should().Be(BugStatus.Resolved);
    }

    [Fact]
    public async Task UpdateStatus_ResolvedToClosed_Succeeds()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Resolved,
            Comments = new List<Comment>()
        };
        var request = new UpdateBugRequest { Status = BugStatus.Closed };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);
        _mockBugRepository.Setup(r => r.UpdateAsync(It.IsAny<Bug>())).Returns(Task.CompletedTask);

        // Act
        var result = await _bugService.UpdateBugAsync(1, request);

        // Assert
        result.Status.Should().Be(BugStatus.Closed);
    }

    [Fact]
    public async Task UpdateStatus_ClosedToOpen_ThrowsInvalidStatusTransitionException()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Closed,
            Comments = new List<Comment>()
        };
        var request = new UpdateBugRequest { Status = BugStatus.Open };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => _bugService.UpdateBugAsync(1, request));
        exception.CurrentStatus.Should().Be(BugStatus.Closed);
        exception.AttemptedStatus.Should().Be(BugStatus.Open);
    }

    [Fact]
    public async Task UpdateStatus_OpenToResolved_ThrowsInvalidStatusTransitionException()
    {
        // Arrange - cannot skip InProgress
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Open,
            Comments = new List<Comment>()
        };
        var request = new UpdateBugRequest { Status = BugStatus.Resolved };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => _bugService.UpdateBugAsync(1, request));
        exception.Message.Should().Contain("Open");
        exception.Message.Should().Contain("Resolved");
    }

    [Fact]
    public async Task UpdateStatus_InProgressToOpen_ThrowsInvalidStatusTransitionException()
    {
        // Arrange - no backwards transitions
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.InProgress,
            Comments = new List<Comment>()
        };
        var request = new UpdateBugRequest { Status = BugStatus.Open };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => _bugService.UpdateBugAsync(1, request));
    }

    [Fact]
    public async Task UpdateStatus_OpenToClosed_ThrowsInvalidStatusTransitionException()
    {
        // Arrange - cannot skip steps
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Open,
            Comments = new List<Comment>()
        };
        var request = new UpdateBugRequest { Status = BugStatus.Closed };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => _bugService.UpdateBugAsync(1, request));
    }

    [Fact]
    public async Task UpdateStatus_SameStatus_Succeeds()
    {
        // Arrange - updating to same status should succeed
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Open,
            Comments = new List<Comment>()
        };
        var request = new UpdateBugRequest { Status = BugStatus.Open };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);
        _mockBugRepository.Setup(r => r.UpdateAsync(It.IsAny<Bug>())).Returns(Task.CompletedTask);

        // Act
        var result = await _bugService.UpdateBugAsync(1, request);

        // Assert
        result.Status.Should().Be(BugStatus.Open);
    }

    #endregion

    #region GetBug Tests

    [Fact]
    public async Task GetBugById_ExistingId_ReturnsBug()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Description = "Test Description",
            Severity = BugSeverity.High,
            Status = BugStatus.Open,
            Comments = new List<Comment>()
        };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);

        // Act
        var result = await _bugService.GetBugByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be(bug.Title);
        result.Severity.Should().Be(bug.Severity);
    }

    [Fact]
    public async Task GetBugById_NonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        _mockBugRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Bug?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _bugService.GetBugByIdAsync(999));
        exception.Message.Should().Contain("Bug");
    }

    #endregion

    #region DeleteBug Tests

    [Fact]
    public async Task DeleteBug_ExistingId_CallsRepositoryDelete()
    {
        // Arrange
        _mockBugRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockBugRepository.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        await _bugService.DeleteBugAsync(1);

        // Assert
        _mockBugRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteBug_NonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        _mockBugRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _bugService.DeleteBugAsync(999));
    }

    #endregion

    #region GetBugsByProjectId Tests

    [Fact]
    public async Task GetBugsByProjectId_ValidProject_ReturnsBugs()
    {
        // Arrange
        var bugs = new List<Bug>
        {
            new Bug { Id = 1, ProjectId = 1, Title = "Bug 1", Status = BugStatus.Open, Comments = new List<Comment>() },
            new Bug { Id = 2, ProjectId = 1, Title = "Bug 2", Status = BugStatus.InProgress, Comments = new List<Comment>() }
        };

        _mockProjectRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockBugRepository.Setup(r => r.GetByProjectIdAsync(1)).ReturnsAsync(bugs);

        // Act
        var result = await _bugService.GetBugsByProjectIdAsync(1);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetBugsByProjectId_NonExistentProject_ThrowsNotFoundException()
    {
        // Arrange
        _mockProjectRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _bugService.GetBugsByProjectIdAsync(999));
    }

    #endregion
}
