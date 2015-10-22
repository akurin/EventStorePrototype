using System;
using System.Collections.Generic;

namespace EventStore.Mongo
{
    internal sealed class Commit
    {
        public Guid StreamId { get; }
        public int StreamVersion { get; }
        public IEnumerable<Guid> EventsIds { get; }

        public Commit(Guid streamId, int streamVersion, IEnumerable<Guid> eventsIds)
        {
            if (eventsIds == null) throw new ArgumentNullException(nameof(eventsIds));

            StreamId = streamId;
            StreamVersion = streamVersion;
            EventsIds = eventsIds;
        }
    }
}