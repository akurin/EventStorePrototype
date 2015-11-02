using System;
using System.Linq;
using Given.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local

namespace EventStore.Tests
{
    [TestClass]
    [Story(AsA = "developer",
        IWant = "an event store with optimistic concurrency control",
        SoThat = "I can implement repository")]
    public class when_appending_events_to_not_empty_stream_with_wrong_expected_stream_length :
        MongoEventStoreScenario
    {
        private static IEventStore _eventStore;
        private static readonly Guid SomeStreamId = Guid.NewGuid();
        private static OptimisticConcurrencyException _resultException;

        private given mongo_event_store = () =>
        {
            DropDatabase();
            _eventStore = CreateMongoEventStore();
        };

        private given not_empty_stream =
            () =>
            {
                _eventStore.AppendAsync(SomeStreamId, 0, new[] {new CounterIncremented {CounterValue = 100}}).Wait();
            };

        private when appending_events_to_the_stream_with_wrong_expected_stream_length = () =>
        {
            try
            {
                _eventStore.AppendAsync(SomeStreamId, 100, new[] {new CounterIncremented {CounterValue = 10}}).Wait();
            }
            catch (AggregateException ex)
            {
                _resultException = ex.InnerExceptions.SingleOrDefault() as OptimisticConcurrencyException;
            }
        };

        [TestMethod]
        public void it_should_throw_optimistic_concurrency_exception()
        {
            _resultException.ShouldBeOfType<OptimisticConcurrencyException>();
        }
    }
}