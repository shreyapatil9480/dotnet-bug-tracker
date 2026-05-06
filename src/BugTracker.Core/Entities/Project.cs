namespace BugTracker.Core.Entities;

/// <summary>
/// Represents a project that contains bugs. Deleting a project cascades to all its bugs and comments.
/// </summary>
public class Project
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Bug> Bugs { get; set; } = new List<Bug>();
}
