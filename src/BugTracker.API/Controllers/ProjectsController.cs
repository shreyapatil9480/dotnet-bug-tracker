using BugTracker.API.DTOs;
using BugTracker.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BugTracker.API.Controllers;

/// <summary>
/// Controller for project management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IBugService _bugService;

    public ProjectsController(IProjectService projectService, IBugService bugService)
    {
        _projectService = projectService;
        _bugService = bugService;
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    /// <param name="request">The project creation request.</param>
    /// <returns>The created project.</returns>
    /// <response code="201">Returns the newly created project.</response>
    /// <response code="400">If the request is invalid.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProjectResponse>> CreateProject([FromBody] CreateProjectRequest request)
    {
        var project = await _projectService.CreateProjectAsync(request);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    /// <summary>
    /// Gets all projects.
    /// </summary>
    /// <returns>A list of all projects.</returns>
    /// <response code="200">Returns the list of projects.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ProjectResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProjectResponse>>> GetProjects()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects);
    }

    /// <summary>
    /// Gets a project by ID.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>The project details.</returns>
    /// <response code="200">Returns the project.</response>
    /// <response code="404">If the project is not found.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ProjectResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectResponse>> GetProject(int id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        return Ok(project);
    }

    /// <summary>
    /// Deletes a project and all its bugs and comments (cascade delete).
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <response code="204">Project deleted successfully.</response>
    /// <response code="404">If the project is not found.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(int id)
    {
        await _projectService.DeleteProjectAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Creates a new bug for a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <param name="request">The bug creation request.</param>
    /// <returns>The created bug.</returns>
    /// <response code="201">Returns the newly created bug.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <response code="404">If the project is not found.</response>
    [HttpPost("{id}/bugs")]
    [ProducesResponseType(typeof(BugResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BugResponse>> CreateBug(int id, [FromBody] CreateBugRequest request)
    {
        var bug = await _bugService.CreateBugAsync(id, request);
        return CreatedAtAction(
            nameof(BugsController.GetBug),
            "Bugs",
            new { id = bug.Id },
            bug);
    }

    /// <summary>
    /// Gets all bugs for a project.
    /// </summary>
    /// <param name="id">The project ID.</param>
    /// <returns>A list of bugs for the project.</returns>
    /// <response code="200">Returns the list of bugs.</response>
    /// <response code="404">If the project is not found.</response>
    [HttpGet("{id}/bugs")]
    [ProducesResponseType(typeof(IEnumerable<BugResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<BugResponse>>> GetBugs(int id)
    {
        var bugs = await _bugService.GetBugsByProjectIdAsync(id);
        return Ok(bugs);
    }
}
