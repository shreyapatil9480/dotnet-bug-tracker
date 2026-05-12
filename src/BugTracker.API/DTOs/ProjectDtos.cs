using System.ComponentModel.DataAnnotations;

namespace BugTracker.API.DTOs;

/// <summary>
/// Request DTO for creating a new project.
/// </summary>
public class CreateProjectRequest
{
    [Required(ErrorMessage = "Name is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Name must be between 1 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for project data.
/// </summary>
public class ProjectResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int BugCount { get; set; }
}
