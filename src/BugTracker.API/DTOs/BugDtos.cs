using System.ComponentModel.DataAnnotations;
using BugTracker.Core.Enums;

namespace BugTracker.API.DTOs;

/// <summary>
/// Request DTO for creating a new bug.
/// </summary>
public class CreateBugRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Severity is required")]
    [EnumDataType(typeof(BugSeverity), ErrorMessage = "Invalid severity value. Must be: Low, Medium, High, or Critical")]
    public BugSeverity Severity { get; set; }
}

/// <summary>
/// Request DTO for updating an existing bug.
/// </summary>
public class UpdateBugRequest
{
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters")]
    public string? Title { get; set; }

    [StringLength(4000, ErrorMessage = "Description cannot exceed 4000 characters")]
    public string? Description { get; set; }

    [EnumDataType(typeof(BugSeverity), ErrorMessage = "Invalid severity value. Must be: Low, Medium, High, or Critical")]
    public BugSeverity? Severity { get; set; }

    [EnumDataType(typeof(BugStatus), ErrorMessage = "Invalid status value. Must be: Open, InProgress, Resolved, or Closed")]
    public BugStatus? Status { get; set; }
}

/// <summary>
/// Response DTO for bug data.
/// </summary>
public class BugResponse
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public BugSeverity Severity { get; set; }
    public BugStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int CommentCount { get; set; }
}
