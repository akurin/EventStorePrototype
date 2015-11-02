using System;
using MongoDB.Driver;

namespace EventStore.Mongo
{
    internal sealed class MongoExceptionAnalyzer
    {
        public static string ExtactViolatedIndexNameFrom(MongoWriteException exception)
        {
            if (exception.WriteError.Category != ServerErrorCategory.DuplicateKey)
                throw new ArgumentException("WriteError.Category should be DuplicateKey", nameof(exception));

            var message = exception.WriteError.Message;
            var startIndex = message.IndexOf(".$") + 2;
            var endIndex = message.IndexOf(" dup key");

            return message.Substring(startIndex, endIndex - startIndex);
        } 
    }
}