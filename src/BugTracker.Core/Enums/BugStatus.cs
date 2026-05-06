namespace BugTracker.Core.Enums;

/// <summary>
/// Defines the lifecycle states for a bug.
/// Status transitions must follow: Open → InProgress → Resolved → Closed (no backwards, no skipping).
/// </summary>
public enum BugStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2,
    Closed = 3
}
