using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public interface IEventStore
    {
        /// <exception cref="OptimisticConcurrencyException" />
        /// <exception cref="EventStoreException" />
        Task AppendAsync(Guid streamId, long expectedStreamLength, IEnumerable<IEvent> events);

        /// <exception cref="EventStoreException" />
        Task<IEnumerable<IEvent>> ReadAsync(Guid streamId, int offset, int limit);

        /// <exception cref="EventStoreException" />
        Task<IEnumerable<IEvent>> ReadAllAsync(int offset, int limit);
    }
}