using BugTracker.Core.Entities;

namespace BugTracker.Core.Interfaces;

/// <summary>
/// Repository interface for Bug entity operations.
/// </summary>
public interface IBugRepository
{
    Task<Bug?> GetByIdAsync(int id);
    Task<IEnumerable<Bug>> GetByProjectIdAsync(int projectId);
    Task<Bug> AddAsync(Bug bug);
    Task UpdateAsync(Bug bug);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task DeleteByProjectIdAsync(int projectId);
}
