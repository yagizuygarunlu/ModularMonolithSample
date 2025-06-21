using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ModularMonolithSample.BuildingBlocks.Common;
using ModularMonolithSample.Feedback.Domain;
using FeedbackEntity = ModularMonolithSample.Feedback.Domain.Feedback;

namespace ModularMonolithSample.Feedback.Infrastructure;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly FeedbackDbContext _context;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    public FeedbackRepository(FeedbackDbContext context, IDomainEventDispatcher domainEventDispatcher)
    {
        _context = context;
        _domainEventDispatcher = domainEventDispatcher;
    }

    public async Task<FeedbackEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Feedbacks.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<IEnumerable<FeedbackEntity>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _context.Feedbacks
            .Where(f => f.EventId == eventId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeedbackEntity>> GetByAttendeeIdAsync(Guid attendeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Feedbacks
            .Where(f => f.AttendeeId == attendeeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<FeedbackEntity?> GetByEventAndAttendeeIdAsync(Guid eventId, Guid attendeeId, CancellationToken cancellationToken = default)
    {
        return await _context.Feedbacks
            .FirstOrDefaultAsync(f => f.EventId == eventId && f.AttendeeId == attendeeId, cancellationToken);
    }

    public async Task<double> GetAverageRatingForEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _context.Feedbacks
            .Where(f => f.EventId == eventId)
            .ToListAsync(cancellationToken);
            
        return feedbacks.Any() ? feedbacks.Average(f => f.Rating) : 0.0;
    }

    public async Task AddAsync(FeedbackEntity feedback, CancellationToken cancellationToken = default)
    {
        await _context.Feedbacks.AddAsync(feedback, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Note: If Feedback entity had domain events, we would dispatch them here
        // await _domainEventDispatcher.DispatchAsync(feedback.DomainEvents, cancellationToken);
        // feedback.ClearDomainEvents();
    }

    public async Task UpdateAsync(FeedbackEntity feedback, CancellationToken cancellationToken = default)
    {
        _context.Feedbacks.Update(feedback);
        await _context.SaveChangesAsync(cancellationToken);
        
        // Note: If Feedback entity had domain events, we would dispatch them here
        // await _domainEventDispatcher.DispatchAsync(feedback.DomainEvents, cancellationToken);
        // feedback.ClearDomainEvents();
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
