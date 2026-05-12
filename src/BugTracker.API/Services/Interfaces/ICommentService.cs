using BugTracker.API.DTOs;

namespace BugTracker.API.Services.Interfaces;

/// <summary>
/// Service interface for comment operations.
/// </summary>
public interface ICommentService
{
    Task<CommentResponse> AddCommentAsync(int bugId, CreateCommentRequest request);
    Task<IEnumerable<CommentResponse>> GetCommentsByBugIdAsync(int bugId);
}
