
namespace SyslogNet.Client
{


	[System.Serializable]
	public class SyslogNetException 
		: System.Exception
	{


		public SyslogNetException()
		{ }


		public SyslogNetException(string message) 
			: base(message)
		{ }


		public SyslogNetException(string message, System.Exception inner) 
			: base(message, inner)
		{ }


		protected SyslogNetException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) 
			: base(info, context)
		{ }


	}


}
