using MediatR;
using Microsoft.Extensions.Logging;

namespace ModularMonolithSample.BuildingBlocks.Behaviors;

/// <summary>
/// Retries failed requests that implement IRetryable with exponential backoff
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class RetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<RetryBehavior<TRequest, TResponse>> _logger;

    public RetryBehavior(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        // Only retry requests that implement IRetryable
        if (request is not IRetryable retryableRequest)
        {
            return await next();
        }

        var requestName = typeof(TRequest).Name;
        var maxRetries = retryableRequest.MaxRetries;
        var baseDelay = retryableRequest.BaseDelay;
        var retryableExceptions = retryableRequest.RetryableExceptions;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                if (attempt > 0)
                {
                    _logger.LogInformation(
                        "Retrying {RequestName} - attempt {Attempt} of {MaxRetries}",
                        requestName, attempt, maxRetries);
                }

                var response = await next();
                
                if (attempt > 0)
                {
                    _logger.LogInformation(
                        "Retry successful for {RequestName} on attempt {Attempt}",
                        requestName, attempt);
                }
                
                return response;
            }
            catch (Exception ex) when (attempt < maxRetries && ShouldRetry(ex, retryableExceptions))
            {
                var delay = CalculateDelay(baseDelay, attempt);
                
                _logger.LogWarning(ex,
                    "Request {RequestName} failed on attempt {Attempt}. Will retry in {DelayMs}ms. Error: {ErrorMessage}",
                    requestName, attempt + 1, delay.TotalMilliseconds, ex.Message);

                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                if (attempt == maxRetries)
                {
                    _logger.LogError(ex,
                        "Request {RequestName} failed after {MaxRetries} attempts. Giving up. Error: {ErrorMessage}",
                        requestName, maxRetries, ex.Message);
                }
                else
                {
                    _logger.LogError(ex,
                        "Request {RequestName} failed with non-retryable exception: {ErrorMessage}",
                        requestName, ex.Message);
                }
                
                throw;
            }
        }

        // This should never be reached due to the loop logic above
        throw new InvalidOperationException($"Unexpected end of retry loop for {requestName}");
    }

    private static bool ShouldRetry(Exception exception, Type[]? retryableExceptions)
    {
        if (retryableExceptions == null || retryableExceptions.Length == 0)
        {
            // Default retryable exceptions
            return exception is TaskCanceledException ||
                   exception is TimeoutException ||
                   exception is HttpRequestException ||
                   (exception is InvalidOperationException && exception.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase));
        }

        var exceptionType = exception.GetType();
        return retryableExceptions.Any(retryableType => 
            retryableType.IsAssignableFrom(exceptionType));
    }

    private static TimeSpan CalculateDelay(TimeSpan baseDelay, int attempt)
    {
        // Exponential backoff with jitter
        var exponentialDelay = TimeSpan.FromMilliseconds(
            baseDelay.TotalMilliseconds * Math.Pow(2, attempt));
        
        // Add jitter (random delay up to 20% of the calculated delay)
        var jitter = new Random().NextDouble() * 0.2 * exponentialDelay.TotalMilliseconds;
        var totalDelay = exponentialDelay.Add(TimeSpan.FromMilliseconds(jitter));
        
        // Cap the delay at 30 seconds
        return totalDelay > TimeSpan.FromSeconds(30) 
            ? TimeSpan.FromSeconds(30) 
            : totalDelay;
    }
}

/// <summary>
/// Interface for requests that can be retried
/// </summary>
public interface IRetryable
{
    /// <summary>
    /// Maximum number of retry attempts (default: 3)
    /// </summary>
    int MaxRetries { get; }

    /// <summary>
    /// Base delay between retries (default: 100ms)
    /// </summary>
    TimeSpan BaseDelay { get; }

    /// <summary>
    /// Exception types that should trigger a retry (optional)
    /// If null, default exceptions will be used
    /// </summary>
    Type[]? RetryableExceptions { get; }
}

/// <summary>
/// Attribute to mark requests as retryable with default settings
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class RetryableAttribute : Attribute
{
    public RetryableAttribute(int maxRetries = 3, int baseDelayMs = 100)
    {
        MaxRetries = maxRetries;
        BaseDelayMs = baseDelayMs;
    }

    public int MaxRetries { get; }
    public int BaseDelayMs { get; }
    public Type[]? RetryableExceptions { get; set; }
} 