namespace BugTracker.Core.Exceptions;

/// <summary>
/// Thrown when input validation fails (e.g., invalid severity value).
/// </summary>
public class ValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string propertyName, string errorMessage) 
        : base($"Validation failed for '{propertyName}': {errorMessage}")
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }

    public ValidationException(IDictionary<string, string[]> errors) 
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}
