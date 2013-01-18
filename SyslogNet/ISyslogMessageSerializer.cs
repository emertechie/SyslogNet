using System.IO;

namespace SyslogNet
{
	public interface ISyslogMessageSerializer
	{
		void Serialize(SyslogMessage message, Stream stream);
	}
}