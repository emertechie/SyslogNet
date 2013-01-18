using System.IO;
using System.Text;
using SyslogNet.Serialization;

namespace SyslogNet.Tests.Serialization
{
	internal static class SyslogSerializerExtensionsForTests
	{
		public static string Serialize(this SyslogRfc5424MessageSerializer serializer, SyslogMessage message)
		{
			using (var stream = new MemoryStream())
			{
				serializer.Serialize(message, stream);
				stream.Flush();
				stream.Position = 0;

				using (var reader = new StreamReader(stream, Encoding.UTF8))
					return reader.ReadLine();
			}
		}

		public static string Serialize(this SyslogRfc3164MessageSerializer serializer, SyslogMessage message)
		{
			using (var stream = new MemoryStream())
			{
				serializer.Serialize(message, stream);
				stream.Flush();
				stream.Position = 0;

				using (var reader = new StreamReader(stream, Encoding.UTF8))
					return reader.ReadLine();
			}
		}
	}
}