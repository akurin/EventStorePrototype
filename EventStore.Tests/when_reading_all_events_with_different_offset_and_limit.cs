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
        IWant = "an event store that can be used to read events",
        SoThat = "I can implement projection engine")]
    public class when_reading_all_events_with_different_offset_and_limit : MongoEventStoreScenario
    {
        private static IEventStore _eventStore;
        private const int EventCount = 100;
        private static readonly Guid SomeStreamId1 = Guid.NewGuid();
        private static readonly Guid SomeStreamId2 = Guid.NewGuid();
        private static IEnumerable<IEvent> _readAllEventsAtATimeResult;
        private static IEnumerable<IEvent> _readAllEventsOneByOneResult;

        private given mongo_event_store = () =>
        {
            DropDatabase();
            _eventStore = CreateMongoEventStore();
        };

        private given events_has_been_appended_to_a_couple_of_streams = () =>
        {
            Task.WhenAll(
                AppendEventsOneByOneAsync(SomeStreamId1),
                AppendEventsOneByOneAsync(SomeStreamId2))
                .Wait();
        };

        private static async Task AppendEventsOneByOneAsync(Guid streamId)
        {
            var eventStore = CreateMongoEventStore();
            var events = CounterEventMother.CreateIncrementedEvents().Take(EventCount);
            var appendedEventCount = 0;
            foreach (var counterIncremented in events)
            {
                await eventStore.AppendAsync(streamId, appendedEventCount, new[] {counterIncremented});
                appendedEventCount++;
            }
        }

        private when reading_all_events_with_different_offset_and_limit = () =>
        {
            _readAllEventsAtATimeResult = _eventStore.ReadAllAsync(0, 10000).Result;
            _readAllEventsOneByOneResult = ReadAllEventsOneByOne();
        };

        private static IEnumerable<IEvent> ReadAllEventsOneByOne()
        {
            var events = new List<IEvent>();
            for (var i = 0; i < EventCount*2; i++)
            {
                var @event = _eventStore.ReadAllAsync(i, 1).Result.Single();
                events.Add(@event);
            }

            return events;
        }

        [TestMethod]
        public void it_should_return_right_number_of_events()
        {
            then(() =>
            {
                const int expectedEventCount = 2*EventCount;
                _readAllEventsAtATimeResult.Count().ShouldEqual(expectedEventCount);
                _readAllEventsOneByOneResult.Count().ShouldEqual(expectedEventCount);
            });
        }

        [TestMethod]
        public void aggregated_result_should_be_same()
        {
            then(() =>
            {
                var readEventsAtATimeResult = _readAllEventsAtATimeResult.Cast<CounterIncremented>().ToList();
                var readAllEventsOneByOneResult = _readAllEventsOneByOneResult.Cast<CounterIncremented>().ToList();

                for (var i = 0; i < EventCount*2; i++)
                {
                    var counterValue1 = readEventsAtATimeResult[i].CounterValue;
                    var counterValue2 = readAllEventsOneByOneResult[i].CounterValue;

                    counterValue1.ShouldEqual(counterValue2);
                }
            });
        }
    }
}