using System;
using System.Collections.Generic;

namespace EventStore.Mongo
{
    internal sealed class Commit
    {
        public Guid StreamId { get; }
        public long IndexInStream { get; }
        public long IndexInAllStreams { get; }
        public long EventIndexInStreamStartsFrom { get; }
        public long EventIndexInAllStreamsStartsFrom { get; }
        public IEnumerable<Guid> EventIds { get; }

        public Commit(
            Guid streamId,
            long indexInStream,
            long indexInAllStreams,
            long eventIndexInStreamStartsFrom,
            long eventIndexInAllStreamsStartsFrom,
            IEnumerable<Guid> eventIds)
        {
            StreamId = streamId;
            IndexInStream = indexInStream;
            IndexInAllStreams = indexInAllStreams;
            EventIndexInStreamStartsFrom = eventIndexInStreamStartsFrom;
            EventIndexInAllStreamsStartsFrom = eventIndexInAllStreamsStartsFrom;
            EventIds = eventIds;
        }
    }
}