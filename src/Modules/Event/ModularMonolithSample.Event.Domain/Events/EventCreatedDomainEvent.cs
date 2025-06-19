using ModularMonolithSample.BuildingBlocks.Common;

namespace ModularMonolithSample.Event.Domain.Events;

public record EventCreatedDomainEvent(
    Guid EventId,
    string EventName,
    DateTime StartDate,
    DateTime EndDate,
    int Capacity,
    decimal Price) : DomainEvent; 