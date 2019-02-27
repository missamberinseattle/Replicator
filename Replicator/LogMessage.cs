using System.Diagnostics;

namespace Replicator
{
    public class LogMessage
    {
        public LogMessage(TraceLevel traceLevel, string prefix, string message)
        {
            TraceLevel = traceLevel;
            Message = message;
            Prefix = prefix;
        }

        public TraceLevel TraceLevel { get; }
        public string Message { get; }
        public string Prefix { get; }
    }
}