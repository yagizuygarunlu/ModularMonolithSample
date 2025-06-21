using System.Collections.Generic;
using ModularMonolithSample.BuildingBlocks.Common;
using ModularMonolithSample.Event.Domain.Events;

namespace ModularMonolithSample.Event.Domain;

public class Event
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Location { get; private set; }
    public int Capacity { get; private set; }
    public decimal Price { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Private constructor for EF Core
    private Event()
    {
        Name = string.Empty;
        Description = string.Empty;
        Location = string.Empty;
    }

    public Event(string name, string description, DateTime startDate, DateTime endDate, string location, int capacity, decimal price)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        StartDate = startDate;
        EndDate = endDate;
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Capacity = capacity;
        Price = price;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;

        // Raise domain event
        _domainEvents.Add(new EventCreatedDomainEvent(Id, Name, StartDate, EndDate, Capacity, Price));
    }

    public void UpdateDetails(string name, string description, string location, int capacity, decimal price)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Capacity = capacity;
        Price = price;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
} 
