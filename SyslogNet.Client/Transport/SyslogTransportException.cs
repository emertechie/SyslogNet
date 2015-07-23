using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SyslogNet.Client.Transport
{
	class SyslogTransportException : Exception
	{
		public SyslogTransportException(string message) : base(message)
		{

		}
	}
}
