using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Optional;

namespace EventStore.Mongo
{
    internal sealed class CommitDao
    {
        private readonly IMongoCollection<BsonDocument> _commitCollection;

        public CommitDao(IMongoCollection<BsonDocument> commitCollection)
        {
            if (commitCollection == null) throw new ArgumentNullException(nameof(commitCollection));

            _commitCollection = commitCollection;
        }

        public async Task<InsertCommitResult> InsertAsync(BsonDocument commitDocument)
        {
            if (commitDocument == null) throw new ArgumentNullException(nameof(commitDocument));

            try
            {
                await _commitCollection.InsertOneAsync(commitDocument);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category != ServerErrorCategory.DuplicateKey) throw;

                var indexName = MongoExceptionAnalyzer.ExtactViolatedIndexNameFrom(ex);

                if (indexName == IndexCreator.IndexInAllStreamsIndexName)
                    return InsertCommitResult.DuplicateCommitWithSameIndexInAllStreams;

                if (indexName == IndexCreator.StreamIdAndIndexInStreamName)
                    return InsertCommitResult.DuplicateCommitWithSameIndexInStream;

                throw new EventStoreException("Unexpected index name");
            }

            return InsertCommitResult.Success;
        }

        public async Task<IAsyncCursor<BsonDocument>> GetCommitsInStreamAsync(Guid streamId, long startIndex)
        {
            var filter = Builders<BsonDocument>.Filter.Where(
                commit =>
                    commit[CommitSerializer.StreamIdFieldName] == streamId &&
                    commit[CommitSerializer.EventIndexInStream] >= startIndex);

            return await _commitCollection.FindAsync(filter);
        }

        public async Task<IAsyncCursor<BsonDocument>> FindInAllStreamsAsync(long startIndex)
        {
            var filter = Builders<BsonDocument>.Filter.Where(
                commit => commit[CommitSerializer.EventIndexInAllStreams] >= startIndex);

            return await _commitCollection.FindAsync(filter);
        }

        public async Task<Option<BsonDocument>> GetLastAsync()
        {
            var sortDefinition = Builders<BsonDocument>.Sort.Descending("_id");
            var lastOrEmpty = await _commitCollection
                .Find("{ }")
                .Sort(sortDefinition)
                .Limit(1)
                .ToListAsync();

            var result = lastOrEmpty.SingleOrDefault();
            return result == null
                ? Option.None<BsonDocument>()
                : Option.Some(result);
        }

        public async Task<Option<BsonDocument>> GetLastInStreamAsync(Guid streamId)
        {
            var filter = Builders<BsonDocument>.Filter.Where(
                commit => commit[CommitSerializer.StreamIdFieldName] == streamId);
            var sortDefinition = Builders<BsonDocument>.Sort.Descending("_id");

            var lastOrEmpty = await _commitCollection
                .Find(filter)
                .Sort(sortDefinition)
                .Limit(1)
                .ToListAsync();

            var result = lastOrEmpty.SingleOrDefault();
            return result == null
                ? Option.None<BsonDocument>()
                : Option.Some(result);
        }

        public async Task<long> CountAsync(Guid streamId)
        {
            var filter = Builders<BsonDocument>.Filter.Where(
                commit => commit[CommitSerializer.StreamIdFieldName] == streamId);

            return await _commitCollection.CountAsync(filter);
        }
    }
}