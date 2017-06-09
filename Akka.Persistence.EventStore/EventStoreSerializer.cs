using System;
using Akka.Actor;
using Akka.Serialization;
using EventStore.ClientAPI;

namespace Akka.Persistence.EventStore
{
    public abstract class EventStoreSerializer : Serializer
    {
        protected EventStoreSerializer(ExtendedActorSystem system) : base(system)
        {
        }

        public abstract EventData ToEvent(object o);
        public abstract object FromEvent(RecordedEvent recordedEvent, Type type);

        public object FromEvent(RecordedEvent recordedEvent)
        {
            return FromEvent(recordedEvent, Type.GetType(recordedEvent.EventType));
        }

        /// <summary>Deserializes an Event into an object.</summary>
        /// <param name="recordedEvent">EventData object from EventStore</param>
        /// <returns>The object contained in the array</returns>
        public T FromEvent<T>(RecordedEvent recordedEvent)
        {
            return (T)this.FromEvent(recordedEvent, typeof(T));
        }
    }
}