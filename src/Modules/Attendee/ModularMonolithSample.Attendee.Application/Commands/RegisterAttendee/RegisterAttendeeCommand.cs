using System;
using MediatR;

namespace ModularMonolithSample.Attendee.Application.Commands.RegisterAttendee;

public record RegisterAttendeeCommand(
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    Guid EventId) : IRequest<Guid>; 