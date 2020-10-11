using System;
using System.Runtime.Serialization;

namespace OliWorkshop.Threading
{
    [Serializable]
    public class EventNotFoundException : Exception
    {
        public EventNotFoundException()
        {
        }

        public EventNotFoundException(string message) : base(message)
        {
        }

        public EventNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EventNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}