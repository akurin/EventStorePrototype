using System;
using System.Collections.Generic;
using System.Linq;
using Given.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace EventStore.Tests
{
    [TestClass]
    [Story(AsA = "developer",
        IWant = "an event store that stores events",
        SoThat = "I can implement repository")]
    public class when_reading_events_from_stream : MongoEventStoreScenario
    {
        private static IEventStore _eventStore;
        private static readonly Guid SomeStreamId = Guid.NewGuid();
        private static IEnumerable<IEvent> _readResult;

        private given mongo_event_store = () =>
        {
            DropDatabase();
            _eventStore = CreateMongoEventStore();
        };

        private given not_empty_stream = () =>
        {
            var events = CounterEventMother.CreateIncrementedEvents().Take(10);
            var appendedEventCount = 0;

            foreach (var counterIncremented in events)
            {
                _eventStore.AppendAsync(SomeStreamId, appendedEventCount, new[] {counterIncremented}).Wait();
                appendedEventCount++;
            }
        };

        private when reading_events_from_the_stream =
            () => { _readResult = _eventStore.ReadAsync(SomeStreamId, 0, 100).Result; };

        [TestMethod]
        public void it_should_return_right_number_of_events()
        {
            then(() => _readResult.Count().ShouldEqual(10));
        }

        [TestMethod]
        public void it_should_return_right_events()
        {
            then(() =>
            {
                var events = _readResult.Cast<CounterIncremented>().ToList();

                for (var i = 0; i < events.Count; i++)
                {
                    events[i].CounterValue.ShouldEqual(i);
                }
            });
        }
    }
}