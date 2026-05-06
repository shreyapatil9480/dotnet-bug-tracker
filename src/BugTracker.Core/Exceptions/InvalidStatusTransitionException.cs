using BugTracker.Core.Enums;

namespace BugTracker.Core.Exceptions;

/// <summary>
/// Thrown when an invalid bug status transition is attempted.
/// Valid transitions: Open → InProgress → Resolved → Closed (no backwards, no skipping).
/// </summary>
public class InvalidStatusTransitionException : Exception
{
    public BugStatus CurrentStatus { get; }
    public BugStatus AttemptedStatus { get; }

    public InvalidStatusTransitionException(BugStatus currentStatus, BugStatus attemptedStatus)
        : base($"Invalid status transition from '{currentStatus}' to '{attemptedStatus}'. " +
               "Status must progress in order: Open → InProgress → Resolved → Closed.")
    {
        CurrentStatus = currentStatus;
        AttemptedStatus = attemptedStatus;
    }
}
