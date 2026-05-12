using BugTracker.API.DTOs;
using BugTracker.API.Services.Interfaces;
using BugTracker.Core.Entities;
using BugTracker.Core.Enums;
using BugTracker.Core.Exceptions;
using BugTracker.Core.Interfaces;

namespace BugTracker.API.Services;

/// <summary>
/// Service implementation for bug operations.
/// Enforces business rules including status transition validation.
/// </summary>
public class BugService : IBugService
{
    private readonly IBugRepository _bugRepository;
    private readonly IProjectRepository _projectRepository;

    public BugService(IBugRepository bugRepository, IProjectRepository projectRepository)
    {
        _bugRepository = bugRepository;
        _projectRepository = projectRepository;
    }

    public async Task<BugResponse> CreateBugAsync(int projectId, CreateBugRequest request)
    {
        var projectExists = await _projectRepository.ExistsAsync(projectId);
        if (!projectExists)
        {
            throw new NotFoundException("Project", projectId);
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ValidationException("Title", "Bug title cannot be empty");
        }

        if (!Enum.IsDefined(typeof(BugSeverity), request.Severity))
        {
            throw new ValidationException("Severity", $"Invalid severity value: {request.Severity}");
        }

        var bug = new Bug
        {
            ProjectId = projectId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            Severity = request.Severity,
            Status = BugStatus.Open
        };

        var created = await _bugRepository.AddAsync(bug);

        return MapToResponse(created);
    }

    public async Task<IEnumerable<BugResponse>> GetBugsByProjectIdAsync(int projectId)
    {
        var projectExists = await _projectRepository.ExistsAsync(projectId);
        if (!projectExists)
        {
            throw new NotFoundException("Project", projectId);
        }

        var bugs = await _bugRepository.GetByProjectIdAsync(projectId);
        return bugs.Select(MapToResponse);
    }

    public async Task<BugResponse> GetBugByIdAsync(int id)
    {
        var bug = await _bugRepository.GetByIdAsync(id);
        
        if (bug == null)
        {
            throw new NotFoundException("Bug", id);
        }

        return MapToResponse(bug);
    }

    public async Task<BugResponse> UpdateBugAsync(int id, UpdateBugRequest request)
    {
        var bug = await _bugRepository.GetByIdAsync(id);
        
        if (bug == null)
        {
            throw new NotFoundException("Bug", id);
        }

        if (request.Title != null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ValidationException("Title", "Bug title cannot be empty");
            }
            bug.Title = request.Title.Trim();
        }

        if (request.Description != null)
        {
            bug.Description = request.Description.Trim();
        }

        if (request.Severity.HasValue)
        {
            if (!Enum.IsDefined(typeof(BugSeverity), request.Severity.Value))
            {
                throw new ValidationException("Severity", $"Invalid severity value: {request.Severity}");
            }
            bug.Severity = request.Severity.Value;
        }

        if (request.Status.HasValue)
        {
            ValidateStatusTransition(bug.Status, request.Status.Value);
            bug.Status = request.Status.Value;
        }

        await _bugRepository.UpdateAsync(bug);

        return MapToResponse(bug);
    }

    public async Task DeleteBugAsync(int id)
    {
        var exists = await _bugRepository.ExistsAsync(id);
        
        if (!exists)
        {
            throw new NotFoundException("Bug", id);
        }

        await _bugRepository.DeleteAsync(id);
    }

    /// <summary>
    /// Validates that a status transition follows the allowed progression:
    /// Open → InProgress → Resolved → Closed (no backwards, no skipping).
    /// </summary>
    private static void ValidateStatusTransition(BugStatus currentStatus, BugStatus newStatus)
    {
        if (currentStatus == newStatus)
        {
            return;
        }

        bool isValidTransition = (currentStatus, newStatus) switch
        {
            (BugStatus.Open, BugStatus.InProgress) => true,
            (BugStatus.InProgress, BugStatus.Resolved) => true,
            (BugStatus.Resolved, BugStatus.Closed) => true,
            _ => false
        };

        if (!isValidTransition)
        {
            throw new InvalidStatusTransitionException(currentStatus, newStatus);
        }
    }

    private static BugResponse MapToResponse(Bug bug)
    {
        return new BugResponse
        {
            Id = bug.Id,
            ProjectId = bug.ProjectId,
            Title = bug.Title,
            Description = bug.Description,
            Severity = bug.Severity,
            Status = bug.Status,
            CreatedAt = bug.CreatedAt,
            UpdatedAt = bug.UpdatedAt,
            CommentCount = bug.Comments?.Count ?? 0
        };
    }
}
