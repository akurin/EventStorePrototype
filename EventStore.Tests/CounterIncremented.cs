namespace EventStore.Tests
{
    public class CounterIncremented : IEvent
    {
        public int CounterValue { get; set; }
    }
}