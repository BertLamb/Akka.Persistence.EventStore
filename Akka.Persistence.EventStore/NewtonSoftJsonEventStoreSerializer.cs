using System;
using Akka.Actor;
using Akka.Persistence.EventStore.Exceptions;
using Akka.Serialization;
using EventStore.ClientAPI;

namespace Akka.Persistence.EventStore
{
    public class NewtonSoftJsonEventStoreSerializer : EventStoreSerializer
    {
        public override int Identifier => 6977;
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
                return new EventData(Guid.NewGuid(), GetClassForObject(o), true, ToBinary(o), null);
            }
            throw new ArgumentException($"Cannot serialize {o.GetType().AssemblyQualifiedName}, SnapshotEvent expected");
        }

        public override object FromEvent(RecordedEvent recordedEvent, Type type)
        {
            var eventType = Type.GetType(recordedEvent.EventType, throwOnError: true);
            var result = FromBinary(recordedEvent.Data, eventType);
            if (type.IsInstanceOfType(result))
            {
                return result;
            }
            throw new DeserializationException($"Cannot deserialize event as {type}, event: {recordedEvent}");
        }

        private string GetClassForObject(object o)
        {
            return o.GetType().AssemblyQualifiedName;
        }
    }
}