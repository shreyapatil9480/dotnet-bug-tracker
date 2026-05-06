namespace BugTracker.Core.Entities;

/// <summary>
/// Represents a comment attached to a bug. Comments cannot be added to closed bugs.
/// </summary>
public class Comment
{
    public int Id { get; set; }
    
    public int BugId { get; set; }
    
    public string Text { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Bug? Bug { get; set; }
}
