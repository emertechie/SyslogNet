using System;
using System.Collections.Generic;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public interface ISyslogMessageSender : IDisposable
	{
		void Reconnect();
		void Send(SyslogMessage message, ISyslogMessageSerializer serializer);
		void Send(IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer);
	}
}