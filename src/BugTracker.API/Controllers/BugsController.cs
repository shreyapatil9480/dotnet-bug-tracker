using BugTracker.API.DTOs;
using BugTracker.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.API.Controllers;

/// <summary>
/// Controller for bug management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BugsController : ControllerBase
{
    private readonly IBugService _bugService;
    private readonly ICommentService _commentService;

    public BugsController(IBugService bugService, ICommentService commentService)
    {
        _bugService = bugService;
        _commentService = commentService;
    }

    /// <summary>
    /// Gets a bug by ID.
    /// </summary>
    /// <param name="id">The bug ID.</param>
    /// <returns>The bug details.</returns>
    /// <response code="200">Returns the bug.</response>
    /// <response code="404">If the bug is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BugResponse>> GetBug(int id)
    {
        var bug = await _bugService.GetBugByIdAsync(id);
        return Ok(bug);
    }

    /// <summary>
    /// Updates a bug (status, severity, title, description).
    /// Status transitions must follow: Open → InProgress → Resolved → Closed.
    /// </summary>
    /// <param name="id">The bug ID.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated bug.</returns>
    /// <response code="200">Returns the updated bug.</response>
    /// <response code="400">If the request is invalid or status transition is not allowed.</response>
    /// <response code="404">If the bug is not found.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(BugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BugResponse>> UpdateBug(int id, [FromBody] UpdateBugRequest request)
    {
        var bug = await _bugService.UpdateBugAsync(id, request);
        return Ok(bug);
    }

    /// <summary>
    /// Deletes a bug and all its comments.
    /// </summary>
    /// <param name="id">The bug ID.</param>
    /// <response code="204">Bug deleted successfully.</response>
    /// <response code="404">If the bug is not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBug(int id)
    {
        await _bugService.DeleteBugAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Adds a comment to a bug. Comments cannot be added to closed bugs.
    /// </summary>
    /// <param name="id">The bug ID.</param>
    /// <param name="request">The comment creation request.</param>
    /// <returns>The created comment.</returns>
    /// <response code="201">Returns the newly created comment.</response>
    /// <response code="400">If the request is invalid or the bug is closed.</response>
    /// <response code="404">If the bug is not found.</response>
    [HttpPost("{id}/comments")]
    [ProducesResponseType(typeof(CommentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CommentResponse>> AddComment(int id, [FromBody] CreateCommentRequest request)
    {
        var comment = await _commentService.AddCommentAsync(id, request);
        return CreatedAtAction(nameof(GetComments), new { id }, comment);
    }

    /// <summary>
    /// Gets all comments for a bug, ordered by creation date ascending.
    /// </summary>
    /// <param name="id">The bug ID.</param>
    /// <returns>A list of comments for the bug.</returns>
    /// <response code="200">Returns the list of comments.</response>
    /// <response code="404">If the bug is not found.</response>
    [HttpGet("{id}/comments")]
    [ProducesResponseType(typeof(IEnumerable<CommentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<CommentResponse>>> GetComments(int id)
    {
        var comments = await _commentService.GetCommentsByBugIdAsync(id);
        return Ok(comments);
    }
}
