using System;
using System.Runtime.Serialization;

namespace SyslogNet.Client
{
	[Serializable]
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

		protected CommunicationsException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}