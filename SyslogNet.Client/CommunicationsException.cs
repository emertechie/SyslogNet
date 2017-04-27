using System;
#if NET4_0
using System.Runtime.Serialization;
#endif

namespace SyslogNet.Client
{
#if NET4_0
    [Serializable]
#endif
    public class CommunicationsException : Exception
	{
		public CommunicationsException()
		{
		}

		public CommunicationsException(string message) : base(message)
		{
		}

		public CommunicationsException(string message, Exception inner) : base(message, inner)
		{
		}

#if NET4_0
        protected CommunicationsException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
#endif
    }
}