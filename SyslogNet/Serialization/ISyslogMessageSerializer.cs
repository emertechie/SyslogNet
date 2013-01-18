using System.IO;

namespace SyslogNet.Client.Serialization
{
	public interface ISyslogMessageSerializer
	{
		void Serialize(SyslogMessage message, Stream stream);
	}
}