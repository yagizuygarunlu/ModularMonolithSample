namespace ModularMonolithSample.BuildingBlocks.Models;

/// <summary>
/// Generic API response wrapper for consistent response structure
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public object? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }

    /// <summary>
    /// Creates a successful response with data
    /// </summary>
    public static ApiResponse<T> CreateSuccess(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Creates a successful response without data
    /// </summary>
    public static ApiResponse<T> CreateSuccess(string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static ApiResponse<T> CreateFailure(string message, object? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }

    /// <summary>
    /// Creates a successful response with data (legacy method)
    /// </summary>
    public static ApiResponse<T> SuccessResult(T data, string message = "Success")
    {
        return CreateSuccess(data, message);
    }

    /// <summary>
    /// Creates a successful response without data (legacy method)
    /// </summary>
    public static ApiResponse<T> SuccessResult(string message = "Success")
    {
        return CreateSuccess(message);
    }

    /// <summary>
    /// Creates an error response (legacy method)
    /// </summary>
    public static ApiResponse<T> ErrorResult(string message, object? errors = null)
    {
        return CreateFailure(message, errors);
    }

    /// <summary>
    /// Creates an error response from exception
    /// </summary>
    public static ApiResponse<T> ErrorResult(Exception exception)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = exception.Message,
            Errors = exception.GetType().Name
        };
    }
}

/// <summary>
/// Non-generic API response for responses without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Creates a successful response without data
    /// </summary>
    public static new ApiResponse SuccessResult(string message = "Success")
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    /// <summary>
    /// Creates an error response
    /// </summary>
    public static new ApiResponse ErrorResult(string message, object? errors = null)
    {
        return new ApiResponse
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
} 