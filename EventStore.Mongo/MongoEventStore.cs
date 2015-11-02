using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Optional;

namespace EventStore.Mongo
{
    public sealed class MongoEventStore : IEventStore
    {
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

            _commitSerializer = new CommitSerializer();
            _eventContainerSerializer = new EventContainerSerializer(eventBsonSerializer);

            _commitDao = new CommitDao(commitCollection);
            _eventDao = new EventDao(eventCollection);
        }

        public async Task AppendAsync(Guid streamId, long expectedStreamLength, IEnumerable<IEvent> events)
        {
            if (expectedStreamLength < 0)
                throw new ArgumentException("Should be greater than or equal to zero", nameof(expectedStreamLength));

            if (events == null) throw new ArgumentNullException(nameof(events));

            var lastCommitInStream = await GetLastCommitInStreamAsync(streamId);
            var actualStreamLength = CalculateActualStreamLength(lastCommitInStream);

            if (actualStreamLength != expectedStreamLength)
                ThrowOptimisticConcurrencyException();

            var eventContainers = events
                .Select(@event => new EventContainer(@event, Guid.NewGuid()))
                .ToList();

            await InsertAsync(eventContainers);

            InsertCommitResult insertCommitResult;
            do
            {
                var lastCommit = await GetLastCommit();
                var commit = CreateCommit(streamId, lastCommitInStream, lastCommit, eventContainers);
                insertCommitResult = await TryInsertAsync(commit);
            } while (insertCommitResult == InsertCommitResult.DuplicateCommitWithSameIndexInAllStreams);

            if (insertCommitResult == InsertCommitResult.DuplicateCommitWithSameIndexInStream)
                ThrowOptimisticConcurrencyException();
        }

        private async Task<Option<Commit>> GetLastCommitInStreamAsync(Guid streamId)
        {
            var lastCommitDocumentInStream = await _commitDao.GetLastInStreamAsync(streamId);
            return lastCommitDocumentInStream.Map(_commitSerializer.Deserialize);
        }

        private static long CalculateActualStreamLength(Option<Commit> lastCommitInStream)
        {
            return lastCommitInStream
                .Map(commit => commit.EventIndexInStream + commit.EventIds.Count())
                .ValueOr(0);
        }

        private static void ThrowOptimisticConcurrencyException()
        {
            throw new OptimisticConcurrencyException("Stream has been changed");
        }

        private async Task InsertAsync(IEnumerable<EventContainer> eventContainers)
        {
            var documents = eventContainers.Select(_eventContainerSerializer.Serialize);
            await _eventDao.InsertEventsAsync(documents);
        }

        private async Task<Option<Commit>> GetLastCommit()
        {
            var lastCommitDocument = await _commitDao.GetLastAsync();
            return lastCommitDocument.Map(_commitSerializer.Deserialize);
        }

        private static Commit CreateCommit(
            Guid streamId,
            Option<Commit> lastCommitInStream,
            Option<Commit> lastCommit,
            IEnumerable<EventContainer> identifiedEvents)
        {
            var indexInStream = lastCommitInStream
                .Map(commit => commit.IndexInStream + 1)
                .ValueOr(0);

            var indexInAllStreams = lastCommit
                .Map(commit => commit.IndexInAllStreams + 1)
                .ValueOr(0);

            var eventsIds = identifiedEvents.Select(identifiedEvent => identifiedEvent.Id);

            var eventIndexInStream = lastCommitInStream
                .Map(commit => commit.EventIndexInStream + commit.EventIds.Count())
                .ValueOr(0);

            var eventIndexInAllStreams = lastCommit
                .Map(commit => commit.EventIndexInAllStreams + commit.EventIds.Count())
                .ValueOr(0);

            return new Commit(
                streamId: streamId,
                indexInStream: indexInStream,
                indexInAllStreams: indexInAllStreams,
                eventIndexInStream: eventIndexInStream,
                eventIndexInAllStreams: eventIndexInAllStreams,
                eventIds: eventsIds);
        }

        private async Task<InsertCommitResult> TryInsertAsync(Commit commit)
        {
            var commitDocument = _commitSerializer.Serialize(commit);
            return await _commitDao.InsertAsync(commitDocument);
        }

        public async Task<IEnumerable<IEvent>> ReadAsync(Guid streamId, int offset, int limit)
        {
            var result = new List<IEvent>(limit);

            using (var cursor = await _commitDao.GetCommitsInStreamAsync(streamId, offset))
            {
                while (await cursor.MoveNextAsync())
                {
                    var currentDocuments = cursor.Current;

                    foreach (var currentDocument in currentDocuments)
                    {
                        var commit = _commitSerializer.Deserialize(currentDocument);
                        foreach (var eventId in commit.EventIds)
                        {
                            var eventDocument = await _eventDao.GetByAsync(eventId);
                            var eventContainer = _eventContainerSerializer.Deserialize(eventDocument);
                            result.Add(eventContainer.Event);

                            if (result.Count >= limit)
                                break;
                        }
                    }
                }
            }

            return result;
        }

        private async Task<IEnumerable<IEvent>> GetEventsForAsync(IEnumerable<BsonDocument> commitDocuments)
        {
            var commits = commitDocuments.Select(_commitSerializer.Deserialize);
            var eventIds = commits.SelectMany(commit => commit.EventIds);

            var result = new List<IEvent>();

            foreach (var eventId in eventIds)
            {
                var eventDocument = await _eventDao.GetByAsync(eventId);
                var identifiedEvent = _eventContainerSerializer.Deserialize(eventDocument);
                result.Add(identifiedEvent.Event);
            }

            return result;
        }

        public async Task<IEnumerable<IEvent>> ReadAllAsync(int offset, int limit) // TODO: refactor duplicate code
        {
            var result = new List<IEvent>(limit);

            using (var cursor = await _commitDao.FindInAllStreamsAsync(offset))
            {
                while (await cursor.MoveNextAsync())
                {
                    var currentDocuments = cursor.Current;

                    foreach (var currentDocument in currentDocuments)
                    {
                        var commit = _commitSerializer.Deserialize(currentDocument);
                        foreach (var eventId in commit.EventIds)
                        {
                            var eventDocument = await _eventDao.GetByAsync(eventId);
                            var eventContainer = _eventContainerSerializer.Deserialize(eventDocument);
                            result.Add(eventContainer.Event);

                            if (result.Count >= limit)
                                return result;
                        }
                    }
                }
            }

            return result;
        }
    }
}