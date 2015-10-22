using System.Collections.Generic;
using System.Linq;

namespace EventStore.Tests
{
    public static class FibonacciEventMother
    {
        public static IEnumerable<FibonacciNumberCalculated> CreateEvents()
        {
            return CalculateFibonacciSequence()
                .Select(fibNumber => new FibonacciNumberCalculated {Number = fibNumber});
        }

        private static IEnumerable<int> CalculateFibonacciSequence()
        {
            var previous = 1;
            var current = 1;

            yield return previous;
            yield return current;

            while (true)
            {
                var memorizedCurrent = current;
                current = current + previous;
                yield return current;
                previous = memorizedCurrent;
            }
        }
    }
}