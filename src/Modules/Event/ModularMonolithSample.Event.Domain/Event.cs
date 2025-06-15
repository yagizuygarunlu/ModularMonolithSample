using System;

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
} 