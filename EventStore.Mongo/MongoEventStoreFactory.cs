using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventStore.Mongo
{
    public sealed class MongoEventStoreFactory
    {
        public async Task<IEventStore> CreateAsync(string connectionString, string databaseName,
            IEventSerializer eventSerializer = null)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (databaseName == null) throw new ArgumentNullException(nameof(databaseName));

            var mongoClient = new MongoClient(connectionString);
            var database = mongoClient.GetDatabase(databaseName);

            var eventCollection = GetEventCollection(database);
            var commitCollection = GetCommitCollection(database);

            var mongoEventStore = new MongoEventStore(commitCollection, eventCollection,
                eventSerializer ?? CreateDefaultEventBsonDocumentSerializer());

            var indexCreator = new IndexCreator(commitCollection);
            await indexCreator.EnsureIndexesAsync();

            return mongoEventStore;
        }

        private static IMongoCollection<BsonDocument> GetEventCollection(IMongoDatabase database)
        {
            return database.GetCollection<BsonDocument>("events");
        }

        private static IMongoCollection<BsonDocument> GetCommitCollection(IMongoDatabase database)
        {
            return database.GetCollection<BsonDocument>("commits");
        }

        private static IEventSerializer CreateDefaultEventBsonDocumentSerializer()
        {
            return new CSharpMongoDriverEventSerializer();
        }
    }
}