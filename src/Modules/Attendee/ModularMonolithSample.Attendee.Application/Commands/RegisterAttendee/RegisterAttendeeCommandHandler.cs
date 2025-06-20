using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ModularMonolithSample.Attendee.Domain;
using ModularMonolithSample.Event.Domain;
using AttendeeEntity = ModularMonolithSample.Attendee.Domain.Attendee;

namespace ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;

public class RegisterAttendeeCommandHandler : IRequestHandler<RegisterAttendeeCommand, Guid>
{
    private readonly IAttendeeRepository _attendeeRepository;
    private readonly IEventRepository _eventRepository;

    public RegisterAttendeeCommandHandler(
        IAttendeeRepository attendeeRepository,
        IEventRepository eventRepository)
    {
        _attendeeRepository = attendeeRepository;
        _eventRepository = eventRepository;
    }

    public async Task<Guid> Handle(RegisterAttendeeCommand request, CancellationToken cancellationToken)
    {
        // Validate event exists
        var @event = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (@event == null)
        {
            throw new InvalidOperationException($"Event with ID {request.EventId} not found.");
        }

        // Check event capacity
        var currentAttendeeCount = await _attendeeRepository.GetAttendeeCountForEventAsync(request.EventId, cancellationToken);
        if (currentAttendeeCount >= @event.Capacity)
        {
            throw new InvalidOperationException($"Event {@event.Name} has reached its capacity of {@event.Capacity} attendees.");
        }

        // Create and save attendee
        var attendee = new AttendeeEntity(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.EventId);

        await _attendeeRepository.AddAsync(attendee, cancellationToken);
        return attendee.Id;
    }
} 