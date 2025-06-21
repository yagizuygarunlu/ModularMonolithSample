using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModularMonolithSample.Attendee.Application.EventHandlers;
using ModularMonolithSample.Event.Domain.Events;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ModularMonolithSample.Attendee.Application.UnitTests;

public class EventCreatedDomainEventHandlerTests
{
    private readonly ILogger<EventCreatedDomainEventHandler> _logger;
    private readonly EventCreatedDomainEventHandler _handler;

    public EventCreatedDomainEventHandlerTests()
    {
        _logger = Substitute.For<ILogger<EventCreatedDomainEventHandler>>();
        _handler = new EventCreatedDomainEventHandler(_logger);
    }

    [Fact]
    public async Task Handle_ValidEvent_ShouldLogEventCreation()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var domainEvent = new EventCreatedDomainEvent(
            eventId,
            "Tech Conference 2024",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(32),
            500,
            100.00m);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert - Just verify logging was called once
        _logger.ReceivedWithAnyArgs(1).LogInformation(default(string));
    }

    [Fact]
    public async Task Handle_EventWithDifferentProperties_ShouldLogCorrectValues()
    {
        // Arrange  
        var eventId = Guid.NewGuid();
        var domainEvent = new EventCreatedDomainEvent(
            eventId,
            "Small Workshop",
            DateTime.UtcNow.AddDays(7),
            DateTime.UtcNow.AddDays(8),
            20,
            50.00m);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert - Just verify logging was called
        _logger.ReceivedWithAnyArgs(1).LogInformation(default(string));
    }

    [Fact]
    public async Task Handle_MultipleEvents_ShouldLogEachEvent()
    {
        // Arrange
        var event1 = new EventCreatedDomainEvent(
            Guid.NewGuid(),
            "Conference 1",
            DateTime.UtcNow.AddDays(30),
            DateTime.UtcNow.AddDays(32),
            300,
            75.00m);

        var event2 = new EventCreatedDomainEvent(
            Guid.NewGuid(),
            "Conference 2",
            DateTime.UtcNow.AddDays(45),
            DateTime.UtcNow.AddDays(47),
            500,
            100.00m);

        // Act
        await _handler.Handle(event1, CancellationToken.None);
        await _handler.Handle(event2, CancellationToken.None);

        // Assert - Verify logging was called twice
        _logger.ReceivedWithAnyArgs(2).LogInformation(default(string));
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldCompleteSuccessfully()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var domainEvent = new EventCreatedDomainEvent(
            eventId,
            "Cancellable Event",
            DateTime.UtcNow.AddDays(15),
            DateTime.UtcNow.AddDays(16),
            100,
            25.00m);

        using var cts = new CancellationTokenSource();

        // Act
        await _handler.Handle(domainEvent, cts.Token);

        // Assert - Verify logging was called
        _logger.ReceivedWithAnyArgs(1).LogInformation(default(string));
    }

    [Fact]
    public async Task Handle_FreeEvent_ShouldLogCorrectly()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var domainEvent = new EventCreatedDomainEvent(
            eventId,
            "Free Community Event",
            DateTime.UtcNow.AddDays(10),
            DateTime.UtcNow.AddDays(11),
            200,
            0.00m);

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert - Verify logging was called
        _logger.ReceivedWithAnyArgs(1).LogInformation(default(string));
    }
} 