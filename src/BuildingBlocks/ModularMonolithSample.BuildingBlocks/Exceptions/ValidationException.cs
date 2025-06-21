using FluentValidation.Results;

namespace ModularMonolithSample.BuildingBlocks.Exceptions;

/// <summary>
/// Exception thrown when validation fails
/// </summary>
public class ValidationException : BaseException
{
    public ValidationException() : base("One or more validation failures have occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures) : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(string message, Exception innerException) : base(message, innerException)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public override int StatusCode => 422; // Unprocessable Entity

    public override string ErrorCode => "VALIDATION_ERROR";

    public IDictionary<string, string[]> Errors { get; }

    public override object? Details => Errors;
} 