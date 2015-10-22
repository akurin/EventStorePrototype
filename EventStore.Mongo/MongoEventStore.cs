using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventStore.Mongo
{
    public class MongoEventStore : IEventStore
    {
        private readonly IMongoCollection<BsonDocument> _commitCollection;
        private readonly CommitSerializer _commitSerializer;
        private readonly EventContainerSerializer _eventContainerSerializer;
        private readonly CommitDao _commitDao;
        private readonly EventDao _eventDao;

        public MongoEventStore(
            IMongoCollection<BsonDocument> commitCollection,
            IMongoCollection<BsonDocument> eventCollection,
            IEventSerializer eventBsonSerializer)
        {
            if (commitCollection == null) throw new ArgumentNullException(nameof(commitCollection));
            if (eventCollection == null) throw new ArgumentNullException(nameof(eventCollection));
            if (eventBsonSerializer == null) throw new ArgumentNullException(nameof(eventBsonSerializer));

            _commitCollection = commitCollection;
            _commitSerializer = new CommitSerializer();
            _eventContainerSerializer = new EventContainerSerializer(eventBsonSerializer);

            _commitDao = new CommitDao(commitCollection);
            _eventDao = new EventDao(eventCollection);
        }

        public async Task AppendAsync(Guid streamId, ExpectedStreamVersion expectedStreamVersion,
            IEnumerable<IEvent> events)
        {
            var identifiedEvents = Identify(events).ToList();
            var eventDocuments = identifiedEvents.Select(_eventContainerSerializer.Serialize);

            var commit = CreateCommit(streamId, expectedStreamVersion, identifiedEvents);
            var commitDocument = _commitSerializer.Serialize(commit);

            await _commitDao.InsertAsync(commitDocument);
            await _eventDao.InsertEventsAsync(eventDocuments);
        }

        private static IEnumerable<EventContainer> Identify(IEnumerable<IEvent> events)
        {
            return events.Select(@event => new EventContainer(@event, Guid.NewGuid()));
        }

        private static Commit CreateCommit(Guid streamId, ExpectedStreamVersion streamVersion,
            IEnumerable<EventContainer> identifiedEvents)
        {
            var nextStreamVersion = streamVersion.AsInt() + 1;
            var eventsIds = identifiedEvents.Select(identifiedEvent => identifiedEvent.Id);
            return new Commit(streamId, nextStreamVersion, eventsIds);
        }

        public async Task<IEnumerable<IEvent>> ReadAsync(Guid streamId)
        {
            var commitDocuments = await _commitDao.FindByAsync(streamId);
            return await GetEventsForAsync(commitDocuments);
        }

        private async Task<IEnumerable<IEvent>> GetEventsForAsync(IEnumerable<BsonDocument> commitDocuments)
        {
            var commits = commitDocuments.Select(_commitSerializer.Deserialize);
            var eventIds = commits.SelectMany(commit => commit.EventsIds);

            var result = new List<IEvent>();

            foreach (var eventId in eventIds)
            {
                var eventDocument = await _eventDao.GetByAsync(eventId);
                var identifiedEvent = _eventContainerSerializer.Deserialize(eventDocument);
                result.Add(identifiedEvent.Event);
            }

            return result;
        }

        public async Task<IEnumerable<IEvent>> ReadAllAsync()
        {
            var commitDocuments = await _commitDao.FindAll();
            return await GetEventsForAsync(commitDocuments);
        }

        public async Task EnsureIndexesAsync()
        {
            await CreateStreamIdAndVersionConstraintAsync();
        }

        private async Task CreateStreamIdAndVersionConstraintAsync()
        {
            var keys = Builders<BsonDocument>.IndexKeys
                .Ascending(CommitSerializer.StreamIdFieldName)
                .Ascending(CommitSerializer.StreamVersionFieldName);

            var keyName = $"{CommitSerializer.StreamIdFieldName}_{CommitSerializer.StreamVersionFieldName}";

            await _commitCollection
                .Indexes.CreateOneAsync(keys, new CreateIndexOptions {Name = keyName, Unique = true});
        }
    }
}