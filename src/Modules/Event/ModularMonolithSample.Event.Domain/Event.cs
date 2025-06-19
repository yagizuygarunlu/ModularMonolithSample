using System;
using System.Collections.Generic;
using ModularMonolithSample.BuildingBlocks.Common;
using ModularMonolithSample.Event.Domain.Events;

namespace ModularMonolithSample.Event.Domain;

public class Event
{
    private readonly List<DomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Location { get; private set; }
    public int Capacity { get; private set; }
    public decimal Price { get; private set; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Event() { } // For EF Core

    public Event(string name, string description, DateTime startDate, DateTime endDate, string location, int capacity, decimal price)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        Location = location;
        Capacity = capacity;
        Price = price;

        // Raise domain event
        _domainEvents.Add(new EventCreatedDomainEvent(Id, Name, StartDate, EndDate, Capacity, Price));
    }

    public void UpdateDetails(string name, string description, DateTime startDate, DateTime endDate, string location, int capacity, decimal price)
    {
        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        Location = location;
        Capacity = capacity;
        Price = price;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
} 