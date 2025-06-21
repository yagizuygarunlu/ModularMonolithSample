namespace ModularMonolithSample.BuildingBlocks.Exceptions;

/// <summary>
/// Exception thrown when a domain business rule is violated
/// </summary>
public class DomainException : BaseException
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public override int StatusCode => 400; // Bad Request

    public override string ErrorCode => "DOMAIN_ERROR";
} 