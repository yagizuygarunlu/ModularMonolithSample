using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModularMonolithSample.Feedback.Domain;

public interface IFeedbackRepository
{
    Task<Feedback?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Feedback>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Feedback>> GetByAttendeeIdAsync(Guid attendeeId, CancellationToken cancellationToken = default);
    Task<Feedback?> GetByEventAndAttendeeIdAsync(Guid eventId, Guid attendeeId, CancellationToken cancellationToken = default);
    Task<double> GetAverageRatingForEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task AddAsync(Feedback feedback, CancellationToken cancellationToken = default);
    Task UpdateAsync(Feedback feedback, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
} 
