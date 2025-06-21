namespace ModularMonolithSample.BuildingBlocks.Models;

/// <summary>
/// Result pattern implementation for better error handling
/// </summary>
/// <typeparam name="T">The type of value for successful results</typeparam>
public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    private Result(bool isSuccess, T? value, List<string> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
        Error = errors.FirstOrDefault();
    }

    /// <summary>
    /// Creates a successful result with value
    /// </summary>
    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, string.Empty);
    }

    /// <summary>
    /// Creates a failed result with error message
    /// </summary>
    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error);
    }

    /// <summary>
    /// Creates a failed result with multiple error messages
    /// </summary>
    public static Result<T> Failure(List<string> errors)
    {
        return new Result<T>(false, default, errors);
    }

    /// <summary>
    /// Creates a failed result from exception
    /// </summary>
    public static Result<T> Failure(Exception exception)
    {
        return new Result<T>(false, default, exception.Message);
    }

    /// <summary>
    /// Converts result to ApiResponse
    /// </summary>
    public ApiResponse<T> ToApiResponse(string? successMessage = null)
    {
        if (IsSuccess)
        {
            return ApiResponse<T>.SuccessResult(Value!, successMessage ?? "Operation completed successfully");
        }

        return ApiResponse<T>.ErrorResult(Error ?? "An error occurred", Errors.Any() ? Errors : null);
    }
}

/// <summary>
/// Non-generic Result for operations without return values
/// </summary>
public class Result
{
    public bool IsSuccess { get; private set; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; private set; }
    public List<string> Errors { get; private set; } = new();

    private Result(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    private Result(bool isSuccess, List<string> errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
        Error = errors.FirstOrDefault();
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success()
    {
        return new Result(true, string.Empty);
    }

    /// <summary>
    /// Creates a failed result with error message
    /// </summary>
    public static Result Failure(string error)
    {
        return new Result(false, error);
    }

    /// <summary>
    /// Creates a failed result with multiple error messages
    /// </summary>
    public static Result Failure(List<string> errors)
    {
        return new Result(false, errors);
    }

    /// <summary>
    /// Creates a failed result from exception
    /// </summary>
    public static Result Failure(Exception exception)
    {
        return new Result(false, exception.Message);
    }

    /// <summary>
    /// Converts result to ApiResponse
    /// </summary>
    public ApiResponse ToApiResponse(string? successMessage = null)
    {
        if (IsSuccess)
        {
            return ApiResponse.SuccessResult(successMessage ?? "Operation completed successfully");
        }

        return ApiResponse.ErrorResult(Error ?? "An error occurred", Errors.Any() ? Errors : null);
    }

    /// <summary>
    /// Converts to generic Result<T>
    /// </summary>
    public Result<T> To<T>(T value)
    {
        if (IsSuccess)
        {
            return Result<T>.Success(value);
        }

        return Errors.Any() ? Result<T>.Failure(Errors) : Result<T>.Failure(Error!);
    }
} 