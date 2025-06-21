using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ModularMonolithSample.BuildingBlocks.Behaviors;

/// <summary>
/// Logs all MediatR requests and responses with execution details
/// </summary>
/// <typeparam name="TRequest">The request type</typeparam>
/// <typeparam name="TResponse">The response type</typeparam>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();
        var stopwatch = Stopwatch.StartNew();

        // Log request start
        _logger.LogInformation(
            "Starting request {RequestName} with ID {RequestId} at {Timestamp}. Request data: {RequestData}",
            requestName,
            requestId,
            DateTime.UtcNow,
            JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = false }));

        TResponse response;
        try
        {
            response = await next();
            
            stopwatch.Stop();
            
            // Log successful response
            _logger.LogInformation(
                "Completed request {RequestName} with ID {RequestId} in {ElapsedMilliseconds}ms. Response type: {ResponseType}",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds,
                typeof(TResponse).Name);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Log error
            _logger.LogError(ex,
                "Request {RequestName} with ID {RequestId} failed after {ElapsedMilliseconds}ms. Error: {ErrorMessage}",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds,
                ex.Message);
            
            throw;
        }
    }
} 