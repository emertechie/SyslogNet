using System;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public interface ISyslogMessageSender : IDisposable
	{
		void Send(SyslogMessage message, ISyslogMessageSerializer serializer);
	}
}