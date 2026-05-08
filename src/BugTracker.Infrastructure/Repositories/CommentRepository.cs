using BugTracker.Core.Entities;
using BugTracker.Core.Interfaces;
using BugTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Infrastructure.Repositories;

/// <summary>
/// SQLite repository implementation for Comment entities.
/// </summary>
public class CommentRepository : ICommentRepository
{
    private readonly BugTrackerDbContext _context;

    public CommentRepository(BugTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(int id)
    {
        return await _context.Comments.FindAsync(id);
    }

    public async Task<IEnumerable<Comment>> GetByBugIdAsync(int bugId)
    {
        return await _context.Comments
            .Where(c => c.BugId == bugId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Comment> AddAsync(Comment comment)
    {
        comment.CreatedAt = DateTime.UtcNow;
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    public async Task DeleteByBugIdAsync(int bugId)
    {
        var comments = await _context.Comments.Where(c => c.BugId == bugId).ToListAsync();
        _context.Comments.RemoveRange(comments);
        await _context.SaveChangesAsync();
    }
}
