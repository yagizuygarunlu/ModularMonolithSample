using MediatR;
using Microsoft.Extensions.Logging;
using ModularMonolithSample.Attendee.Domain.Events;

namespace ModularMonolithSample.Ticket.Application.EventHandlers;

public class AttendeeRegisteredDomainEventHandler : INotificationHandler<AttendeeRegisteredDomainEvent>
{
    private readonly ILogger<AttendeeRegisteredDomainEventHandler> _logger;

    public AttendeeRegisteredDomainEventHandler(ILogger<AttendeeRegisteredDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(AttendeeRegisteredDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attendee registered: {AttendeeId} - {AttendeeName} for event {EventId}",
            notification.AttendeeId,
            notification.AttendeeName,
            notification.EventId);

        // Here you could implement business logic like:
        // - Auto-generate tickets for paid events
        // - Send welcome emails
        // - Update capacity tracking
        
        await Task.CompletedTask;
    }
} 