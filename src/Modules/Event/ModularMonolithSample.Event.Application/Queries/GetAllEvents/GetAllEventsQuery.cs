using MediatR;
using ModularMonolithSample.Event.Application.Queries.GetEvent;

namespace ModularMonolithSample.Event.Application.Queries.GetAllEvents;

public record GetAllEventsQuery : IRequest<IEnumerable<EventDto>>; 