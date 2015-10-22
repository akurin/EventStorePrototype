using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Given.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventStore.Tests
{
    [TestClass]
    [Story(AsA = "developer",
        IWant = "an event store that writes commits atomically",
        SoThat = "I can implement repository")]
    public class when_reading_events_while_a_background_worker_is_writing_events : MongoEventStoreScenario
    {
        private static IEventStore _eventStore;
        private static readonly Guid SomeStreamId = Guid.NewGuid();
        private static ICollection<IEnumerable<IEvent>> _readResults;
        private const int EventCount = 1000;
        private static Task _backgroundTask;

        private given mongo_event_store_and_background_writer = () =>
        {
            _eventStore = CreateMongoEventStore();
            var events = FibonacciEventMother.CreateEvents().Take(EventCount);
            _backgroundTask = _eventStore.AppendAsync(SomeStreamId, ExpectedStreamVersion.Empty, events);
        };

        private when reading_events = () =>
        {
            _readResults = new List<IEnumerable<IEvent>>();
            while (!_backgroundTask.IsCompleted)
            {
                var events = _eventStore.ReadAsync(SomeStreamId).Result;
                _readResults.Add(events);
            }
        };

        [TestMethod]
        public void it_should_return_the_whole_commit_or_nothing()
        {
            foreach (var readResult in _readResults)
            {
                var count = readResult.Count();
                if (count > 0)
                    count.ShouldEqual(EventCount);
            }
        }
    }
}