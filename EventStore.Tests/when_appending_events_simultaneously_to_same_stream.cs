using System;
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
        IWant = "an event store with optimistic concurrency control",
        SoThat = "I can implement repository")]
    public class when_appending_events_simultaneously_to_same_stream :
        MongoEventStoreScenario
    {
        private static readonly Guid SomeStreamId = Guid.NewGuid();
        private const int EventCount = 1000;
        private static Task _task1;
        private static Task _task2;

        private given mongo_event_store = () => { DropDatabase(); };

        private given two_parallel_tasks_of_appending_events_to_same_stream = () =>
        {
            _task1 = AppendAsync();
            _task2 = AppendAsync();
        };

        private static Task AppendAsync()
        {
            var eventStore = CreateMongoEventStore();
            var events = CounterEventMother.CreateIncrementedEvents().Take(EventCount);
            return eventStore.AppendAsync(SomeStreamId, 0, events);
        }

        private when awaiting_tasks = () =>
        {
            WaitAndIgnoreAggregateException(_task1);
            WaitAndIgnoreAggregateException(_task2);
        };

        private static void WaitAndIgnoreAggregateException(Task task)
        {
            try
            {
                task.Wait();
            }
            catch (AggregateException)
            {
            }
        }

        [TestMethod]
        public void one_task_should_fail_with_optimistic_concurrency_exception()
        {
            then(() =>
            {
                var task1Failed = FailedWithOptimisticConcurrencyException(_task1);
                var task2Failed = FailedWithOptimisticConcurrencyException(_task2);

                (task1Failed || task2Failed).ShouldBeTrue();
                (task1Failed && task2Failed).ShouldBeFalse();
            });
        }

        private static bool FailedWithOptimisticConcurrencyException(Task task)
        {
            return task.IsFaulted && task.Exception.InnerExceptions.Single() is OptimisticConcurrencyException;
        }

        [TestMethod]
        public void another_task_should_successfully_append_events()
        {
            then(() =>
            {
                var eventStore = CreateMongoEventStore();
                var events = eventStore.ReadAsync(SomeStreamId, 0, 10000).Result;
                events.Count().ShouldEqual(EventCount);
            });
        }
    }
}