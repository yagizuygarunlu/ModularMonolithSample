using MediatR;

namespace ModularMonolithSample.Event.Application.Commands.CreateEvent;

public record CreateEventCommand(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    int Capacity,
    decimal Price) : IRequest<Guid>; 
