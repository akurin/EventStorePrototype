using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventStore.Mongo
{
    internal sealed class EventDao
    {
        private readonly IMongoCollection<BsonDocument> _eventCollection;

        public EventDao(IMongoCollection<BsonDocument> eventCollection)
        {
            if (eventCollection == null) throw new ArgumentNullException(nameof(eventCollection));

            _eventCollection = eventCollection;
        }

        public async Task InsertEventsAsync(IEnumerable<BsonDocument> eventDocuments)
        {
            if (eventDocuments == null) throw new ArgumentNullException(nameof(eventDocuments));

            var insertWriteModels = eventDocuments
                .Select(eventDocument => new InsertOneModel<BsonDocument>(eventDocument));

            await _eventCollection.BulkWriteAsync(insertWriteModels);
        }

        public async Task<BsonDocument> GetByAsync(Guid eventId)
        {
            var findEventCursor = await _eventCollection.FindAsync(document => document["_id"] == eventId);
            var findEventResult = await findEventCursor.ToListAsync();

            if (findEventResult.Count != 1)
            {
                var message = $"Query to event collection returned {findEventResult.Count} documents, " +
                              "expected exactly one";

                throw new EventStoreException(message);
            }

            return findEventResult.First();
        }
    }
}