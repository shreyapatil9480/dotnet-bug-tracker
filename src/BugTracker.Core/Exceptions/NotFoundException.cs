namespace BugTracker.Core.Exceptions;

/// <summary>
/// Thrown when a requested resource does not exist.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with key '{key}' was not found.")
    {
    }
}
