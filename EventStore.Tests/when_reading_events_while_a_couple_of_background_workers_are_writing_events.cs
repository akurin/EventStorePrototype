using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.Mongo;
using Given.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EventStore.Tests
{
    [TestClass]
    [Story(AsA = "developer",
        IWant = "an event store with optimistic concurrency control",
        SoThat = "I can implement repository")]
    public class when_reading_events_while_a_couple_of_background_workers_are_writing_events : MongoEventStoreScenario
    {
        private static IEventStore _eventStore;
        private const int EventCount = 1000;
        private static readonly Guid SomeStreamId = Guid.NewGuid();
        private static Task _backgroundTask1;
        private static Task _backgroundTask2;
        private static AggregateException _resultException;

        private given mongo_event_store_and_a_couple_of_background_workers = () =>
        {
            _eventStore = CreateMongoEventStore();
            var events = FibonacciEventMother.CreateEvents().Take(EventCount);
            _backgroundTask1 = _eventStore.AppendAsync(SomeStreamId, ExpectedStreamVersion.Empty, events);
            _backgroundTask2 = _eventStore.AppendAsync(SomeStreamId, ExpectedStreamVersion.Empty, events);
        };

        private when waiting_for_workers_to_complete = () =>
        {
            try
            {
                Task.WhenAll(_backgroundTask1, _backgroundTask2).Wait();
            }
            catch (AggregateException ex)
            {
                _resultException = ex;
            }
        };

        [TestMethod]
        public void some_of_them_should_throw_concurrency_exception()
        {
            _resultException.ShouldNotBeNull();
            _resultException.InnerExceptions.Count.ShouldEqual(1);
            var exception = _resultException.InnerExceptions.Single();
            exception.ShouldBeOfType<OptimisticConcurrencyException>();
        }
    }
}