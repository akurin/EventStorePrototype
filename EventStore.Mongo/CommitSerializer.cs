using System;
using System.Linq;
using MongoDB.Bson;

namespace EventStore.Mongo
{
    internal sealed class CommitSerializer
    {
        public static readonly string StreamIdFieldName = "streamId";
        public static readonly string IndexInStreamFieldName = "indexInStream";
        public static readonly string IndexInAllStreamsFileName = "indexInAllStreams";
        public static readonly string EventIndexInStreamFieldName = "eventIndexInStreamStartsFrom";
        public static readonly string EventIndexInAllStreamsFieldName = "eventIndexInAllStreamsStartsFrom";
        public static readonly string EventIdsFieldName = "eventIds";

        public BsonDocument Serialize(Commit commit)
        {
            if (commit == null) throw new ArgumentNullException(nameof(commit));

            return new BsonDocument
            {
                {StreamIdFieldName, commit.StreamId},
                {IndexInStreamFieldName, commit.IndexInStream},
                {IndexInAllStreamsFileName, commit.IndexInAllStreams},
                {EventIndexInStreamFieldName, commit.EventIndexInStreamStartsFrom},
                {EventIndexInAllStreamsFieldName, commit.EventIndexInAllStreamsStartsFrom},
                {EventIdsFieldName, new BsonArray(commit.EventIds)}
            };
        }

        public Commit Deserialize(BsonDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var streamId = document[StreamIdFieldName].AsGuidOrEventStoreException();
            var indexInStream = document[IndexInStreamFieldName].AsInt64OrEventStoreException();
            var indexInAllStreams = document[IndexInAllStreamsFileName].AsInt64OrEventStoreException();
            var eventIndexInStreamStartsFrom = document[EventIndexInStreamFieldName].AsInt64OrEventStoreException();
            var eventIndexInAllStreamsStartsFrom = document[EventIndexInAllStreamsFieldName].AsInt64OrEventStoreException();
            var eventIds = document[EventIdsFieldName]
                .AsBsonArrayOrEventStoreException()
                .Select(bsonValue => bsonValue.AsGuidOrEventStoreException())
                .ToList();

            return new Commit(
                streamId:streamId,
                indexInStream: indexInStream,
                indexInAllStreams: indexInAllStreams,
                eventIndexInStreamStartsFrom: eventIndexInStreamStartsFrom,
                eventIndexInAllStreamsStartsFrom: eventIndexInAllStreamsStartsFrom,
                eventIds: eventIds);
        }
    }
}