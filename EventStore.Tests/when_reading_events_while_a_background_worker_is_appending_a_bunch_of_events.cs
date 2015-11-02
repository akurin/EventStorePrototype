using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Given.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace EventStore.Tests
{
    [TestClass]
    [Story(AsA = "developer",
        IWant = "an event store that does not allow dirty reads",
        SoThat = "I can implement repository")]
    public class when_reading_events_while_a_background_worker_is_appending_a_bunch_of_events : MongoEventStoreScenario
    {
        private static IEventStore _eventStore;
        private static readonly Guid SomeStreamId = Guid.NewGuid();
        private static ICollection<IEnumerable<IEvent>> _readResults;
        private const int EventCount = 1000;
        private static Task _backgroundTask;

        private given mongo_event_store = () =>
        {
            DropDatabase();
            _eventStore = CreateMongoEventStore();
        };

        private given an_empty_stream = () => { };

        private given background_worker_is_appending_a_bunch_of_events_to_the_stream = () =>
        {
            var events = CounterEventMother.CreateIncrementedEvents().Take(EventCount);
            var eventStore = CreateMongoEventStore();
            _backgroundTask = eventStore.AppendAsync(SomeStreamId, 0, events);
        };

        private when reading_events = () =>
        {
            _readResults = new List<IEnumerable<IEvent>>();
            while (!_backgroundTask.IsCompleted)
            {
                var events = _eventStore.ReadAsync(SomeStreamId, 0, 10000).Result;
                _readResults.Add(events);
            }
        };

        [TestMethod]
        public void it_should_return_the_whole_bunch_of_events_or_nothing()
        {
            then(() =>
            {
                foreach (var readResult in _readResults)
                {
                    var count = readResult.Count();
                    if (count > 0)
                        count.ShouldEqual(EventCount);
                }
            });
        }

        [TestMethod]
        public void finally_it_should_return_the_bunch_of_events()
        {
            then(() =>
            {
                _readResults.Last().Count().ShouldEqual(EventCount);
            });
        }
    }
}