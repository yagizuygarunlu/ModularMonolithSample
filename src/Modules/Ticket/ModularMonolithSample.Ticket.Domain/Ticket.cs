using System;

namespace ModularMonolithSample.Ticket.Domain;

public class Ticket
{
    public Guid Id { get; private set; }
    public string TicketNumber { get; private set; }
    public Guid EventId { get; private set; }
    public Guid AttendeeId { get; private set; }
    public decimal Price { get; private set; }
    public DateTime IssueDate { get; private set; }
    public TicketStatus Status { get; private set; }

    private Ticket() { } // For EF Core

    public Ticket(string ticketNumber, Guid eventId, Guid attendeeId, decimal price)
    {
        Id = Guid.NewGuid();
        TicketNumber = ticketNumber;
        EventId = eventId;
        AttendeeId = attendeeId;
        Price = price;
        IssueDate = DateTime.UtcNow;
        Status = TicketStatus.Issued;
    }

    public void Cancel()
    {
        if (Status != TicketStatus.Issued)
        {
            throw new InvalidOperationException("Only issued tickets can be cancelled.");
        }
        Status = TicketStatus.Cancelled;
    }

    public void Validate()
    {
        if (Status != TicketStatus.Issued)
        {
            throw new InvalidOperationException("Only issued tickets can be validated.");
        }
        Status = TicketStatus.Validated;
    }
}

public enum TicketStatus
{
    Issued,
    Validated,
    Cancelled
} 