using BugTracker.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BugTracker.API.Controllers;

/// <summary>
/// Test-only endpoints for BDD and automation isolation. Available only in the Test environment.
/// </summary>
[ApiController]
[Route("api/test")]
[Produces("application/json")]
public class TestController : ControllerBase
{
    private readonly BugTrackerDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public TestController(BugTrackerDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    /// <summary>
    /// Clears all projects, bugs, and comments so each BDD scenario starts from a clean database.
    /// </summary>
    /// <response code="204">Database reset successfully.</response>
    /// <response code="404">Endpoint is not available outside the Test environment.</response>
    [HttpDelete("reset")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetDatabase()
    {
        if (!_environment.IsEnvironment("Test"))
            return NotFound();

        await _dbContext.Comments.ExecuteDeleteAsync();
        await _dbContext.Bugs.ExecuteDeleteAsync();
        await _dbContext.Projects.ExecuteDeleteAsync();

        return NoContent();
    }
}
