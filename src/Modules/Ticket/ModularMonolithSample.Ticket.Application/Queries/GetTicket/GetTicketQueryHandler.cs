using MediatR;
using ModularMonolithSample.Ticket.Domain;

namespace ModularMonolithSample.Ticket.Application.Queries.GetTicket;

public class GetTicketQueryHandler : IRequestHandler<GetTicketQuery, TicketDto?>
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketQueryHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<TicketDto?> Handle(GetTicketQuery request, CancellationToken cancellationToken)
    {
        var ticket = await _ticketRepository.GetByIdAsync(request.Id, cancellationToken);
        
        if (ticket == null)
            return null;

        return new TicketDto(
            ticket.Id,
            ticket.TicketNumber,
            ticket.EventId,
            ticket.AttendeeId,
            ticket.Price,
            ticket.Status,
            ticket.IssueDate);
    }
} 