using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ModularMonolithSample.Event.Domain;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Event @event, CancellationToken cancellationToken = default);
    Task UpdateAsync(Event @event, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
} 