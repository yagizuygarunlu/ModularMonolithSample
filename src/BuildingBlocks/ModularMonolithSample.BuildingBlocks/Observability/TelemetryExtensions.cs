using System.Diagnostics;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ModularMonolithSample.BuildingBlocks.Observability;

/// <summary>
/// Modern observability configuration for .NET 9 with OpenTelemetry
/// </summary>
public static class TelemetryExtensions
{
    public static IServiceCollection AddModernObservability(
        this IServiceCollection services, 
        string serviceName = "ModularMonolithSample")
    {
        // Add custom metrics
        services.AddSingleton<EventManagementMetrics>();
        
        // Add custom activity source
        services.AddSingleton<EventManagementActivitySource>();

        // Configure structured logging
        services.AddLogging(builder =>
        {
            builder.AddConsole()
                   .AddDebug()
                   .AddSystemdConsole(); // For containerized environments
        });

        return services;
    }
}

/// <summary>
/// Custom metrics for event management system
/// </summary>
public class EventManagementMetrics : IDisposable
{
    private readonly Meter _meter;
    
    // Counters
    public readonly Counter<int> EventsCreated;
    public readonly Counter<int> AttendeesRegistered;
    public readonly Counter<int> TicketsIssued;
    public readonly Counter<int> FeedbackSubmitted;
    public readonly Counter<int> ValidationErrors;
    
    // Gauges (ObservableGauges for current values)
    public readonly ObservableGauge<int> ActiveEvents;
    public readonly ObservableGauge<int> TotalAttendees;
    
    // Histograms
    public readonly Histogram<double> RequestDuration;
    public readonly Histogram<double> DatabaseQueryDuration;

    public EventManagementMetrics()
    {
        _meter = new Meter("ModularMonolithSample.EventManagement", "1.0.0");
        
        // Initialize counters
        EventsCreated = _meter.CreateCounter<int>(
            "events_created_total",
            description: "Total number of events created");
            
        AttendeesRegistered = _meter.CreateCounter<int>(
            "attendees_registered_total", 
            description: "Total number of attendees registered");
            
        TicketsIssued = _meter.CreateCounter<int>(
            "tickets_issued_total",
            description: "Total number of tickets issued");
            
        FeedbackSubmitted = _meter.CreateCounter<int>(
            "feedback_submitted_total",
            description: "Total number of feedback submissions");
            
        ValidationErrors = _meter.CreateCounter<int>(
            "validation_errors_total",
            description: "Total number of validation errors");

        // Initialize observable gauges
        ActiveEvents = _meter.CreateObservableGauge<int>(
            "active_events_current",
            description: "Current number of active events",
            observeValue: () => GetActiveEventsCount());
            
        TotalAttendees = _meter.CreateObservableGauge<int>(
            "total_attendees_current",
            description: "Current total number of attendees",
            observeValue: () => GetTotalAttendeesCount());

        // Initialize histograms
        RequestDuration = _meter.CreateHistogram<double>(
            "request_duration_seconds",
            unit: "s",
            description: "Duration of HTTP requests");
            
        DatabaseQueryDuration = _meter.CreateHistogram<double>(
            "database_query_duration_seconds",
            unit: "s", 
            description: "Duration of database queries");
    }

    private int GetActiveEventsCount()
    {
        // This would typically query your data store
        // For now, return a mock value
        return Random.Shared.Next(1, 50);
    }

    private int GetTotalAttendeesCount()
    {
        // This would typically query your data store
        // For now, return a mock value
        return Random.Shared.Next(100, 1000);
    }

    public void Dispose()
    {
        _meter?.Dispose();
    }
}

/// <summary>
/// Custom activity source for distributed tracing
/// </summary>
public class EventManagementActivitySource : IDisposable
{
    private readonly ActivitySource _activitySource;
    
    public EventManagementActivitySource()
    {
        _activitySource = new ActivitySource("ModularMonolithSample.EventManagement", "1.0.0");
    }

    public Activity? StartActivity(string name, ActivityKind kind = ActivityKind.Internal)
    {
        return _activitySource.StartActivity(name, kind);
    }

    public Activity? StartEventCreationActivity(string eventName)
    {
        var activity = _activitySource.StartActivity("event.create");
        activity?.SetTag("event.name", eventName);
        activity?.SetTag("operation.type", "create");
        return activity;
    }

    public Activity? StartAttendeeRegistrationActivity(string attendeeEmail)
    {
        var activity = _activitySource.StartActivity("attendee.register");
        activity?.SetTag("attendee.email", attendeeEmail);
        activity?.SetTag("operation.type", "register");
        return activity;
    }

    public Activity? StartTicketIssuanceActivity(int eventId, int attendeeId)
    {
        var activity = _activitySource.StartActivity("ticket.issue");
        activity?.SetTag("event.id", eventId);
        activity?.SetTag("attendee.id", attendeeId);
        activity?.SetTag("operation.type", "issue");
        return activity;
    }

    public Activity? StartDatabaseQuery(string queryType, string tableName)
    {
        var activity = _activitySource.StartActivity("database.query", ActivityKind.Client);
        activity?.SetTag("db.operation", queryType);
        activity?.SetTag("db.table", tableName);
        activity?.SetTag("db.system", "sqlserver");
        return activity;
    }

    public void Dispose()
    {
        _activitySource?.Dispose();
    }
}

/// <summary>
/// Performance monitoring behavior using modern observability
/// </summary>
public class ModernPerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ModernPerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly EventManagementMetrics _metrics;
    private readonly EventManagementActivitySource _activitySource;

    public ModernPerformanceBehavior(
        ILogger<ModernPerformanceBehavior<TRequest, TResponse>> logger,
        EventManagementMetrics metrics,
        EventManagementActivitySource activitySource)
    {
        _logger = logger;
        _metrics = metrics;
        _activitySource = activitySource;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        
        using var activity = _activitySource.StartActivity($"request.{requestName}");
        activity?.SetTag("request.type", requestName);
        
        var timer = Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("Starting request {RequestName}", requestName);
            
            var response = await next();
            
            timer.Stop();
            var elapsedSeconds = timer.Elapsed.TotalSeconds;
            
            // Record metrics
            _metrics.RequestDuration.Record(elapsedSeconds, 
                new KeyValuePair<string, object?>("request.type", requestName),
                new KeyValuePair<string, object?>("request.status", "success"));
            
            activity?.SetTag("request.duration_ms", timer.ElapsedMilliseconds);
            activity?.SetTag("request.status", "success");
            
            if (elapsedSeconds > 2.0)
            {
                _logger.LogWarning("Slow request detected: {RequestName} took {ElapsedSeconds:F2}s", 
                    requestName, elapsedSeconds);
            }
            else
            {
                _logger.LogInformation("Completed request {RequestName} in {ElapsedSeconds:F2}s", 
                    requestName, elapsedSeconds);
            }
            
            return response;
        }
        catch (Exception ex)
        {
            timer.Stop();
            var elapsedSeconds = timer.Elapsed.TotalSeconds;
            
            // Record error metrics
            _metrics.RequestDuration.Record(elapsedSeconds,
                new KeyValuePair<string, object?>("request.type", requestName),
                new KeyValuePair<string, object?>("request.status", "error"));
            
            activity?.SetTag("request.duration_ms", timer.ElapsedMilliseconds);
            activity?.SetTag("request.status", "error");
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);
            
            _logger.LogError(ex, "Request {RequestName} failed after {ElapsedSeconds:F2}s", 
                requestName, elapsedSeconds);
                
            throw;
        }
    }
}

// Required interface for the behavior
public interface IPipelineBehavior<in TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

public interface IRequest<out TResponse> { }

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(); 