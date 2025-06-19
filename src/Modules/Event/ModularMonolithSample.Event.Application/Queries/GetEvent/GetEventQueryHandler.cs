using MediatR;
using ModularMonolithSample.Event.Domain;

namespace ModularMonolithSample.Event.Application.Queries.GetEvent;

public class GetEventQueryHandler : IRequestHandler<GetEventQuery, EventDto?>
{
    private readonly IEventRepository _eventRepository;

    public GetEventQueryHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<EventDto?> Handle(GetEventQuery request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.EventId, cancellationToken);

        if (@event == null)
            return null;

        return new EventDto(
            @event.Id,
            @event.Name,
            @event.Description,
            @event.StartDate,
            @event.EndDate,
            @event.Location,
            @event.Capacity,
            @event.Price);
    }
} 