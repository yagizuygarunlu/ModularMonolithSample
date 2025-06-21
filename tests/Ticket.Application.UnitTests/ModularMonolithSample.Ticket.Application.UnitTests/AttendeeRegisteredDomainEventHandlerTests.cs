using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModularMonolithSample.Attendee.Domain.Events;
using ModularMonolithSample.Ticket.Application.EventHandlers;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ModularMonolithSample.Ticket.Application.UnitTests;

public class AttendeeRegisteredDomainEventHandlerTests
{
    private readonly ILogger<AttendeeRegisteredDomainEventHandler> _logger;
    private readonly AttendeeRegisteredDomainEventHandler _handler;

    public AttendeeRegisteredDomainEventHandlerTests()
    {
        _logger = Substitute.For<ILogger<AttendeeRegisteredDomainEventHandler>>();
        _handler = new AttendeeRegisteredDomainEventHandler(_logger);
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldCompleteSuccessfully()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var attendeeRegisteredEvent = new AttendeeRegisteredDomainEvent(
            attendeeId,
            eventId,
            "john.doe@email.com",
            "John Doe"
        );

        // Act
        var task = _handler.Handle(attendeeRegisteredEvent, CancellationToken.None);

        // Assert
        await task.ShouldNotThrowAsync();
        task.IsCompletedSuccessfully.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_EventWithDifferentData_ShouldCompleteSuccessfully()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var attendeeRegisteredEvent = new AttendeeRegisteredDomainEvent(
            attendeeId,
            eventId,
            "alice.johnson@example.com",
            "Alice Johnson"
        );

        // Act
        var task = _handler.Handle(attendeeRegisteredEvent, CancellationToken.None);

        // Assert
        await task.ShouldNotThrowAsync();
        task.IsCompletedSuccessfully.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_MultipleEvents_ShouldHandleEachSuccessfully()
    {
        // Arrange
        var firstEventId = Guid.NewGuid();
        var firstAttendeeId = Guid.NewGuid();
        var firstEvent = new AttendeeRegisteredDomainEvent(
            firstAttendeeId,
            firstEventId,
            "bob.wilson@email.com",
            "Bob Wilson"
        );

        var secondEventId = Guid.NewGuid();
        var secondAttendeeId = Guid.NewGuid();
        var secondEvent = new AttendeeRegisteredDomainEvent(
            secondAttendeeId,
            secondEventId,
            "carol.davis@email.com",
            "Carol Davis"
        );

        // Act & Assert
        var firstTask = _handler.Handle(firstEvent, CancellationToken.None);
        await firstTask.ShouldNotThrowAsync();
        firstTask.IsCompletedSuccessfully.ShouldBeTrue();

        var secondTask = _handler.Handle(secondEvent, CancellationToken.None);
        await secondTask.ShouldNotThrowAsync();
        secondTask.IsCompletedSuccessfully.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_CancellationRequested_ShouldStillComplete()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var attendeeRegisteredEvent = new AttendeeRegisteredDomainEvent(
            attendeeId,
            eventId,
            "david.brown@email.com",
            "David Brown"
        );

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act
        var task = _handler.Handle(attendeeRegisteredEvent, cancellationTokenSource.Token);

        // Assert
        await task.ShouldNotThrowAsync();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public async Task Handle_EmptyOrNullNames_ShouldStillCompleteSuccessfully(string? firstName)
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var attendeeId = Guid.NewGuid();
        var fullName = $"{firstName} LastName";
        var attendeeRegisteredEvent = new AttendeeRegisteredDomainEvent(
            attendeeId,
            eventId,
            "test@email.com",
            fullName
        );

        // Act
        var task = _handler.Handle(attendeeRegisteredEvent, CancellationToken.None);

        // Assert
        await task.ShouldNotThrowAsync();
        task.IsCompletedSuccessfully.ShouldBeTrue();
    }

    [Fact]
    public async Task Handle_EventWithEmptyGuids_ShouldStillCompleteSuccessfully()
    {
        // Arrange
        var attendeeRegisteredEvent = new AttendeeRegisteredDomainEvent(
            Guid.Empty,
            Guid.Empty,
            "test@email.com",
            "Test User"
        );

        // Act
        var task = _handler.Handle(attendeeRegisteredEvent, CancellationToken.None);

        // Assert
        await task.ShouldNotThrowAsync();
        task.IsCompletedSuccessfully.ShouldBeTrue();
    }
} 
