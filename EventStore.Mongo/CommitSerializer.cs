using System;
using System.Linq;
using MongoDB.Bson;

namespace EventStore.Mongo
{
    internal sealed class CommitSerializer
    {
        public static readonly string StreamIdFieldName = "streamId";
        public static readonly string StreamVersionFieldName = "streamVersion";
        public static readonly string EventIdsFieldName = "eventIds";

        public BsonDocument Serialize(Commit commit)
        {
            if (commit == null) throw new ArgumentNullException(nameof(commit));

            return new BsonDocument
            {
                {StreamIdFieldName, commit.StreamId},
                {StreamVersionFieldName, commit.StreamVersion},
                {EventIdsFieldName, new BsonArray(commit.EventsIds)}
            };
        }

        public Commit Deserialize(BsonDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var streamId = document[StreamIdFieldName].AsGuidOrEventStoreException();
            var streamVersion = document[StreamVersionFieldName].AsInt32OrEventStoreException();
            var eventIds = document[EventIdsFieldName]
                .AsBsonArrayOrEventStoreException()
                .Select(bsonValue => bsonValue.AsGuidOrEventStoreException())
                .ToList();

            return new Commit(streamId, streamVersion, eventIds);
        }
    }
}