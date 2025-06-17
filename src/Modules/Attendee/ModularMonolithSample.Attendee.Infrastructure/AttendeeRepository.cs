using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.Attendee.Domain;

namespace ModularMonolithSample.Attendee.Infrastructure;

public class AttendeeRepository : IAttendeeRepository
{
    private readonly AttendeeDbContext _context;

    public AttendeeRepository(AttendeeDbContext context)
    {
        _context = context;
    }

    public async Task<Attendee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Attendees.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Attendee>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Attendees
            .Where(a => a.EventId == eventId)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetAttendeeCountForEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Attendees
            .CountAsync(a => a.EventId == eventId, cancellationToken);
    }

    public async Task AddAsync(Attendee attendee, CancellationToken cancellationToken = default)
    {
        await _context.Attendees.AddAsync(attendee, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Attendee attendee, CancellationToken cancellationToken = default)
    {
        _context.Attendees.Update(attendee);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var attendee = await GetByIdAsync(id, cancellationToken);
        if (attendee != null)
        {
            _context.Attendees.Remove(attendee);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 