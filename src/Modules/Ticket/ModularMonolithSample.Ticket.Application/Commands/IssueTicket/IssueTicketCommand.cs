using System;
using MediatR;

namespace ModularMonolithSample.Ticket.Application.Commands.IssueTicket;

public record IssueTicketCommand(
    Guid EventId,
    Guid AttendeeId) : IRequest<Guid>; 