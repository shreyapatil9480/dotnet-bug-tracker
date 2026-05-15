using BugTracker.API.DTOs;
using BugTracker.API.Services;
using BugTracker.Core.Entities;
using BugTracker.Core.Enums;
using BugTracker.Core.Exceptions;
using BugTracker.Core.Interfaces;
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
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(request.Title, result.Title);
        Assert.Equal(request.Severity, result.Severity);
        Assert.Equal(BugStatus.Open, result.Status);
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
        Assert.Contains("Project", exception.Message);
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
        Assert.Contains("Title", exception.Message);
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
        Assert.Contains("Severity", exception.Message);
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
        Assert.Equal(BugStatus.InProgress, result.Status);
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
        Assert.Equal(BugStatus.Resolved, result.Status);
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
        Assert.Equal(BugStatus.Closed, result.Status);
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
        Assert.Equal(BugStatus.Closed, exception.CurrentStatus);
        Assert.Equal(BugStatus.Open, exception.AttemptedStatus);
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
        Assert.Contains("Open", exception.Message);
        Assert.Contains("Resolved", exception.Message);
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
        Assert.Equal(BugStatus.Open, result.Status);
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
        Assert.NotNull(result);
        Assert.Equal(bug.Title, result.Title);
        Assert.Equal(bug.Severity, result.Severity);
    }

    [Fact]
    public async Task GetBugById_NonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        _mockBugRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Bug?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _bugService.GetBugByIdAsync(999));
        Assert.Contains("Bug", exception.Message);
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
        Assert.Equal(2, result.Count());
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
