using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public interface ISyslogMessageSender : IDisposable
	{
		void Reconnect();
		void Send(SyslogMessage message, ISyslogMessageSerializer serializer);
		void Send(IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer);
		Task SendAsync(SyslogMessage message, ISyslogMessageSerializer serializer);
		Task SendAsync(IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer);

	}
}