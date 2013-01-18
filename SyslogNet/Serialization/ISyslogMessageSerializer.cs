using System.IO;

namespace SyslogNet.Serialization
{
	public interface ISyslogMessageSerializer
	{
		void Serialize(SyslogMessage message, Stream stream);
	}
}