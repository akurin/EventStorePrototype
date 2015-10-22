using System;
using System.Collections.Generic;
using System.Linq;
using Given.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Driver;

namespace EventStore.Tests
{
    [TestClass]
    [Story(AsA = "developer",
        IWant = "an event store that stores events",
        SoThat = "I can implement repository")]
    public class when_reading_events : MongoEventStoreScenario
    {
        private static IEventStore _eventStore;
        private static readonly Guid SomeStreamId = Guid.NewGuid();
        private static IEnumerable<IEvent> _readResult;

        private given mongo_event_store_with_appended_events = () =>
        {
            _eventStore = CreateMongoEventStore();
            var events = FibonacciEventMother.CreateEvents().Take(3);
            _eventStore.AppendAsync(SomeStreamId, ExpectedStreamVersion.Empty, events).Wait();
        };

        private when reading_events = () =>
        {
            _readResult = _eventStore.ReadAsync(SomeStreamId).Result;
        };

        [TestMethod]
        public void it_should_return_expected_number_of_events()
        {
            _readResult.Count().ShouldEqual(3);
        }

        [TestMethod]
        public void it_should_return_right_events()
        {
            var events = _readResult.Cast<FibonacciNumberCalculated>().ToList();
            events[0].Number.ShouldEqual(1);
            events[1].Number.ShouldEqual(1);
            events[2].Number.ShouldEqual(2);
        }
    }
}