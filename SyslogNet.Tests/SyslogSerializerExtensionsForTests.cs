using System.IO;
using System.Text;

namespace SyslogNet.Tests
{
	internal static class SyslogSerializerExtensionsForTests
	{
		public static string Serialize(this SyslogSerializer serializer, SyslogMessage message)
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