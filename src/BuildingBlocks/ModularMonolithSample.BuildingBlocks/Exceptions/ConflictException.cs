namespace ModularMonolithSample.BuildingBlocks.Exceptions;

/// <summary>
/// Exception thrown when there's a conflict with the current state of a resource
/// </summary>
public class ConflictException : BaseException
{
    public ConflictException(string message) : base(message)
    {
    }

    public ConflictException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public override int StatusCode => 409; // Conflict

    public override string ErrorCode => "CONFLICT";
} 