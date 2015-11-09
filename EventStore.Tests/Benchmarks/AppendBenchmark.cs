using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using EventStore.Mongo;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;

namespace EventStore.Tests.Benchmarks
{
    [TestClass]
    public sealed class AppendBenchmark
    {
        private IEventStore _eventStore;
        private List<CounterIncremented> _events;
        private const string ConnectionString = "mongodb://localhost:27017";
        private const string DatabaseName = "MongoEventStoreScenario";

        private const int IterationCount = 10;
        private const int EventCount = 1000;

        [TestInitialize]
        public void TestInitialize()
        {
            var mongoEventStoreFactory = new MongoEventStoreFactory();
            _eventStore = mongoEventStoreFactory.CreateAsync(ConnectionString, DatabaseName).Result;
            _events = CounterEventMother.CreateIncrementedEvents().Take(EventCount).ToList();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            var mongoClient = new MongoClient();
            mongoClient.DropDatabaseAsync(DatabaseName).Wait();
        }

        [TestMethod]
        public async Task BenchmarkAsync()
        {
            await WarmupAsync();

            var measureResults = new List<TimeSpan>();

            for (var interation = 0; interation < IterationCount; interation++)
            {
                var measureResult = await MeasureAsync(ActionAsync);
                measureResults.Add(measureResult);
            }

            for (var i = 0; i < measureResults.Count; i++)
            {
                Console.WriteLine("#{0}: {1}", i, measureResults[i]);
            }

            var averageTicks = measureResults.Sum(timeSpan => timeSpan.Ticks)/measureResults.Count;
            var average = TimeSpan.FromTicks(averageTicks);
            Console.WriteLine("Average: {0}", average);
        }

        private async Task WarmupAsync()
        {
            await ActionAsync();
        }

        private async Task ActionAsync()
        {
            var streamId = Guid.NewGuid();

            for (var eventIndex = 0; eventIndex < _events.Count; eventIndex++)
            {
                var expectedStreamLength = eventIndex;
                await _eventStore.AppendAsync(streamId, expectedStreamLength, new[] {_events[eventIndex]});
            }
        }

        private static async Task<TimeSpan> MeasureAsync(Func<Task> asyncAction)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            await asyncAction();
            return stopwatch.Elapsed;
        }
    }
}