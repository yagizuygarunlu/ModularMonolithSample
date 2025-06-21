using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ModularMonolithSample.Attendee.Application.EventHandlers;
using ModularMonolithSample.Event.Domain.Events;
using NSubstitute;
using Xunit;

namespace ModularMonolithSample.Attendee.Application.UnitTests.EventHandlers;

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
    public async Task Handle_Should_Log_Information()
    {
        // Arrange
        var domainEvent = new EventCreatedDomainEvent(
            Guid.NewGuid(),
            "Test Event",
            DateTime.UtcNow,
            DateTime.UtcNow.AddHours(1),
            100,
            25.50m
        );

        // Act
        await _handler.Handle(domainEvent, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString().Contains($"Event created: {domainEvent.EventId}")),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }
}

// Wrapper to satisfy MediatR's INotification requirement if EventCreatedDomainEvent isn't one directly
public class NotificationWrapper<T> : MediatR.INotification
{
    public T DomainEvent { get; }
    public NotificationWrapper(T domainEvent) => DomainEvent = domainEvent;
} 