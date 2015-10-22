using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

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

        public async Task InsertAsync(BsonDocument commitDocument)
        {
            if (commitDocument == null) throw new ArgumentNullException(nameof(commitDocument));

            try
            {
                await _commitCollection.InsertOneAsync(commitDocument);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                    throw new OptimisticConcurrencyException("The stream has already changed");

                throw;
            }
        }

        public async Task<IEnumerable<BsonDocument>> FindByAsync(Guid streamId)
        {
            var filter = Builders<BsonDocument>.Filter.Where(
                commit => commit[CommitSerializer.StreamIdFieldName] == streamId);

            var commitsCursor = _commitCollection.Find(filter);
            return await commitsCursor.ToListAsync();
        }

        public async Task<IEnumerable<BsonDocument>> FindAll()
        {
            var commitsCursor = _commitCollection.Find("{ }");
            return await commitsCursor.ToListAsync();
        }
    }
}