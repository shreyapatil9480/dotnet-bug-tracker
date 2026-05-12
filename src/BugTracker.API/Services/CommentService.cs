using BugTracker.API.DTOs;
using BugTracker.API.Services.Interfaces;
using BugTracker.Core.Entities;
using BugTracker.Core.Enums;
using BugTracker.Core.Exceptions;
using BugTracker.Core.Interfaces;

namespace BugTracker.API.Services;

/// <summary>
/// Service implementation for comment operations.
/// Enforces business rule that closed bugs cannot receive new comments.
/// </summary>
public class CommentService : ICommentService
{
    private readonly ICommentRepository _commentRepository;
    private readonly IBugRepository _bugRepository;

    public CommentService(ICommentRepository commentRepository, IBugRepository bugRepository)
    {
        _commentRepository = commentRepository;
        _bugRepository = bugRepository;
    }

    public async Task<CommentResponse> AddCommentAsync(int bugId, CreateCommentRequest request)
    {
        var bug = await _bugRepository.GetByIdAsync(bugId);
        
        if (bug == null)
        {
            throw new NotFoundException("Bug", bugId);
        }

        if (bug.Status == BugStatus.Closed)
        {
            throw new ValidationException("BugId", "Cannot add comments to a closed bug");
        }

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new ValidationException("Text", "Comment text cannot be empty");
        }

        var comment = new Comment
        {
            BugId = bugId,
            Text = request.Text.Trim()
        };

        var created = await _commentRepository.AddAsync(comment);

        return MapToResponse(created);
    }

    public async Task<IEnumerable<CommentResponse>> GetCommentsByBugIdAsync(int bugId)
    {
        var bugExists = await _bugRepository.ExistsAsync(bugId);
        
        if (!bugExists)
        {
            throw new NotFoundException("Bug", bugId);
        }

        var comments = await _commentRepository.GetByBugIdAsync(bugId);
        return comments.Select(MapToResponse);
    }

    private static CommentResponse MapToResponse(Comment comment)
    {
        return new CommentResponse
        {
            Id = comment.Id,
            BugId = comment.BugId,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt
        };
    }
}
