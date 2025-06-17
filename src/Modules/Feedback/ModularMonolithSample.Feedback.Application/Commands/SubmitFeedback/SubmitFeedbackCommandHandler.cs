using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ModularMonolithSample.Attendee.Domain;
using ModularMonolithSample.Event.Domain;
using ModularMonolithSample.Feedback.Domain;
using ModularMonolithSample.Ticket.Domain;

namespace ModularMonolithSample.Feedback.Application.Commands.SubmitFeedback;

public class SubmitFeedbackCommandHandler : IRequestHandler<SubmitFeedbackCommand, Guid>
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IAttendeeRepository _attendeeRepository;
    private readonly ITicketRepository _ticketRepository;

    public SubmitFeedbackCommandHandler(
        IFeedbackRepository feedbackRepository,
        IEventRepository eventRepository,
        IAttendeeRepository attendeeRepository,
        ITicketRepository ticketRepository)
    {
        _feedbackRepository = feedbackRepository;
        _eventRepository = eventRepository;
        _attendeeRepository = attendeeRepository;
        _ticketRepository = ticketRepository;
    }

    public async Task<Guid> Handle(SubmitFeedbackCommand request, CancellationToken cancellationToken)
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

        // Check if attendee has a validated ticket
        var tickets = await _ticketRepository.GetByAttendeeIdAsync(request.AttendeeId, cancellationToken);
        var hasValidatedTicket = false;
        foreach (var ticket in tickets)
        {
            if (ticket.EventId == request.EventId && ticket.Status == TicketStatus.Validated)
            {
                hasValidatedTicket = true;
                break;
            }
        }

        if (!hasValidatedTicket)
        {
            throw new InvalidOperationException("Attendee must have a validated ticket to submit feedback.");
        }

        // Check if feedback already exists
        var existingFeedback = await _feedbackRepository.GetByEventAndAttendeeIdAsync(
            request.EventId,
            request.AttendeeId,
            cancellationToken);

        if (existingFeedback != null)
        {
            existingFeedback.UpdateFeedback(request.Rating, request.Comment);
            await _feedbackRepository.UpdateAsync(existingFeedback, cancellationToken);
            return existingFeedback.Id;
        }

        // Create and save new feedback
        var feedback = new Feedback(
            request.EventId,
            request.AttendeeId,
            request.Rating,
            request.Comment);

        await _feedbackRepository.AddAsync(feedback, cancellationToken);
        return feedback.Id;
    }
} 