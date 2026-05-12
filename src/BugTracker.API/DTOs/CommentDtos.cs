using System.ComponentModel.DataAnnotations;

namespace BugTracker.API.DTOs;

/// <summary>
/// Request DTO for creating a new comment.
/// </summary>
public class CreateCommentRequest
{
    [Required(ErrorMessage = "Text is required")]
    [StringLength(4000, MinimumLength = 1, ErrorMessage = "Comment text must be between 1 and 4000 characters")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Response DTO for comment data.
/// </summary>
public class CommentResponse
{
    public int Id { get; set; }
    public int BugId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
