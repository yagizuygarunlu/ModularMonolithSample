using System;

namespace ModularMonolithSample.Attendee.Domain;

public class Attendee
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public Guid EventId { get; private set; }
    public DateTime RegistrationDate { get; private set; }

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
    }

    public void UpdateDetails(string firstName, string lastName, string email, string phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PhoneNumber = phoneNumber;
    }
} 