using System;
using System.Threading;
using System.Threading.Tasks;
using ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;
using ModularMonolithSample.Attendee.Domain;
using ModularMonolithSample.Event.Domain;
using NSubstitute;
using Shouldly;
using Xunit;
using AttendeeEntity = ModularMonolithSample.Attendee.Domain.Attendee;
using EventEntity = ModularMonolithSample.Event.Domain.Event;

namespace ModularMonolithSample.Attendee.Application.UnitTests;

public class RegisterAttendeeCommandHandlerTests
{
    private readonly IAttendeeRepository _attendeeRepository;
    private readonly IEventRepository _eventRepository;
    private readonly RegisterAttendeeCommandHandler _handler;

    public RegisterAttendeeCommandHandlerTests()
    {
        _attendeeRepository = Substitute.For<IAttendeeRepository>();
        _eventRepository = Substitute.For<IEventRepository>();
        _handler = new RegisterAttendeeCommandHandler(_attendeeRepository, _eventRepository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldRegisterAttendeeSuccessfully()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new EventEntity(
            "Tech Conference",
            "Annual tech event",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(32),
            "Convention Center",
            500,
            100.00m
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetAttendeeCountForEventAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(50);

        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            "john.doe@email.com",
            "+1234567890",
            eventId
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        await _attendeeRepository.Received(1).AddAsync(Arg.Any<AttendeeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPassCorrectValuesToRepository()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var firstName = "Jane";
        var lastName = "Smith";
        var email = "jane.smith@email.com";
        var phoneNumber = "+9876543210";

        var eventEntity = new EventEntity(
            "Workshop",
            "Learning workshop",
            DateTime.UtcNow.AddDays(15),
            DateTime.UtcNow.AddDays(16),
            "Training Center",
            100,
            75.00m
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetAttendeeCountForEventAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(25);

        var command = new RegisterAttendeeCommand(
            firstName,
            lastName,
            email,
            phoneNumber,
            eventId
        );

        AttendeeEntity? capturedAttendee = null;
        await _attendeeRepository.AddAsync(Arg.Do<AttendeeEntity>(a => capturedAttendee = a), Arg.Any<CancellationToken>());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        capturedAttendee.ShouldNotBeNull();
        capturedAttendee.FirstName.ShouldBe(firstName);
        capturedAttendee.LastName.ShouldBe(lastName);
        capturedAttendee.Email.ShouldBe(email);
        capturedAttendee.PhoneNumber.ShouldBe(phoneNumber);
        capturedAttendee.EventId.ShouldBe(eventId);
        capturedAttendee.Id.ShouldBe(result);
    }

    [Fact]
    public async Task Handle_EventNotFound_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns((EventEntity?)null);

        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            "john.doe@email.com",
            "+1234567890",
            eventId
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe($"Event with ID {eventId} not found.");
        await _attendeeRepository.DidNotReceive().AddAsync(Arg.Any<AttendeeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EventAtCapacity_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new EventEntity(
            "Small Workshop",
            "Limited capacity workshop",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(11),
            "Small Room",
            10, // Small capacity
            50.00m
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetAttendeeCountForEventAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(10); // Already at capacity

        var command = new RegisterAttendeeCommand(
            "John",
            "Doe",
            "john.doe@email.com",
            "+1234567890",
            eventId
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe($"Event {eventEntity.Name} has reached its capacity of {eventEntity.Capacity} attendees.");
        await _attendeeRepository.DidNotReceive().AddAsync(Arg.Any<AttendeeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EventNearCapacity_ShouldRegisterSuccessfully()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new EventEntity(
            "Popular Event",
            "Almost full event",
            DateTime.UtcNow.AddDays(20),
            DateTime.UtcNow.AddDays(22),
            "Large Venue",
            100,
            150.00m
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetAttendeeCountForEventAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(99); // One spot remaining

        var command = new RegisterAttendeeCommand(
            "Lucky",
            "Attendee",
            "lucky@email.com",
            "+1111111111",
            eventId
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShouldNotBe(Guid.Empty);
        await _attendeeRepository.Received(1).AddAsync(Arg.Any<AttendeeEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventEntity = new EventEntity(
            "Test Event",
            "Test Description",
            DateTime.UtcNow.AddDays(5),
            DateTime.UtcNow.AddDays(6),
            "Test Location",
            50,
            25.00m
        );

        _eventRepository.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(eventEntity);
        _attendeeRepository.GetAttendeeCountForEventAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(10);

        _attendeeRepository.When(x => x.AddAsync(Arg.Any<AttendeeEntity>(), Arg.Any<CancellationToken>()))
            .Do(x => throw new InvalidOperationException("Database connection failed"));

        var command = new RegisterAttendeeCommand(
            "Test",
            "User",
            "test@email.com",
            "+1234567890",
            eventId
        );

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None)
        );
        
        exception.Message.ShouldBe("Database connection failed");
    }
} 