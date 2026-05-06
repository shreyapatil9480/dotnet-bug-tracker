using BugTracker.Core.Enums;

namespace BugTracker.Core.Entities;

/// <summary>
/// Represents a bug/issue logged against a project with severity and status lifecycle.
/// </summary>
public class Bug
{
    public int Id { get; set; }
    
    public int ProjectId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public BugSeverity Severity { get; set; } = BugSeverity.Medium;
    
    public BugStatus Status { get; set; } = BugStatus.Open;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Project? Project { get; set; }
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
