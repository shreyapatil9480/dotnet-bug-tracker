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
/// Unit tests for CommentService covering comment-related business rules.
/// </summary>
public class CommentServiceTests
{
    private readonly Mock<ICommentRepository> _mockCommentRepository;
    private readonly Mock<IBugRepository> _mockBugRepository;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        _mockCommentRepository = new Mock<ICommentRepository>();
        _mockBugRepository = new Mock<IBugRepository>();
        _commentService = new CommentService(_mockCommentRepository.Object, _mockBugRepository.Object);
    }

    #region AddComment Tests

    [Fact]
    public async Task AddComment_OpenBug_Succeeds()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Open
        };
        var request = new CreateCommentRequest { Text = "Test comment" };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);
        _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) =>
            {
                c.Id = 1;
                return c;
            });

        // Act
        var result = await _commentService.AddCommentAsync(1, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Text, result.Text);
        Assert.Equal(1, result.BugId);
        _mockCommentRepository.Verify(r => r.AddAsync(It.IsAny<Comment>()), Times.Once);
    }

    [Fact]
    public async Task AddComment_InProgressBug_Succeeds()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.InProgress
        };
        var request = new CreateCommentRequest { Text = "Test comment" };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);
        _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) =>
            {
                c.Id = 1;
                return c;
            });

        // Act
        var result = await _commentService.AddCommentAsync(1, request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AddComment_ResolvedBug_Succeeds()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Resolved
        };
        var request = new CreateCommentRequest { Text = "Test comment" };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);
        _mockCommentRepository.Setup(r => r.AddAsync(It.IsAny<Comment>()))
            .ReturnsAsync((Comment c) =>
            {
                c.Id = 1;
                return c;
            });

        // Act
        var result = await _commentService.AddCommentAsync(1, request);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task AddComment_ClosedBug_ThrowsValidationException()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Closed
        };
        var request = new CreateCommentRequest { Text = "Test comment" };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _commentService.AddCommentAsync(1, request));
        Assert.Contains("closed", exception.Message.ToLower());
    }

    [Fact]
    public async Task AddComment_NonExistentBug_ThrowsNotFoundException()
    {
        // Arrange
        var request = new CreateCommentRequest { Text = "Test comment" };
        _mockBugRepository.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Bug?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => _commentService.AddCommentAsync(999, request));
        Assert.Contains("Bug", exception.Message);
    }

    [Fact]
    public async Task AddComment_EmptyText_ThrowsValidationException()
    {
        // Arrange
        var bug = new Bug
        {
            Id = 1,
            ProjectId = 1,
            Title = "Test Bug",
            Status = BugStatus.Open
        };
        var request = new CreateCommentRequest { Text = "   " };

        _mockBugRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(bug);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _commentService.AddCommentAsync(1, request));
        Assert.Contains("text", exception.Message.ToLower());
    }

    #endregion

    #region GetComments Tests

    [Fact]
    public async Task GetComments_ValidBug_ReturnsCommentsOrderedByCreatedAt()
    {
        // Arrange
        var comments = new List<Comment>
        {
            new Comment { Id = 1, BugId = 1, Text = "First comment", CreatedAt = DateTime.UtcNow.AddHours(-2) },
            new Comment { Id = 2, BugId = 1, Text = "Second comment", CreatedAt = DateTime.UtcNow.AddHours(-1) },
            new Comment { Id = 3, BugId = 1, Text = "Third comment", CreatedAt = DateTime.UtcNow }
        };

        _mockBugRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCommentRepository.Setup(r => r.GetByBugIdAsync(1)).ReturnsAsync(comments);

        // Act
        var result = await _commentService.GetCommentsByBugIdAsync(1);

        // Assert
        Assert.Equal(3, result.Count());
        var resultList = result.ToList();
        Assert.Equal("First comment", resultList[0].Text);
        Assert.Equal("Second comment", resultList[1].Text);
        Assert.Equal("Third comment", resultList[2].Text);
    }

    [Fact]
    public async Task GetComments_NonExistentBug_ThrowsNotFoundException()
    {
        // Arrange
        _mockBugRepository.Setup(r => r.ExistsAsync(999)).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _commentService.GetCommentsByBugIdAsync(999));
    }

    [Fact]
    public async Task GetComments_BugWithNoComments_ReturnsEmptyList()
    {
        // Arrange
        _mockBugRepository.Setup(r => r.ExistsAsync(1)).ReturnsAsync(true);
        _mockCommentRepository.Setup(r => r.GetByBugIdAsync(1)).ReturnsAsync(new List<Comment>());

        // Act
        var result = await _commentService.GetCommentsByBugIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    #endregion
}
