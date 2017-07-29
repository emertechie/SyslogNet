using System;
using System.Runtime.Serialization;

namespace SyslogNet.Client
{
	[Serializable]
	public class SyslogNetException : Exception
	{
		public SyslogNetException()
		{
		}

		public SyslogNetException(string message) : base(message)
		{
		}

		public SyslogNetException(string message, Exception inner) : base(message, inner)
		{
		}

		protected SyslogNetException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
	}
}