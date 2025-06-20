using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.BuildingBlocks.Common;
using ModularMonolithSample.Event.Domain;
using EventEntity = ModularMonolithSample.Event.Domain.Event;

namespace ModularMonolithSample.Event.Infrastructure;

public class EventRepository : IEventRepository
{
    private readonly EventDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public EventRepository(EventDbContext context, IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<EventEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<EventEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(EventEntity @event, CancellationToken cancellationToken = default)
    {
        await _context.Events.AddAsync(@event, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Dispatch domain events
        await _domainEventDispatcher.DispatchAsync(@event.DomainEvents, cancellationToken);
        @event.ClearDomainEvents();
    }

    public async Task UpdateAsync(EventEntity @event, CancellationToken cancellationToken = default)
    {
        _context.Events.Update(@event);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Dispatch domain events
        await _domainEventDispatcher.DispatchAsync(@event.DomainEvents, cancellationToken);
        @event.ClearDomainEvents();
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var @event = await GetByIdAsync(id, cancellationToken);
        if (@event != null)
        {
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 