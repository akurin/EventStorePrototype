using System;
using MongoDB.Bson;

namespace EventStore.Mongo
{
    internal static class BsonValueCastExtensions
    {
        public static Guid AsGuidOrEventStoreException(this BsonValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            EnsureValueHasExpectedType(value, BsonType.Binary);
            return value.AsGuid;
        }

        private static void EnsureValueHasExpectedType(BsonValue value, BsonType bsonType)
        {
            if (value.BsonType != bsonType)
                throw new EventStoreException($"Expected BSON type {bsonType} but was {value.BsonType}");
        }

        public static int AsInt32OrEventStoreException(this BsonValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            EnsureValueHasExpectedType(value, BsonType.Int32);
            return value.AsInt32;
        }

        public static BsonArray AsBsonArrayOrEventStoreException(this BsonValue value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            EnsureValueHasExpectedType(value, BsonType.Array);
            return value.AsBsonArray;
        }
    }
}