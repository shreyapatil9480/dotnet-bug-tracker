using BugTracker.Core.Entities;
using BugTracker.Core.Interfaces;
using BugTracker.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.Infrastructure.Repositories;

/// <summary>
/// SQLite repository implementation for Bug entities.
/// </summary>
public class BugRepository : IBugRepository
{
    private readonly BugTrackerDbContext _context;

    public BugRepository(BugTrackerDbContext context)
    {
        _context = context;
    }

    public async Task<Bug?> GetByIdAsync(int id)
    {
        return await _context.Bugs
            .Include(b => b.Comments)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<IEnumerable<Bug>> GetByProjectIdAsync(int projectId)
    {
        return await _context.Bugs
            .Where(b => b.ProjectId == projectId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<Bug> AddAsync(Bug bug)
    {
        bug.CreatedAt = DateTime.UtcNow;
        bug.UpdatedAt = DateTime.UtcNow;
        _context.Bugs.Add(bug);
        await _context.SaveChangesAsync();
        return bug;
    }

    public async Task UpdateAsync(Bug bug)
    {
        bug.UpdatedAt = DateTime.UtcNow;
        _context.Bugs.Update(bug);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var bug = await _context.Bugs.FindAsync(id);
        if (bug != null)
        {
            _context.Bugs.Remove(bug);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Bugs.AnyAsync(b => b.Id == id);
    }

    public async Task DeleteByProjectIdAsync(int projectId)
    {
        var bugs = await _context.Bugs.Where(b => b.ProjectId == projectId).ToListAsync();
        _context.Bugs.RemoveRange(bugs);
        await _context.SaveChangesAsync();
    }
}
