using SyslogNet.Serialization;

namespace SyslogNet.Transport
{
	public interface ISyslogMessageSender
	{
		void Send(SyslogMessage message, ISyslogMessageSerializer serializer);
	}
}