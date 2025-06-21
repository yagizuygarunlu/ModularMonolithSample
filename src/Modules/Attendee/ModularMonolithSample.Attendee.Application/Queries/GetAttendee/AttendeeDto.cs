namespace ModularMonolithSample.Attendee.Application.Queries.GetAttendee;

public record AttendeeDto(
    Guid Id,
    string Name,
    string Email,
    Guid EventId,
    DateTime RegisteredAt); 