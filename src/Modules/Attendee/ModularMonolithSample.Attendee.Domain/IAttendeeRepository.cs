using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModularMonolithSample.Attendee.Domain;

public interface IAttendeeRepository
{
    Task<Attendee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Attendee>> GetByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<int> GetAttendeeCountForEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task AddAsync(Attendee attendee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Attendee attendee, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
} 