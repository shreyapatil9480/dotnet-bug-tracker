using BugTracker.Core.Entities;

namespace BugTracker.Core.Interfaces;

/// <summary>
/// Repository interface for Project entity operations.
/// </summary>
public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<IEnumerable<Project>> GetAllAsync();
    Task<Project> AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
