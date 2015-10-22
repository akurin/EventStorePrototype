using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EventStore
{
    public interface IEventStore
    {
        /// <exception cref="OptimisticConcurrencyException" />
        /// <exception cref="EventStoreException" />
        Task AppendAsync(Guid streamId, ExpectedStreamVersion expectedStreamVersion, IEnumerable<IEvent> events);

        /// <exception cref="EventStoreException" />
        Task<IEnumerable<IEvent>> ReadAsync(Guid streamId);

        /// <exception cref="EventStoreException" />
        Task<IEnumerable<IEvent>> ReadAllAsync();
    }
}