using MediatR;
using ModularMonolithSample.Event.Application.Queries.GetEvent;
using ModularMonolithSample.Event.Domain;

namespace ModularMonolithSample.Event.Application.Queries.GetAllEvents;

public class GetAllEventsQueryHandler : IRequestHandler<GetAllEventsQuery, IEnumerable<EventDto>>
{
    private readonly IEventRepository _eventRepository;

    public GetAllEventsQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IEnumerable<EventDto>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _eventRepository.GetAllAsync(cancellationToken);
        
        return events.Select(e => new EventDto(
            e.Id,
            e.Name,
            e.Description,
            e.StartDate,
            e.EndDate,
            e.Location,
            e.Capacity,
            e.Price));
    }
} 