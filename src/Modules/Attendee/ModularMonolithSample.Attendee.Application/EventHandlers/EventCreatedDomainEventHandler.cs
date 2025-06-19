using MediatR;
using Microsoft.Extensions.Logging;
using ModularMonolithSample.Event.Domain.Events;

namespace ModularMonolithSample.Attendee.Application.EventHandlers;

public class EventCreatedDomainEventHandler : INotificationHandler<EventCreatedDomainEvent>
{
    private readonly ILogger<EventCreatedDomainEventHandler> _logger;

    public EventCreatedDomainEventHandler(ILogger<EventCreatedDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(EventCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Event created: {EventId} - {EventName} with capacity {Capacity}",
            notification.EventId,
            notification.EventName,
            notification.Capacity);

        // Here you could implement business logic like:
        // - Send notifications to interested parties
        // - Initialize attendee-related data
        // - Update analytics
        
        await Task.CompletedTask;
    }
} 