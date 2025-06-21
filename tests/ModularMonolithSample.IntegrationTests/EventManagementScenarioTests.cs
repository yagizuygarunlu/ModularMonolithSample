using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using ModularMonolithSample.Event.Application.Commands.CreateEvent;
using ModularMonolithSample.Event.Application.Queries.GetEvent;
using ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;
using ModularMonolithSample.Ticket.Application.Commands.IssueTicket;

namespace ModularMonolithSample.IntegrationTests;

public class EventManagementScenarioTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public EventManagementScenarioTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CompleteEventManagementWorkflow_ShouldWorkEndToEnd()
    {
        // Step 1: Create an event
        var createEventCommand = new CreateEventCommand(
            "Tech Conference 2024",
            "Annual technology conference",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(32),
            "Convention Center",
            500,
            99.99m
        );

        var eventResponse = await _client.PostAsJsonAsync("/api/events", createEventCommand);
        Assert.Equal(HttpStatusCode.OK, eventResponse.StatusCode);

        var eventIdString = await eventResponse.Content.ReadAsStringAsync();
        var eventId = Guid.Parse(eventIdString.Trim('"'));

        // Step 2: Register an attendee
        var registerAttendeeCommand = new RegisterAttendeeCommand(
            "John Doe",
            "john.doe@email.com",
            eventId
        );

        var attendeeResponse = await _client.PostAsJsonAsync("/api/attendees", registerAttendeeCommand);
        Assert.Equal(HttpStatusCode.OK, attendeeResponse.StatusCode);

        var attendeeIdString = await attendeeResponse.Content.ReadAsStringAsync();
        var attendeeId = Guid.Parse(attendeeIdString.Trim('"'));

        // Step 3: Issue a ticket
        var issueTicketCommand = new IssueTicketCommand(eventId, attendeeId);

        var ticketResponse = await _client.PostAsJsonAsync("/api/tickets", issueTicketCommand);
        Assert.Equal(HttpStatusCode.OK, ticketResponse.StatusCode);

        var ticketIdString = await ticketResponse.Content.ReadAsStringAsync();
        var ticketId = Guid.Parse(ticketIdString.Trim('"'));

        // Step 4: Submit feedback
        var submitFeedbackCommand = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Excellent conference!");

        var feedbackResponse = await _client.PostAsJsonAsync("/api/feedback", submitFeedbackCommand);
        Assert.Equal(HttpStatusCode.OK, feedbackResponse.StatusCode);

        var feedbackIdString = await feedbackResponse.Content.ReadAsStringAsync();
        var feedbackId = Guid.Parse(feedbackIdString.Trim('"'));

        // Verify all IDs are valid
        Assert.NotEqual(Guid.Empty, eventId);
        Assert.NotEqual(Guid.Empty, attendeeId);
        Assert.NotEqual(Guid.Empty, ticketId);
        Assert.NotEqual(Guid.Empty, feedbackId);
    }

    [Fact]
    public async Task RegisterAttendee_WithInvalidEventId_ShouldReturnBadRequest()
    {
        var invalidEventId = Guid.NewGuid();
        var registerAttendeeCommand = new RegisterAttendeeCommand(
            "Jane Smith",
            "jane.smith@email.com",
            invalidEventId
        );

        var response = await _client.PostAsJsonAsync("/api/attendees", registerAttendeeCommand);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task IssueTicket_WithInvalidAttendeeId_ShouldReturnBadRequest()
    {
        var invalidAttendeeId = Guid.NewGuid();
        var invalidEventId = Guid.NewGuid();
        var issueTicketCommand = new IssueTicketCommand(invalidEventId, invalidAttendeeId);

        var response = await _client.PostAsJsonAsync("/api/tickets", issueTicketCommand);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateEvent_WithValidData_ShouldReturnSuccess()
    {
        var createEventCommand = new CreateEventCommand(
            "Workshop 2024",
            "Hands-on workshop",
            DateTime.UtcNow.AddDays(15),
            DateTime.UtcNow.AddDays(16),
            "Training Center",
            50,
            49.99m
        );

        var response = await _client.PostAsJsonAsync("/api/events", createEventCommand);
        
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var eventIdString = await response.Content.ReadAsStringAsync();
        var eventId = Guid.Parse(eventIdString.Trim('"'));
        Assert.NotEqual(Guid.Empty, eventId);
    }

    [Fact]
    public async Task GetEvent_WithValidId_ShouldReturnEventDetails()
    {
        // First create an event
        var createEventCommand = new CreateEventCommand(
            "Seminar 2024",
            "Educational seminar",
            DateTime.UtcNow.AddDays(20),
            DateTime.UtcNow.AddDays(21),
            "University Hall",
            100,
            29.99m
        );

        var createResponse = await _client.PostAsJsonAsync("/api/events", createEventCommand);
        var eventIdString = await createResponse.Content.ReadAsStringAsync();
        var eventId = Guid.Parse(eventIdString.Trim('"'));

        // Then retrieve it
        var getResponse = await _client.GetAsync($"/api/events/{eventId}");
        
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var eventJson = await getResponse.Content.ReadAsStringAsync();
        var eventData = JsonSerializer.Deserialize<JsonElement>(eventJson);
        
        Assert.Equal("Seminar 2024", eventData.GetProperty("name").GetString());
        Assert.Equal("Educational seminar", eventData.GetProperty("description").GetString());
    }

    [Fact]
    public async Task CreateEvent_WithInvalidData_ShouldReturnBadRequest()
    {
        // Test validation pipeline
        var invalidCommand = new CreateEventCommand(
            "", // Empty name should fail validation
            "Description",
            DateTime.UtcNow.AddDays(-1), // Past date should fail
            DateTime.UtcNow.AddDays(-2), // End before start should fail
            "Location",
            -1, // Negative capacity should fail
            -100); // Negative price should fail

        var response = await _client.PostAsJsonAsync("/api/events", invalidCommand);
        
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAttendee_ForNonExistentEvent_ShouldReturnBadRequest()
    {
        var command = new RegisterAttendeeCommand(
            "John Doe",
            "john.doe@example.com", 
            Guid.NewGuid()); // Non-existent event

        var response = await _client.PostAsJsonAsync("/api/attendees", command);
        
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAttendee_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var command = new RegisterAttendeeCommand(
            "John Doe",
            "invalid-email", // Invalid email format
            Guid.NewGuid());

        var response = await _client.PostAsJsonAsync("/api/attendees", command);
        
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task SubmitFeedback_WithInvalidRating_ShouldReturnBadRequest()
    {
        var command = new SubmitFeedbackCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            10, // Invalid rating (should be 1-5)
            "Great event!");

        var response = await _client.PostAsJsonAsync("/api/feedback", command);
        
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
} 