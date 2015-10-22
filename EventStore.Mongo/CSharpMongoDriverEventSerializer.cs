using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace EventStore.Mongo
{
    public sealed class CSharpMongoDriverEventSerializer : IEventSerializer
    {
        public BsonDocument Serizalize(IEvent @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var document = new BsonDocument();

            using (var writer = new BsonDocumentWriter(document))
            {
                BsonSerializer.Serialize(writer, @event);
            }

            return document;
        }

        public IEvent Deserialize(BsonDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            return BsonSerializer.Deserialize<IEvent>(document);
        }
    }
}