using ModularMonolithSample.BuildingBlocks.Common;

namespace ModularMonolithSample.Attendee.Domain.Events;

public record AttendeeRegisteredDomainEvent(
    Guid AttendeeId,
    Guid EventId,
    string AttendeeEmail,
    string AttendeeName) : DomainEvent; 