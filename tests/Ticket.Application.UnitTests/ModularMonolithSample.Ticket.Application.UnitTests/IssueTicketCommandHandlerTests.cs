using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ModularMonolithSample.Attendee.Domain;
using ModularMonolithSample.Event.Domain;
using ModularMonolithSample.Ticket.Application.Commands.IssueTicket;
using ModularMonolithSample.Ticket.Domain;
using NSubstitute;
using Shouldly;
using Xunit;
using AttendeeEntity = ModularMonolithSample.Attendee.Domain.Attendee;
using EventEntity = ModularMonolithSample.Event.Domain.Event;
using TicketEntity = ModularMonolithSample.Ticket.Domain.Ticket;

namespace ModularMonolithSample.Ticket.Application.UnitTests;

public class IssueTicketCommandHandlerTests
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IAttendeeRepository _attendeeRepository;
    private readonly IssueTicketCommandHandler _handler;

    public IssueTicketCommandHandlerTests()
    {
        _ticketRepository = Substitute.For<ITicketRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _attendeeRepository = Substitute.For<IAttendeeRepository>();
        _handler = new IssueTicketCommandHandler(_ticketRepository, _eventRepository, _attendeeRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldIssueTicketSuccessfully()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var eventPrice = 100.00m;
        
        var eventEntity = new EventEntity(
            "Tech Conference",
            "Annual tech event",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(32),
            "Convention Center",
            500,
            eventPrice
        );

        var attendeeEntity = new AttendeeEntity(
            "John Doe",
            "john.doe@email.com",
            eventId
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(attendeeEntity);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(new List<TicketEntity>());

        var command = new IssueTicketCommand(eventId, attendeeId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        await _ticketRepository.Received(1).AddAsync(Arg.Any<TicketEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateTicketWithCorrectProperties()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var eventPrice = 150.00m;
        
        var eventEntity = new EventEntity(
            "Workshop",
            "Learning workshop",
            DateTime.UtcNow.AddDays(15),
            DateTime.UtcNow.AddDays(16),
            "Training Center",
            100,
            eventPrice
        );

        var attendeeEntity = new AttendeeEntity(
            "Jane Smith",
            "jane.smith@email.com",
            eventId
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(attendeeEntity);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(new List<TicketEntity>());

        var command = new IssueTicketCommand(eventId, attendeeId);

        TicketEntity? capturedTicket = null;
        _ticketRepository
            .AddAsync(Arg.Do<TicketEntity>(t => capturedTicket = t), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedTicket.ShouldNotBeNull();
        capturedTicket.EventId.ShouldBe(eventId);
        capturedTicket.AttendeeId.ShouldBe(attendeeId);
        capturedTicket.Price.ShouldBe(eventPrice);
        capturedTicket.Status.ShouldBe(TicketStatus.Issued);
        capturedTicket.TicketNumber.ShouldNotBeNullOrEmpty();
        capturedTicket.TicketNumber.ShouldStartWith("TICK-");
        capturedTicket.Id.ShouldBe(result);
    }

    [Fact]
    public async Task Handle_EventNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns((EventEntity?)null);

        var command = new IssueTicketCommand(eventId, attendeeId);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe($"Event with ID {eventId} not found.");
        await _ticketRepository.DidNotReceive().AddAsync(Arg.Any<TicketEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AttendeeNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        
        var eventEntity = new EventEntity(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(11),
            "Test Location",
            100,
            50.00m
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns((AttendeeEntity?)null);

        var command = new IssueTicketCommand(eventId, attendeeId);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe($"Attendee with ID {attendeeId} not found.");
        await _ticketRepository.DidNotReceive().AddAsync(Arg.Any<TicketEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AttendeeNotRegisteredForEvent_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var differentEventId = Guid.NewGuid();
        
        var eventEntity = new EventEntity(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(11),
            "Test Location",
            100,
            50.00m
        );

        var attendeeEntity = new AttendeeEntity(
            "John Doe",
            "john.doe@email.com",
            differentEventId
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(attendeeEntity);

        var command = new IssueTicketCommand(eventId, attendeeId);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe("Attendee is not registered for this event.");
        await _ticketRepository.DidNotReceive().AddAsync(Arg.Any<TicketEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AttendeeAlreadyHasIssuedTicket_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        
        var eventEntity = new EventEntity(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(11),
            "Test Location",
            100,
            50.00m
        );

        var attendeeEntity = new AttendeeEntity("John Doe", "john.doe@email.com", eventId
        );

        var existingTicket = new TicketEntity(
            "TICK-12345678",
            eventId,
            attendeeId,
            50.00m
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(attendeeEntity);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(new List<TicketEntity> { existingTicket });

        var command = new IssueTicketCommand(eventId, attendeeId);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe("Attendee already has an active ticket for this event.");
        await _ticketRepository.DidNotReceive().AddAsync(Arg.Any<TicketEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AttendeeAlreadyHasValidatedTicket_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        
        var eventEntity = new EventEntity(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(11),
            "Test Location",
            100,
            50.00m
        );

        var attendeeEntity = new AttendeeEntity("John Doe", "john.doe@email.com", eventId
        );

        var existingTicket = new TicketEntity(
            "TICK-12345678",
            eventId,
            attendeeId,
            50.00m
        );
        existingTicket.Validate();

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(attendeeEntity);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(new List<TicketEntity> { existingTicket });

        var command = new IssueTicketCommand(eventId, attendeeId);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe("Attendee already has an active ticket for this event.");
        await _ticketRepository.DidNotReceive().AddAsync(Arg.Any<TicketEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AttendeeHasCancelledTicket_ShouldIssueNewTicket()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var eventPrice = 75.00m;
        
        var eventEntity = new EventEntity(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(11),
            "Test Location",
            100,
            eventPrice
        );

        var attendeeEntity = new AttendeeEntity("John Doe", "john.doe@email.com", eventId
        );

        var cancelledTicket = new TicketEntity(
            "TICK-12345678",
            eventId,
            attendeeId,
            eventPrice
        );
        cancelledTicket.Cancel();

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(attendeeEntity);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(new List<TicketEntity> { cancelledTicket });

        var command = new IssueTicketCommand(eventId, attendeeId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        await _ticketRepository.Received(1).AddAsync(Arg.Any<TicketEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        
        var eventEntity = new EventEntity(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(11),
            "Test Location",
            100,
            50.00m
        );

        var attendeeEntity = new AttendeeEntity("John Doe", "john.doe@email.com", eventId
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetByIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(attendeeEntity);
        _ticketRepository.GetByAttendeeIdAsync(attendeeId, Arg.Any<CancellationToken>())
            .Returns(new List<TicketEntity>());

        _ticketRepository.When(x => x.AddAsync(Arg.Any<TicketEntity>(), Arg.Any<CancellationToken>()))
            .Do(x => throw new InvalidOperationException("Database connection failed"));

        var command = new IssueTicketCommand(eventId, attendeeId);

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe("Database connection failed");
    }
} 

