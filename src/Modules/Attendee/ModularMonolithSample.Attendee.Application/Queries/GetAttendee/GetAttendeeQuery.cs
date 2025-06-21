using MediatR;

namespace ModularMonolithSample.Attendee.Application.Queries.GetAttendee;

public record GetAttendeeQuery(Guid Id) : IRequest<AttendeeDto?>; 