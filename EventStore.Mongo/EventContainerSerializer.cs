using System;
using MongoDB.Bson;

namespace EventStore.Mongo
{
    internal class EventContainerSerializer
    {
        private readonly IEventSerializer _eventSerializer;

        public EventContainerSerializer(IEventSerializer eventSerializer)
        {
            if (eventSerializer == null) throw new ArgumentNullException(nameof(eventSerializer));

            _eventSerializer = eventSerializer;
        }

        public BsonDocument Serialize(EventContainer eventContainer)
        {
            if (eventContainer == null) throw new ArgumentNullException(nameof(eventContainer));

            var document = _eventSerializer.Serizalize(eventContainer.Event);
            document["_id"] = eventContainer.Id;
            return document;
        }

        public EventContainer Deserialize(BsonDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            var id = document["_id"].AsGuidOrEventStoreException();
            var eventDocumentWithoutId = DeleteIdFrom(document);
            var @event = _eventSerializer.Deserialize(eventDocumentWithoutId);
            return new EventContainer(@event, id);
        }

        private static BsonDocument DeleteIdFrom(BsonValue document)
        {
            var result = (BsonDocument) document.Clone();
            result.Remove("_id");
            return result;
        }
    }
}