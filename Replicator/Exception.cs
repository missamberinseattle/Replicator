using System;
using System.Runtime.Serialization;

namespace Replicator
{
    [Serializable]
    public class RootNotFoundException : Exception
    {
        public RootNotFoundException()
        {
        }

        public RootNotFoundException(string message) : base(message)
        {
        }

        public RootNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected RootNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}