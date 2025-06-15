using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using ModularMonolithSample.Event.Domain;

namespace ModularMonolithSample.Event.Application.Commands.CreateEvent;

public class CreateEventCommandHandler : IRequestHandler<CreateEventCommand, Guid>
{
    private readonly IEventRepository _eventRepository;

    public CreateEventCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var @event = new Event(
            request.Name,
            request.Description,
            request.StartDate,
            request.EndDate,
            request.Location,
            request.Capacity,
            request.Price);

        await _eventRepository.AddAsync(@event, cancellationToken);
        return @event.Id;
    }
} 