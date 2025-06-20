using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
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
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CompleteEventLifecycle_ShouldWork()
    {
        // 1. Create Event
        var createEventCommand = new CreateEventCommand(
            "Tech Conference 2024",
            "A conference about latest technology trends",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(31), 
            "Istanbul Convention Center",
            100,
            299.99m);

        var createEventResponse = await _client.PostAsJsonAsync("/api/events", createEventCommand);
        createEventResponse.EnsureSuccessStatusCode();
        
        var eventIdJson = await createEventResponse.Content.ReadAsStringAsync();
        var eventId = JsonSerializer.Deserialize<Guid>(eventIdJson);
        
        Assert.NotEqual(Guid.Empty, eventId);

        // 2. Verify Event Created
        var getEventResponse = await _client.GetAsync($"/api/events/{eventId}");
        getEventResponse.EnsureSuccessStatusCode();
        
        var eventDto = await getEventResponse.Content.ReadFromJsonAsync<EventDto>();
        Assert.NotNull(eventDto);
        Assert.Equal("Tech Conference 2024", eventDto.Name);
        Assert.Equal(100, eventDto.Capacity);

        // 3. Register Attendee
        var registerAttendeeCommand = new RegisterAttendeeCommand(
            "John",
            "Doe", 
            "john.doe@example.com",
            "+1234567890",
            eventId);

        var registerResponse = await _client.PostAsJsonAsync("/api/attendees", registerAttendeeCommand);
        registerResponse.EnsureSuccessStatusCode();
        
        var attendeeIdJson = await registerResponse.Content.ReadAsStringAsync();
        var attendeeId = JsonSerializer.Deserialize<Guid>(attendeeIdJson);
        
        Assert.NotEqual(Guid.Empty, attendeeId);

        // 4. Issue Ticket
        var issueTicketCommand = new IssueTicketCommand(eventId, attendeeId);
        
        var ticketResponse = await _client.PostAsJsonAsync("/api/tickets", issueTicketCommand);
        ticketResponse.EnsureSuccessStatusCode();
        
        var ticketIdJson = await ticketResponse.Content.ReadAsStringAsync();
        var ticketId = JsonSerializer.Deserialize<Guid>(ticketIdJson);
        
        Assert.NotEqual(Guid.Empty, ticketId);

        // 5. Submit Feedback (This will fail due to ticket validation requirement)
        var submitFeedbackCommand = new SubmitFeedbackCommand(
            eventId,
            attendeeId,
            5,
            "Excellent conference! Learned a lot.");

        var feedbackResponse = await _client.PostAsJsonAsync("/api/feedback", submitFeedbackCommand);
        
        // We expect this to fail since ticket is not validated
        Assert.False(feedbackResponse.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, feedbackResponse.StatusCode);
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
            "John",
            "Doe",
            "john.doe@example.com", 
            "+1234567890",
            Guid.NewGuid()); // Non-existent event

        var response = await _client.PostAsJsonAsync("/api/attendees", command);
        
        Assert.False(response.IsSuccessStatusCode);
        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RegisterAttendee_WithInvalidEmail_ShouldReturnBadRequest()
    {
        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            "invalid-email", // Invalid email format
            "+1234567890",
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