using System;
using System.Collections.Generic;

namespace EventStore.Mongo
{
    internal sealed class Commit
    {
        public Guid StreamId { get; }
        public long IndexInStream { get; }
        public long IndexInAllStreams { get; }
        public long EventIndexInStream { get; }
        public long EventIndexInAllStreams { get; }
        public IEnumerable<Guid> EventIds { get; }

        public Commit(Guid streamId, long indexInStream, long indexInAllStreams, long eventIndexInStream,
            long eventIndexInAllStreams, IEnumerable<Guid> eventIds)
        {
            StreamId = streamId;
            IndexInStream = indexInStream;
            IndexInAllStreams = indexInAllStreams;
            EventIndexInStream = eventIndexInStream;
            EventIndexInAllStreams = eventIndexInAllStreams;
            EventIds = eventIds;
        }
    }
}