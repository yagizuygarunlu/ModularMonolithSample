using MediatR;

namespace ModularMonolithSample.BuildingBlocks.Common;

public abstract record DomainEvent(Guid Id, DateTime OccurredOn) : INotification
{
    protected DomainEvent() : this(Guid.NewGuid(), DateTime.UtcNow) { }
} 