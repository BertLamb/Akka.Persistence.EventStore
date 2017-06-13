using System;
using System.Diagnostics;
using EventStore.ClientAPI;

namespace Akka.Persistence.EventStore
{
    public class EventStoreSerialization
    {
        private readonly Akka.Serialization.Serialization _serialization;

        public EventStoreSerialization(Akka.Serialization.Serialization serialization)
        {
            _serialization = serialization;
        }

        public T Deserialize<T>(RecordedEvent recordedEvent)
        {
            var t = Type.GetType(recordedEvent.EventType, throwOnError: true);
            var ser = _serialization.FindSerializerForType(t);
            if (ser is EventStoreSerializer ess)
            {
                return ess.FromEvent<T>(recordedEvent);
            }
            return ser.FromBinary<T>(recordedEvent.Data);
        }

        public EventData Serialize<T>(T data)
        {

            var ser = _serialization.FindSerializerFor(data);
            if (ser is EventStoreSerializer ess)
            {
                return ess.ToEvent(data);
            }
            return new EventData(Guid.NewGuid(), typeof(T).AssemblyQualifiedName, false, ser.ToBinary(data), null);
        }
    }
}