using ModularMonolithSample.BuildingBlocks.Common;

namespace ModularMonolithSample.Attendee.Domain;

public class Attendee
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Email { get; private set; }
    public Guid EventId { get; private set; }
    public DateTime RegistrationDate { get; private set; }

    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    // Private constructor for EF Core
    private Attendee()
    {
        Name = string.Empty;
        Email = string.Empty;
    }

    public Attendee(string name, string email, Guid eventId)
    {
        Id = Guid.NewGuid();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        EventId = eventId;
        RegistrationDate = DateTime.UtcNow;

        // Raise domain event
        _domainEvents.Add(new Events.AttendeeRegisteredDomainEvent(Id, EventId, Email, Name));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
} 