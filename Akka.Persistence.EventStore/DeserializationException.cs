using System;
using System.Runtime.Serialization;

namespace Akka.Persistence.EventStore
{
    public class DeserializationException : Exception
    {
        public DeserializationException()
        {
        }

        public DeserializationException(string message) : base(message)
        {
        }

        public DeserializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DeserializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}