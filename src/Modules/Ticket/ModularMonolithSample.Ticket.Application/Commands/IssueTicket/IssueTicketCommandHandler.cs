using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ModularMonolithSample.Attendee.Domain;
using ModularMonolithSample.Event.Domain;
using ModularMonolithSample.Ticket.Domain;
using TicketEntity = ModularMonolithSample.Ticket.Domain.Ticket;

namespace ModularMonolithSample.Ticket.Application.Commands.IssueTicket;

public class IssueTicketCommandHandler : IRequestHandler<IssueTicketCommand, Guid>
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IAttendeeRepository _attendeeRepository;

    public IssueTicketCommandHandler(
        ITicketRepository ticketRepository,
        IEventRepository eventRepository,
        IAttendeeRepository attendeeRepository)
    {
        _ticketRepository = ticketRepository;
        _eventRepository = eventRepository;
        _attendeeRepository = attendeeRepository;
    }

    public async Task<Guid> Handle(IssueTicketCommand request, CancellationToken cancellationToken)
    {
        // Validate event exists
        var @event = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);
        if (@event == null)
        {
            throw new InvalidOperationException($"Event with ID {request.EventId} not found.");
        }

        // Validate attendee exists and is registered for the event
        var attendee = await _attendeeRepository.GetByIdAsync(request.AttendeeId, cancellationToken);
        if (attendee == null)
        {
            throw new InvalidOperationException($"Attendee with ID {request.AttendeeId} not found.");
        }

        if (attendee.EventId != request.EventId)
        {
            throw new InvalidOperationException($"Attendee is not registered for this event.");
        }

        // Check if attendee already has a ticket
        var existingTickets = await _ticketRepository.GetByAttendeeIdAsync(request.AttendeeId, cancellationToken);
        foreach (var existingTicket in existingTickets)
        {
            if (existingTicket.Status == TicketStatus.Issued || existingTicket.Status == TicketStatus.Validated)
            {
                throw new InvalidOperationException("Attendee already has an active ticket for this event.");
            }
        }

        // Generate unique ticket number
        var ticketNumber = GenerateTicketNumber();

        // Create and save ticket
        var newTicket = new TicketEntity(
            ticketNumber,
            request.EventId,
            request.AttendeeId,
            @event.Price);

        await _ticketRepository.AddAsync(newTicket, cancellationToken);
        return newTicket.Id;
    }

    private string GenerateTicketNumber()
    {
        return $"TICK-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
} 
