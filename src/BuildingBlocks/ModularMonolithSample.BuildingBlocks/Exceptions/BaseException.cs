namespace ModularMonolithSample.BuildingBlocks.Exceptions;

/// <summary>
/// Base exception class for all custom exceptions in the application
/// </summary>
public abstract class BaseException : Exception
{
    protected BaseException(string message) : base(message)
    {
    }

    protected BaseException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <summary>
    /// HTTP status code to return for this exception
    /// </summary>
    public abstract int StatusCode { get; }

    /// <summary>
    /// Error code for categorizing the exception
    /// </summary>
    public abstract string ErrorCode { get; }

    /// <summary>
    /// Additional details about the error
    /// </summary>
    public virtual object? Details { get; set; }
} 