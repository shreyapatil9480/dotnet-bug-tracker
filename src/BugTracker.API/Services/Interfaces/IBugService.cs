using BugTracker.API.DTOs;

namespace BugTracker.API.Services.Interfaces;

/// <summary>
/// Service interface for bug operations.
/// </summary>
public interface IBugService
{
    Task<BugResponse> CreateBugAsync(int projectId, CreateBugRequest request);
    Task<IEnumerable<BugResponse>> GetBugsByProjectIdAsync(int projectId);
    Task<BugResponse> GetBugByIdAsync(int id);
    Task<BugResponse> UpdateBugAsync(int id, UpdateBugRequest request);
    Task DeleteBugAsync(int id);
}
