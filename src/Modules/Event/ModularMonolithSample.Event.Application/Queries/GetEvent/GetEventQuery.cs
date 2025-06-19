using MediatR;

namespace ModularMonolithSample.Event.Application.Queries.GetEvent;

public record GetEventQuery(Guid EventId) : IRequest<EventDto?>; 