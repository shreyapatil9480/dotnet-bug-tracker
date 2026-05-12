using BugTracker.API.DTOs;
using BugTracker.API.Services.Interfaces;
using BugTracker.Core.Entities;
using BugTracker.Core.Exceptions;
using BugTracker.Core.Interfaces;

namespace BugTracker.API.Services;

/// <summary>
/// Service implementation for project operations.
/// Handles business logic and delegates data access to repositories.
/// </summary>
public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IBugRepository _bugRepository;
    private readonly ICommentRepository _commentRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IBugRepository bugRepository,
        ICommentRepository commentRepository)
    {
        _projectRepository = projectRepository;
        _bugRepository = bugRepository;
        _commentRepository = commentRepository;
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Name", "Project name cannot be empty");
        }

        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty
        };

        var created = await _projectRepository.AddAsync(project);

        return MapToResponse(created);
    }

    public async Task<IEnumerable<ProjectResponse>> GetAllProjectsAsync()
    {
        var projects = await _projectRepository.GetAllAsync();
        return projects.Select(MapToResponse);
    }

    public async Task<ProjectResponse> GetProjectByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        
        if (project == null)
        {
            throw new NotFoundException("Project", id);
        }

        return MapToResponse(project);
    }

    public async Task DeleteProjectAsync(int id)
    {
        var exists = await _projectRepository.ExistsAsync(id);
        
        if (!exists)
        {
            throw new NotFoundException("Project", id);
        }

        await _projectRepository.DeleteAsync(id);
    }

    private static ProjectResponse MapToResponse(Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            BugCount = project.Bugs?.Count ?? 0
        };
    }
}
