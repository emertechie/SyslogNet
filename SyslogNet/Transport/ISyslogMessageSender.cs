using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public interface ISyslogMessageSender
	{
		void Send(SyslogMessage message, ISyslogMessageSerializer serializer);
	}
}