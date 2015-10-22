using System;

namespace EventStore.Mongo
{
    internal sealed class EventContainer
    {
        public IEvent Event { get; }
        public Guid Id { get; }

        public EventContainer(IEvent @event, Guid id)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            Event = @event;
            Id = id;
        }
    }
}