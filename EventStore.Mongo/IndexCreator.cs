using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace EventStore.Mongo
{
    internal sealed class IndexCreator
    {
        public static readonly string StreamIdAndIndexInStreamName =
            $"{CommitSerializer.StreamIdFieldName}_{CommitSerializer.IndexInStream}";

        public static readonly string IndexInAllStreamsIndexName = CommitSerializer.IndexInAllStreams;

        private readonly IMongoCollection<BsonDocument> _commitCollection;

        public IndexCreator(IMongoCollection<BsonDocument> commitCollection)
        {
            if (commitCollection == null) throw new ArgumentNullException(nameof(commitCollection));

            _commitCollection = commitCollection;
        }

        public async Task EnsureIndexesAsync()
        {
            await CreateStreamIdAndVersionIndexAsync();
            await IndexInAllStreamsIndexAsync();
        }

        private async Task CreateStreamIdAndVersionIndexAsync()
        {
            var keys = Builders<BsonDocument>.IndexKeys
                .Ascending(CommitSerializer.StreamIdFieldName)
                .Ascending(CommitSerializer.IndexInStream);

            await _commitCollection
                .Indexes.CreateOneAsync(keys, new CreateIndexOptions {Name = StreamIdAndIndexInStreamName, Unique = true});
        }

        private async Task IndexInAllStreamsIndexAsync()
        {
            var keys = Builders<BsonDocument>.IndexKeys
                .Ascending(CommitSerializer.IndexInAllStreams);

            var keyName = CommitSerializer.IndexInAllStreams;

            await _commitCollection
                .Indexes.CreateOneAsync(keys, new CreateIndexOptions {Name = keyName, Unique = true});
        }
    }
}