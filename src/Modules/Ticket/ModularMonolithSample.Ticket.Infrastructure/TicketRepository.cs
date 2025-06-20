using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.BuildingBlocks.Common;
using ModularMonolithSample.Ticket.Domain;
using TicketEntity = ModularMonolithSample.Ticket.Domain.Ticket;

namespace ModularMonolithSample.Ticket.Infrastructure;

public class TicketRepository : ITicketRepository
{
    private readonly TicketDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public TicketRepository(TicketDbContext context, IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<TicketEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<TicketEntity?> GetByTicketNumberAsync(string ticketNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.TicketNumber == ticketNumber, cancellationToken);
    }

    public async Task<IEnumerable<TicketEntity>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.EventId == eventId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TicketEntity>> GetByAttendeeIdAsync(Guid attendeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Tickets
            .Where(t => t.AttendeeId == attendeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TicketEntity ticket, CancellationToken cancellationToken = default)
    {
        await _context.Tickets.AddAsync(ticket, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Note: If Ticket entity had domain events, we would dispatch them here
        // await _domainEventDispatcher.DispatchAsync(ticket.DomainEvents, cancellationToken);
        // ticket.ClearDomainEvents();
    }

    public async Task UpdateAsync(TicketEntity ticket, CancellationToken cancellationToken = default)
    {
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Note: If Ticket entity had domain events, we would dispatch them here
        // await _domainEventDispatcher.DispatchAsync(ticket.DomainEvents, cancellationToken);
        // ticket.ClearDomainEvents();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ticket = await GetByIdAsync(id, cancellationToken);
        if (ticket != null)
        {
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 