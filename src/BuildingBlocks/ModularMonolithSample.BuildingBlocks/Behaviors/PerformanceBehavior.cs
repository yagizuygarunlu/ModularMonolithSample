using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ModularMonolithSample.BuildingBlocks.Behaviors;

/// <summary>
/// Monitors performance of MediatR requests and alerts on slow operations
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly PerformanceSettings _settings;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger, PerformanceSettings? settings = null)
    {
        _logger = logger;
        _settings = settings ?? new PerformanceSettings();
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        var response = await next();

        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

        // Log performance metrics
        if (elapsedMilliseconds > _settings.SlowRequestThresholdMs)
        {
            _logger.LogWarning(
                "Slow request detected: {RequestName} took {ElapsedMilliseconds}ms (threshold: {ThresholdMs}ms)",
                requestName,
                elapsedMilliseconds,
                _settings.SlowRequestThresholdMs);
        }
        else if (elapsedMilliseconds > _settings.MediumRequestThresholdMs)
        {
            _logger.LogInformation(
                "Medium duration request: {RequestName} took {ElapsedMilliseconds}ms",
                requestName,
                elapsedMilliseconds);
        }
        else
        {
            _logger.LogDebug(
                "Fast request: {RequestName} took {ElapsedMilliseconds}ms",
                requestName,
                elapsedMilliseconds);
        }

        // Could add metrics collection here (e.g., to Application Insights, Prometheus, etc.)
        CollectMetrics(requestName, elapsedMilliseconds);

        return response;
    }

    private void CollectMetrics(string requestName, long elapsedMilliseconds)
    {
        // Placeholder for metrics collection
        // In a real application, you would integrate with your metrics system
        // Examples:
        // - Application Insights
        // - Prometheus
        // - StatsD
        // - Custom metrics storage
        
        _logger.LogTrace(
            "Performance metric collected: {RequestName} = {ElapsedMilliseconds}ms", 
            requestName, 
            elapsedMilliseconds);
    }
}

/// <summary>
/// Configuration settings for performance monitoring
/// </summary>
public class PerformanceSettings
{
    /// <summary>
    /// Threshold in milliseconds for considering a request as slow (default: 3000ms)
    /// </summary>
    public long SlowRequestThresholdMs { get; set; } = 3000;

    /// <summary>
    /// Threshold in milliseconds for considering a request as medium duration (default: 1000ms)
    /// </summary>
    public long MediumRequestThresholdMs { get; set; } = 1000;

    /// <summary>
    /// Whether to enable detailed performance logging (default: true)
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;
} 