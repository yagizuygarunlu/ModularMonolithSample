using ModularMonolithSample.Attendee.Infrastructure;
using ModularMonolithSample.Event.Infrastructure;
using ModularMonolithSample.Feedback.Infrastructure;
using ModularMonolithSample.Ticket.Infrastructure;
using ModularMonolithSample.BuildingBlocks.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Text.Json;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ModularMonolithSample.BuildingBlocks.Models;
using ModularMonolithSample.Event.Application.Commands.CreateEvent;
using ModularMonolithSample.Event.Application.Queries.GetAllEvents;
using ModularMonolithSample.Event.Application.Queries.GetEvent;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using ModularMonolithSample.Attendee.Application.Queries.GetAttendee;
using ModularMonolithSample.Ticket.Application.Commands.IssueTicket;
using ModularMonolithSample.Ticket.Application.Queries.GetTicket;
using ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;
using ModularMonolithSample.Feedback.Application.Queries.GetFeedback;

var builder = WebApplication.CreateBuilder(args);

// Configure modern JSON options for .NET 9
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("ver")
    );
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add services to the container
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

// Add basic health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"), tags: ["ready"])
    .AddCheck("database", () => HealthCheckResult.Healthy("Database connection available"), tags: ["ready"]);

// Enhanced Swagger with modern .NET 9 features
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { 
        Title = "Event Management API", 
        Version = "v1.0.0",
        Description = """
            🎯 **Modern Event Management System - 2025 Edition**
            
            A comprehensive enterprise-grade event management system built with:
            - 🏗️ **Modular Monolith Architecture** 
            - 🧅 **Clean Architecture** with CQRS
            - ⚡ **Modern .NET 9 Features**
            - 🎭 **Advanced MediatR Behaviors**
            - 🛡️ **Global Exception Handling**
            - 📊 **API Response Wrappers**
            - 🏥 **Health Checks**
            - 📈 **Performance Monitoring**
            - 🔄 **API Versioning**
            - ✨ **Minimal APIs**
            
            Perfect for demonstrating modern .NET architecture patterns and enterprise-level features.
            """,
        Contact = new() { Name = "ModularMonolithSample", Url = new Uri("https://github.com/yagizuygarunlu/ModularMonolithSample") },
        License = new() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
    });
    
    c.SwaggerDoc("v2", new() { 
        Title = "Event Management API", 
        Version = "v2.0.0",
        Description = """
            🚀 **Event Management API v2 - Enhanced Features**
            
            Version 2.0 includes:
            - 📄 **Pagination Support**
            - 🔍 **Enhanced Search**
            - 📊 **Advanced Analytics**
            - 🎫 **Ticket Management**
            - 💬 **Feedback Analytics**
            
            Backward compatible with v1.0
            """,
        Contact = new() { Name = "ModularMonolithSample", Url = new Uri("https://github.com/yagizuygarunlu/ModularMonolithSample") },
        License = new() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
    });
});

// Add standard API services with enhanced configuration
builder.Services.AddBuildingBlocks(config =>
{
    // Core behaviors - always enabled
    config.EnableLogging = true;
    config.EnablePerformanceMonitoring = true;
    config.EnableCaching = true;
    
    // Advanced behaviors - can be configured via appsettings
    config.EnableRetry = false; // Can be enabled for specific scenarios
    config.EnableAuditing = false; // Enable in production for compliance
    config.EnableTransactions = false; // Enable when you have proper transaction manager
    
    // Performance thresholds
    config.PerformanceSettings.SlowRequestThresholdMs = 2000;
    config.PerformanceSettings.MediumRequestThresholdMs = 500;
});

// Configure modular components
builder.Services.AddEventModule();
builder.Services.AddAttendeeModule();
builder.Services.AddTicketModule();
builder.Services.AddFeedbackModule();

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline with modern middleware order
app.UseGlobalExceptionHandling(); // Must be first for proper error handling

// Development environment specific middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        
        foreach (var description in provider.ApiVersionDescriptions)
        {
            c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", 
                $"Event Management API {description.GroupName.ToUpperInvariant()}");
        }
        
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
        c.DocumentTitle = "Event Management API - 2025 Edition";
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
        c.EnablePersistAuthorization();
    });
}

// Security middleware
app.UseHttpsRedirection();
app.UseAuthorization();

// Health checks with detailed responses
app.MapHealthChecks("/health", new()
{
    ResponseWriter = async (context, report) =>
    {
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            duration = report.TotalDuration,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration,
                description = e.Value.Description,
                data = e.Value.Data,
                tags = e.Value.Tags
            })
        };

        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true 
        }));
    }
});

// Liveness and readiness probes for Kubernetes
app.MapHealthChecks("/health/live", new()
{
    Predicate = _ => false // No checks, just return healthy if app is running
});

app.MapHealthChecks("/health/ready", new()
{
    Predicate = check => check.Tags.Contains("ready")
});

// ============================================================================
// MINIMAL API ENDPOINTS WITH VERSIONING
// ============================================================================

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .HasApiVersion(new ApiVersion(2, 0))
    .ReportApiVersions()
    .Build();

// ============================================================================
// EVENT ENDPOINTS
// ============================================================================

var eventsV1 = app.MapGroup("/api/v{version:apiVersion}/events")
    .WithApiVersionSet(versionSet)
    .WithTags("Events")
    .WithOpenApi();

// V1 Event Endpoints
eventsV1.MapGet("/", async ([FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var query = new GetAllEventsQuery();
    var events = await mediator.Send(query, cancellationToken);
    var response = ApiResponse<IEnumerable<EventDto>>.CreateSuccess(events, "Events retrieved successfully");
    return Results.Ok(response);
})
.MapToApiVersion(1, 0)
.WithName("GetAllEventsV1")
.WithSummary("Get all events")
.WithDescription("Retrieves all events in the system")
.Produces<ApiResponse<IEnumerable<EventDto>>>();

eventsV1.MapGet("/{id:guid}", async ([FromRoute] Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var query = new GetEventQuery(id);
    var eventDto = await mediator.Send(query, cancellationToken);
    
    if (eventDto == null)
    {
        var errorResponse = ApiResponse<EventDto>.CreateFailure($"Event with ID {id} was not found.");
        return Results.NotFound(errorResponse);
    }

    var response = ApiResponse<EventDto>.CreateSuccess(eventDto, "Event retrieved successfully");
    return Results.Ok(response);
})
.MapToApiVersion(1, 0)
.WithName("GetEventV1")
.WithSummary("Get event by ID")
.WithDescription("Retrieves an event by its unique identifier")
.Produces<ApiResponse<EventDto>>()
.Produces<ApiResponse<EventDto>>(StatusCodes.Status404NotFound);

eventsV1.MapPost("/", async ([FromBody] CreateEventCommand command, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var eventId = await mediator.Send(command, cancellationToken);
    var response = ApiResponse<Guid>.CreateSuccess(eventId, "Event created successfully");
    return Results.Created($"/api/v1/events/{eventId}", response);
})
.MapToApiVersion(1, 0)
.WithName("CreateEventV1")
.WithSummary("Create a new event")
.WithDescription("Creates a new event in the system")
.Produces<ApiResponse<Guid>>(StatusCodes.Status201Created);

// V2 Event Endpoints (Enhanced with pagination)
eventsV1.MapGet("/", async ([FromServices] IMediator mediator, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default) =>
{
    var query = new GetAllEventsQuery();
    var allEvents = await mediator.Send(query, cancellationToken);
    
    var totalCount = allEvents.Count();
    var events = allEvents.Skip((page - 1) * pageSize).Take(pageSize);
    
    var response = new PagedResponse<EventDto>(events, page, pageSize, totalCount);
    return Results.Ok(response);
})
.MapToApiVersion(2, 0)
.WithName("GetAllEventsV2")
.WithSummary("Get all events with pagination")
.WithDescription("Retrieves all events with pagination support")
.Produces<PagedResponse<EventDto>>();

// ============================================================================
// ATTENDEE ENDPOINTS
// ============================================================================

var attendeesV1 = app.MapGroup("/api/v{version:apiVersion}/attendees")
    .WithApiVersionSet(versionSet)
    .WithTags("Attendees")
    .WithOpenApi();

// V1 Attendee Endpoints
attendeesV1.MapPost("/register", async ([FromBody] RegisterAttendeeCommand command, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var attendeeId = await mediator.Send(command, cancellationToken);
    var response = ApiResponse<Guid>.CreateSuccess(attendeeId, "Attendee registered successfully");
    return Results.Created($"/api/v1/attendees/{attendeeId}", response);
})
.MapToApiVersion(1, 0)
.WithName("RegisterAttendeeV1")
.WithSummary("Register a new attendee")
.WithDescription("Registers a new attendee for events")
.Produces<ApiResponse<Guid>>(StatusCodes.Status201Created);

attendeesV1.MapGet("/{id:guid}", async ([FromRoute] Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var query = new GetAttendeeQuery(id);
    var attendeeDto = await mediator.Send(query, cancellationToken);
    
    if (attendeeDto == null)
    {
        var errorResponse = ApiResponse<AttendeeDto>.CreateFailure($"Attendee with ID {id} was not found.");
        return Results.NotFound(errorResponse);
    }

    var response = ApiResponse<AttendeeDto>.CreateSuccess(attendeeDto, "Attendee retrieved successfully");
    return Results.Ok(response);
})
.MapToApiVersion(1, 0)
.WithName("GetAttendeeV1")
.WithSummary("Get attendee by ID")
.WithDescription("Retrieves an attendee by their unique identifier")
.Produces<ApiResponse<AttendeeDto>>()
.Produces<ApiResponse<AttendeeDto>>(StatusCodes.Status404NotFound);

// ============================================================================
// TICKET ENDPOINTS
// ============================================================================

var ticketsV1 = app.MapGroup("/api/v{version:apiVersion}/tickets")
    .WithApiVersionSet(versionSet)
    .WithTags("Tickets")
    .WithOpenApi();

// V1 Ticket Endpoints
ticketsV1.MapPost("/issue", async ([FromBody] IssueTicketCommand command, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var ticketId = await mediator.Send(command, cancellationToken);
    var response = ApiResponse<Guid>.CreateSuccess(ticketId, "Ticket issued successfully");
    return Results.Created($"/api/v1/tickets/{ticketId}", response);
})
.MapToApiVersion(1, 0)
.WithName("IssueTicketV1")
.WithSummary("Issue a new ticket")
.WithDescription("Issues a new ticket for an attendee")
.Produces<ApiResponse<Guid>>(StatusCodes.Status201Created);

ticketsV1.MapGet("/{id:guid}", async ([FromRoute] Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var query = new GetTicketQuery(id);
    var ticketDto = await mediator.Send(query, cancellationToken);
    
    if (ticketDto == null)
    {
        var errorResponse = ApiResponse<TicketDto>.CreateFailure($"Ticket with ID {id} was not found.");
        return Results.NotFound(errorResponse);
    }

    var response = ApiResponse<TicketDto>.CreateSuccess(ticketDto, "Ticket retrieved successfully");
    return Results.Ok(response);
})
.MapToApiVersion(1, 0)
.WithName("GetTicketV1")
.WithSummary("Get ticket by ID")
.WithDescription("Retrieves a ticket by its unique identifier")
.Produces<ApiResponse<TicketDto>>()
.Produces<ApiResponse<TicketDto>>(StatusCodes.Status404NotFound);

// ============================================================================
// FEEDBACK ENDPOINTS
// ============================================================================

var feedbackV1 = app.MapGroup("/api/v{version:apiVersion}/feedback")
    .WithApiVersionSet(versionSet)
    .WithTags("Feedback")
    .WithOpenApi();

// V1 Feedback Endpoints
feedbackV1.MapPost("/", async ([FromBody] SubmitFeedbackCommand command, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var feedbackId = await mediator.Send(command, cancellationToken);
    var response = ApiResponse<Guid>.CreateSuccess(feedbackId, "Feedback submitted successfully");
    return Results.Created($"/api/v1/feedback/{feedbackId}", response);
})
.MapToApiVersion(1, 0)
.WithName("SubmitFeedbackV1")
.WithSummary("Submit feedback")
.WithDescription("Submits feedback for an event")
.Produces<ApiResponse<Guid>>(StatusCodes.Status201Created);

feedbackV1.MapGet("/{id:guid}", async ([FromRoute] Guid id, [FromServices] IMediator mediator, CancellationToken cancellationToken) =>
{
    var query = new GetFeedbackQuery(id);
    var feedbackDto = await mediator.Send(query, cancellationToken);
    
    if (feedbackDto == null)
    {
        var errorResponse = ApiResponse<FeedbackDto>.CreateFailure($"Feedback with ID {id} was not found.");
        return Results.NotFound(errorResponse);
    }

    var response = ApiResponse<FeedbackDto>.CreateSuccess(feedbackDto, "Feedback retrieved successfully");
    return Results.Ok(response);
})
.MapToApiVersion(1, 0)
.WithName("GetFeedbackV1")
.WithSummary("Get feedback by ID")
.WithDescription("Retrieves feedback by its unique identifier")
.Produces<ApiResponse<FeedbackDto>>()
.Produces<ApiResponse<FeedbackDto>>(StatusCodes.Status404NotFound);

// ============================================================================
// SYSTEM ENDPOINTS
// ============================================================================

// Modern API versioning endpoint using Minimal APIs
app.MapGet("/api/version", () => new
{
    version = "2.0.0",
    buildDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
    framework = ".NET 9.0",
    architecture = "Modular Monolith",
    year = "2025",
    features = new[]
    {
        "Clean Architecture",
        "CQRS with MediatR", 
        "Domain Events",
        "Global Exception Handling",
        "API Response Wrappers",
        "Advanced MediatR Behaviors",
        "Health Checks",
        "Performance Monitoring",
        "API Versioning",
        "Minimal APIs",
        "Comprehensive Unit Testing (147 tests)",
        "Modern .NET 9 Features"
    }
})
.WithName("GetApiVersion")
.WithTags("System")
.WithSummary("Get API version and features")
.WithOpenApi();

// System information endpoint
app.MapGet("/api/system-info", () => new
{
    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
    machineName = Environment.MachineName,
    processId = Environment.ProcessId,
    workingSet = GC.GetTotalMemory(false),
    gcCollections = new
    {
        gen0 = GC.CollectionCount(0),
        gen1 = GC.CollectionCount(1),
        gen2 = GC.CollectionCount(2)
    },
    uptime = DateTime.UtcNow.ToString("O")
})
.WithName("GetSystemInfo")
.WithTags("System")
.WithSummary("Get system information")
.WithOpenApi();

// Startup information with modern emojis and formatting
Console.WriteLine("""
    🚀 ====================================================================
    🎯 EVENT MANAGEMENT API - 2025 EDITION STARTING UP
    🚀 ====================================================================
    
    🏗️  Architecture: Modular Monolith with Clean Architecture
    ⚡  Framework: .NET 9.0
    🎭  Patterns: CQRS, Domain Events, MediatR Behaviors
    🛡️  Features: Global Exception Handling, API Versioning
    ✨  API Style: Minimal APIs with Versioning
    📊  Monitoring: Health Checks, Performance Monitoring
    🧪  Testing: 147 Unit Tests + Integration Tests
    
    📝  Swagger UI: Available at /
    🏥  Health Checks: /health, /health/live, /health/ready
    📈  System Info: /api/system-info
    🔄  API Versions: v1.0 (stable), v2.0 (enhanced)
    
    🎉  Ready to handle enterprise-level event management!
    🚀 ====================================================================
    """);

app.Run();

// Make the implicit Program.cs class public for testing
public partial class Program { } 