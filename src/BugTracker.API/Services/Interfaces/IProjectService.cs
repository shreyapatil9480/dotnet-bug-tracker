using BugTracker.API.DTOs;

namespace BugTracker.API.Services.Interfaces;

/// <summary>
/// Service interface for project operations.
/// </summary>
public interface IProjectService
{
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request);
    Task<IEnumerable<ProjectResponse>> GetAllProjectsAsync();
    Task<ProjectResponse> GetProjectByIdAsync(int id);
    Task DeleteProjectAsync(int id);
}
