using MediatR;

namespace ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;

public record RegisterAttendeeCommand(
    string Name,
    string Email,
    Guid EventId) : IRequest<Guid>; 