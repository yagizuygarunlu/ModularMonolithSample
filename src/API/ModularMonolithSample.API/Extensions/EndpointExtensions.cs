using Microsoft.AspNetCore.Http.HttpResults;
using ModularMonolithSample.Event.Application.Commands.CreateEvent;
using ModularMonolithSample.Event.Application.Queries.GetEvent;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using ModularMonolithSample.Ticket.Application.Commands.IssueTicket;
using ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;
using ModularMonolithSample.BuildingBlocks.Models;
using MediatR;
using System.Diagnostics;

namespace ModularMonolithSample.API.Extensions;

/// <summary>
/// Modern .NET 9 Minimal API endpoints configuration
/// </summary>
public static class EndpointExtensions
{
    public static WebApplication MapEventManagementEndpoints(this WebApplication app)
    {
        var api = app.MapGroup("/api/v1")
            .WithTags("Event Management API")
            .WithOpenApi();

        MapEventEndpoints(api);
        MapAttendeeEndpoints(api);
        MapTicketEndpoints(api);
        MapFeedbackEndpoints(api);

        return app;
    }

    private static void MapEventEndpoints(RouteGroupBuilder group)
    {
        var events = group.MapGroup("/events").WithTags("Events");

        events.MapPost("/", CreateEventAsync)
            .WithName("CreateEvent")
            .WithSummary("Create a new event")
            .WithDescription("Creates a new event with the specified details")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        events.MapGet("/{id:guid}", GetEventAsync)
            .WithName("GetEvent")
            .WithSummary("Get event by ID")
            .WithDescription("Retrieves an event by its unique identifier")
            .Produces<ApiResponse<EventDto>>()
            .Produces<ApiResponse>(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static void MapAttendeeEndpoints(RouteGroupBuilder group)
    {
        var attendees = group.MapGroup("/attendees").WithTags("Attendees");

        attendees.MapPost("/register", RegisterAttendeeAsync)
            .WithName("RegisterAttendee")
            .WithSummary("Register a new attendee")
            .WithDescription("Registers a new attendee for events")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static void MapTicketEndpoints(RouteGroupBuilder group)
    {
        var tickets = group.MapGroup("/tickets").WithTags("Tickets");

        tickets.MapPost("/issue", IssueTicketAsync)
            .WithName("IssueTicket")
            .WithSummary("Issue a new ticket")
            .WithDescription("Issues a new ticket for an attendee")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static void MapFeedbackEndpoints(RouteGroupBuilder group)
    {
        var feedback = group.MapGroup("/feedback").WithTags("Feedback");

        feedback.MapPost("/", SubmitFeedbackAsync)
            .WithName("SubmitFeedback")
            .WithSummary("Submit event feedback")
            .WithDescription("Submits feedback for a specific event")
            .Produces<ApiResponse<Guid>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    // Modern async endpoint handlers
    private static async Task<Results<Created<ApiResponse<Guid>>, ValidationProblem, ProblemHttpResult>> 
        CreateEventAsync(CreateEventCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        
        var response = new ApiResponse<Guid>
        {
            Success = true,
            Data = result,
            Message = "Event created successfully",
            Timestamp = DateTime.UtcNow,
            TraceId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
        
        return TypedResults.Created($"/api/v1/events/{result}", response);
    }

    private static async Task<Results<Ok<ApiResponse<EventDto>>, NotFound<ApiResponse>, ProblemHttpResult>>
        GetEventAsync(Guid id, IMediator mediator)
    {
        var query = new GetEventQuery(id);
        var result = await mediator.Send(query);

        if (result == null)
        {
            var notFoundResponse = new ApiResponse
            {
                Success = false,
                Message = "Event not found",
                Timestamp = DateTime.UtcNow,
                TraceId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
            };
            return TypedResults.NotFound(notFoundResponse);
        }

        var response = new ApiResponse<EventDto>
        {
            Success = true,
            Data = result,
            Message = "Event retrieved successfully",
            Timestamp = DateTime.UtcNow,
            TraceId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
        
        return TypedResults.Ok(response);
    }

    private static async Task<Results<Created<ApiResponse<Guid>>, ValidationProblem, ProblemHttpResult>>
        RegisterAttendeeAsync(RegisterAttendeeCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        
        var response = new ApiResponse<Guid>
        {
            Success = true,
            Data = result,
            Message = "Attendee registered successfully",
            Timestamp = DateTime.UtcNow,
            TraceId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
        
        return TypedResults.Created($"/api/v1/attendees/{result}", response);
    }

    private static async Task<Results<Created<ApiResponse<Guid>>, ValidationProblem, ProblemHttpResult>>
        IssueTicketAsync(IssueTicketCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        
        var response = new ApiResponse<Guid>
        {
            Success = true,
            Data = result,
            Message = "Ticket issued successfully",
            Timestamp = DateTime.UtcNow,
            TraceId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
        
        return TypedResults.Created($"/api/v1/tickets/{result}", response);
    }

    private static async Task<Results<Created<ApiResponse<Guid>>, ValidationProblem, ProblemHttpResult>>
        SubmitFeedbackAsync(SubmitFeedbackCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        
        var response = new ApiResponse<Guid>
        {
            Success = true,
            Data = result,
            Message = "Feedback submitted successfully",
            Timestamp = DateTime.UtcNow,
            TraceId = Activity.Current?.Id ?? Guid.NewGuid().ToString()
        };
        
        return TypedResults.Created($"/api/v1/feedback/{result}", response);
    }
} 