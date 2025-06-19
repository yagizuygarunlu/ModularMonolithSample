using System;
using System.Collections.Generic;
using ModularMonolithSample.Attendee.Domain.Events;
using ModularMonolithSample.BuildingBlocks.Common;

namespace ModularMonolithSample.Attendee.Domain;

public class Attendee
{
    private readonly List<DomainEvent> _domainEvents = new();

    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public Guid EventId { get; private set; }
    public DateTime RegistrationDate { get; private set; }

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Attendee() { } // For EF Core

    public Attendee(string firstName, string lastName, string email, string phoneNumber, Guid eventId)
    {
        Id = Guid.NewGuid();
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
        EventId = eventId;
        RegistrationDate = DateTime.UtcNow;

        // Raise domain event
        _domainEvents.Add(new AttendeeRegisteredDomainEvent(Id, EventId, Email, $"{FirstName} {LastName}"));
    }

    public void UpdateDetails(string firstName, string lastName, string email, string phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
} 