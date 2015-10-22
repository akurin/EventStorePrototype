using System;
using EventStore.Mongo;
using Given.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;

namespace EventStore.Tests
{
    public class MongoEventStoreScenario : Scenario
    {
        private const string ConnectionString = "mongodb://localhost:27017";
        private const string DatabaseName = "MongoEventStoreScenario";

        protected static IEventStore CreateMongoEventStore()
        {
            var mongoEventStoreFactory = new MongoEventStoreFactory();
            return mongoEventStoreFactory.CreateAsync(ConnectionString, DatabaseName).Result;
        }

        [TestInitialize]
        [TestCleanup]
        public void DropDatabase()
        {
            var mongoClient = new MongoClient();
            mongoClient.DropDatabaseAsync(DatabaseName).Wait();
        }
    }
}