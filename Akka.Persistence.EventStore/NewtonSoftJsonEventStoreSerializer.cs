using System;
using Akka.Actor;
using Akka.Serialization;
using EventStore.ClientAPI;

namespace Akka.Persistence.EventStore
{
    public class NewtonSoftJsonEventStoreSerializer : EventStoreSerializer
    {
        private readonly NewtonSoftJsonSerializer _jsonSerializer;

        public NewtonSoftJsonEventStoreSerializer(ExtendedActorSystem system) : base(system)
        {
            _jsonSerializer = new NewtonSoftJsonSerializer(system);
        }

        public override byte[] ToBinary(object obj)
        {
            return _jsonSerializer.ToBinary(obj);
        }

        public override object FromBinary(byte[] bytes, Type type)
        {
            return _jsonSerializer.FromBinary(bytes, type);
        }

        public override bool IncludeManifest => true;

        public override EventData ToEvent(object o)
        {
            if (o is IPersistentRepresentation || o is ISnapshotEvent)
            {
                return new EventData(Guid.NewGuid(), o.GetType().FullName, true, ToBinary(o), null);
            }
            throw new ArgumentException($"Cannot serialize {o.GetType().FullName}, SnapshotEvent expected");
        }

        public override object FromEvent(RecordedEvent recordedEvent, Type type)
        {
            var eventType = Type.GetType(recordedEvent.EventType);
            var result = FromBinary(recordedEvent.Data, eventType);
            if (result.GetType() == type)
            {
                return result;
            }
            throw new DeserializationException($"Cannot deserialize event as {type}, event: {recordedEvent}");
        }
    }
}