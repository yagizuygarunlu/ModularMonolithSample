using ModularMonolithSample.Ticket.Domain;

namespace ModularMonolithSample.Ticket.Application.Queries.GetTicket;

public record TicketDto(
    Guid Id,
    string TicketNumber,
    Guid EventId,
    Guid AttendeeId,
    decimal Price,
    TicketStatus Status,
    DateTime IssueDate); 