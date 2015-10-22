using System;
using MongoDB.Bson;

namespace EventStore.Mongo
{
    public interface IEventSerializer
    {
        BsonDocument Serizalize(IEvent @event);
        IEvent Deserialize(BsonDocument document);
    }
}