using System;
using System.Runtime.Serialization;

namespace Akka.Persistence.EventStore.Exceptions
{
    public class UnexpectedStreamStatusException : Exception
    {
        public UnexpectedStreamStatusException()
        {
        }

        public UnexpectedStreamStatusException(string message) : base(message)
        {
        }

        public UnexpectedStreamStatusException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnexpectedStreamStatusException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}