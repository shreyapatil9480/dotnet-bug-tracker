using BugTracker.Core.Entities;

namespace BugTracker.Core.Interfaces;

/// <summary>
/// Repository interface for Comment entity operations.
/// </summary>
public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(int id);
    Task<IEnumerable<Comment>> GetByBugIdAsync(int bugId);
    Task<Comment> AddAsync(Comment comment);
    Task DeleteByBugIdAsync(int bugId);
}
