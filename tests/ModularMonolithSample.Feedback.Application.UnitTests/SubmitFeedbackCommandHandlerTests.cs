using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModularMonolithSample.Attendee.Domain;
using ModularMonolithSample.Event.Domain;
using ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;
using ModularMonolithSample.Feedback.Domain;
using ModularMonolithSample.Ticket.Domain;
using NSubstitute;
using Shouldly;
using Xunit;
using AttendeeEntity = ModularMonolithSample.Attendee.Domain.Attendee;
using EventEntity = ModularMonolithSample.Event.Domain.Event;
using FeedbackEntity = ModularMonolithSample.Feedback.Domain.Feedback;
using TicketEntity = ModularMonolithSample.Ticket.Domain.Ticket;

namespace ModularMonolithSample.Feedback.Application.UnitTests;

public class SubmitFeedbackCommandHandlerTests
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IAttendeeRepository _attendeeRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly SubmitFeedbackCommandHandler _handler;

    public SubmitFeedbackCommandHandlerTests()
    {
        _feedbackRepository = Substitute.For<IFeedbackRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _attendeeRepository = Substitute.For<IAttendeeRepository>();
        _ticketRepository = Substitute.For<ITicketRepository>();
        _handler = new SubmitFeedbackCommandHandler(
            _feedbackRepository,
            _eventRepository,
            _attendeeRepository,
            _ticketRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateNewFeedback()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Great event!");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);
        var attendee = new AttendeeEntity("John Doe", "john.doe@email.com", eventId);
        var ticket = new TicketEntity("TICKET-001", eventId, attendeeId, 50.0m);
        ticket.Validate(); // Set status to Validated

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(attendee);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(new[] { ticket });
        _feedbackRepository.GetByEventAndAttendeeIdAsync(eventId, attendeeId, Arg.Any<CancellationToken>()).Returns((FeedbackEntity?)null);

        FeedbackEntity? capturedFeedback = null;
        _feedbackRepository
            .AddAsync(Arg.Do<FeedbackEntity>(f => capturedFeedback = f), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedFeedback.ShouldNotBeNull();
        capturedFeedback.EventId.ShouldBe(eventId);
        capturedFeedback.AttendeeId.ShouldBe(attendeeId);
        capturedFeedback.Rating.ShouldBe(5);
        capturedFeedback.Comment.ShouldBe("Great event!");

        await _feedbackRepository.Received(1).AddAsync(Arg.Any<FeedbackEntity>(), Arg.Any<CancellationToken>());
        await _feedbackRepository.DidNotReceive().UpdateAsync(Arg.Any<FeedbackEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ExistingFeedback_ShouldUpdateFeedback()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 4, "Updated feedback!");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);
        var attendee = new AttendeeEntity("John Doe", "john.doe@email.com", eventId);
        var ticket = new TicketEntity("TICKET-002", eventId, attendeeId, 50.0m);
        ticket.Validate();
        var existingFeedback = new FeedbackEntity(eventId, attendeeId, 3, "Initial feedback");

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(attendee);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(new[] { ticket });
        _feedbackRepository.GetByEventAndAttendeeIdAsync(eventId, attendeeId, Arg.Any<CancellationToken>()).Returns(existingFeedback);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldBe(existingFeedback.Id);
        existingFeedback.Rating.ShouldBe(4);
        existingFeedback.Comment.ShouldBe("Updated feedback!");

        await _feedbackRepository.Received(1).UpdateAsync(existingFeedback, Arg.Any<CancellationToken>());
        await _feedbackRepository.DidNotReceive().AddAsync(Arg.Any<FeedbackEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EventNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Great event!");

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns((EventEntity?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe($"Event with ID {eventId} not found.");
    }

    [Fact]
    public async Task Handle_AttendeeNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Great event!");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns((AttendeeEntity?)null);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe($"Attendee with ID {attendeeId} not found.");
    }

    [Fact]
    public async Task Handle_AttendeeNotRegisteredForEvent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var differentEventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Great event!");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);
        var attendee = new AttendeeEntity("John Doe", "john.doe@email.com", differentEventId); // Different event

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(attendee);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe("Attendee is not registered for this event.");
    }

    [Fact]
    public async Task Handle_NoValidatedTicket_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Great event!");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);
        var attendee = new AttendeeEntity("John Doe", "john.doe@email.com", eventId);
        var ticket = new TicketEntity("TICKET-003", eventId, attendeeId, 50.0m); // Not validated

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(attendee);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(new[] { ticket });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe("Attendee must have a validated ticket to submit feedback.");
    }

    [Fact]
    public async Task Handle_TicketForDifferentEvent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var differentEventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Great event!");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);
        var attendee = new AttendeeEntity("John Doe", "john.doe@email.com", eventId);
        var ticket = new TicketEntity("TICKET-004", differentEventId, attendeeId, 50.0m); // Different event
        ticket.Validate();

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(attendee);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(new[] { ticket });

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe("Attendee must have a validated ticket to submit feedback.");
    }

    [Fact]
    public async Task Handle_MultipleTicketsWithOneValidated_ShouldCreateFeedback()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Great event!");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);
        var attendee = new AttendeeEntity("John Doe", "john.doe@email.com", eventId);
        
        var ticket1 = new TicketEntity("TICKET-005", eventId, attendeeId, 50.0m); // Not validated
        var ticket2 = new TicketEntity("TICKET-006", eventId, attendeeId, 50.0m); // Will be validated
        ticket2.Validate();

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(attendee);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(new[] { ticket1, ticket2 });
        _feedbackRepository.GetByEventAndAttendeeIdAsync(eventId, attendeeId, Arg.Any<CancellationToken>()).Returns((FeedbackEntity?)null);

        FeedbackEntity? capturedFeedback = null;
        _feedbackRepository
            .AddAsync(Arg.Do<FeedbackEntity>(f => capturedFeedback = f), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedFeedback.ShouldNotBeNull();
        await _feedbackRepository.Received(1).AddAsync(Arg.Any<FeedbackEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyTicketCollection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Great event!");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);
        var attendee = new AttendeeEntity("John Doe", "john.doe@email.com", eventId);

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(attendee);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(new List<TicketEntity>());

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        exception.Message.ShouldBe("Attendee must have a validated ticket to submit feedback.");
    }

    [Fact]
    public async Task Handle_CancellationRequested_ShouldStillComplete()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, 5, "Great event!");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);
        var attendee = new AttendeeEntity("John Doe", "john.doe@email.com", eventId);
        var ticket = new TicketEntity("TICKET-007", eventId, attendeeId, 50.0m);
        ticket.Validate();

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(attendee);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(new[] { ticket });
        _feedbackRepository.GetByEventAndAttendeeIdAsync(eventId, attendeeId, Arg.Any<CancellationToken>()).Returns((FeedbackEntity?)null);

        // Configure to simulate completion even with cancellation requested
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        FeedbackEntity? capturedFeedback = null;
        _feedbackRepository
            .AddAsync(Arg.Do<FeedbackEntity>(f => capturedFeedback = f), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, cancellationTokenSource.Token);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedFeedback.ShouldNotBeNull();
        await _feedbackRepository.Received(1).AddAsync(Arg.Any<FeedbackEntity>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Handle_DifferentValidRatings_ShouldCreateFeedback(int rating)
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var command = new SubmitFeedbackCommand(eventId, attendeeId, rating, $"Rating {rating} feedback");

        var @event = new EventEntity("Test Event", "Test Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), "Test Location", 100, 50.0m);
        var attendee = new AttendeeEntity("John Doe", "john.doe@email.com", eventId);
        var ticket = new TicketEntity($"TICKET-{rating:D3}", eventId, attendeeId, 50.0m);
        ticket.Validate();

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>()).Returns(@event);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(attendee);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>()).Returns(new[] { ticket });
        _feedbackRepository.GetByEventAndAttendeeIdAsync(eventId, attendeeId, Arg.Any<CancellationToken>()).Returns((FeedbackEntity?)null);

        FeedbackEntity? capturedFeedback = null;
        _feedbackRepository
            .AddAsync(Arg.Do<FeedbackEntity>(f => capturedFeedback = f), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedFeedback.ShouldNotBeNull();
        capturedFeedback.Rating.ShouldBe(rating);
        capturedFeedback.Comment.ShouldBe($"Rating {rating} feedback");
        await _feedbackRepository.Received(1).AddAsync(Arg.Any<FeedbackEntity>(), Arg.Any<CancellationToken>());
    }
} 
