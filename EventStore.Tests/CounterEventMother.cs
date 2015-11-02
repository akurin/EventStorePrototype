using System.Collections.Generic;
using System.Linq;

namespace EventStore.Tests
{
    public static class CounterEventMother
    {
        public static IEnumerable<CounterIncremented> CreateIncrementedEvents()
        {
            return Enumerable.Range(0, int.MaxValue)
                .Select(i => new CounterIncremented {CounterValue = i});
        }
    }
}