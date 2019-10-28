using System;
using System.Runtime.Serialization;

namespace Gatling.Orchestrator
{
    [Serializable]
    public class GatlingTestFailureException : Exception
    {
        public GatlingTestFailureException()
        {
        }

        public GatlingTestFailureException(string message) : base(message)
        {
        }

        public GatlingTestFailureException(string message, Exception inner) : base(message, inner)
        {
        }

        protected GatlingTestFailureException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}