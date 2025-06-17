using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.Feedback.Domain;

namespace ModularMonolithSample.Feedback.Infrastructure;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly FeedbackDbContext _context;

    public FeedbackRepository(FeedbackDbContext context)
    {
        _context = context;
    }

    public async Task<Feedback?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Feedbacks.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<Feedback>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Feedbacks
            .Where(f => f.EventId == eventId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Feedback>> GetByAttendeeIdAsync(Guid attendeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Feedbacks
            .Where(f => f.AttendeeId == attendeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Feedback?> GetByEventAndAttendeeIdAsync(Guid eventId, Guid attendeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.EventId == eventId && f.AttendeeId == attendeeId, cancellationToken);
    }

    public async Task<double> GetAverageRatingForEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Feedbacks
            .Where(f => f.EventId == eventId)
            .AverageAsync(f => f.Rating, cancellationToken);
    }

    public async Task AddAsync(Feedback feedback, CancellationToken cancellationToken = default)
    {
        await _context.Feedbacks.AddAsync(feedback, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Feedback feedback, CancellationToken cancellationToken = default)
    {
        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var feedback = await GetByIdAsync(id, cancellationToken);
        if (feedback != null)
        {
            _context.Feedbacks.Remove(feedback);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
} 