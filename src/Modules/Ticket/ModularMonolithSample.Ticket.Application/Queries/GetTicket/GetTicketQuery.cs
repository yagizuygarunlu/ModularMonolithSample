using MediatR;

namespace ModularMonolithSample.Ticket.Application.Queries.GetTicket;

public record GetTicketQuery(Guid Id) : IRequest<TicketDto?>; 