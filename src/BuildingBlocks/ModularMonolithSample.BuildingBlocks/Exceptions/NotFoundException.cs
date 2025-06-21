namespace ModularMonolithSample.BuildingBlocks.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found
/// </summary>
public class NotFoundException : BaseException
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string entityName, object key) 
        : base($"{entityName} with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    public NotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public override int StatusCode => 404; // Not Found

    public override string ErrorCode => "NOT_FOUND";

    public string? EntityName { get; }
    public object? Key { get; }
} 