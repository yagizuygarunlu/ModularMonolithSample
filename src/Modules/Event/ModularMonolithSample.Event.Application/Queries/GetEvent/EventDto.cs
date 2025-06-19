namespace ModularMonolithSample.Event.Application.Queries.GetEvent;

public record EventDto(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    string Location,
    int Capacity,
    decimal Price); 