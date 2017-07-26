using System.IO;
using System.Text;
using System.Threading;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Tests.Serialization
{
	internal static class SyslogSerializerExtensionsForTests
	{
		public static string SerializeRfc5424(SyslogMessage message)
		{
			using (var stream = new MemoryStream())
			{
				SyslogRfc5424MessageSerializer.SerializeAsync(message, stream, default(CancellationToken)).Wait();
				stream.Flush();
				stream.Position = 0;

				using (var reader = new StreamReader(stream, Encoding.UTF8))
					return reader.ReadLine();
			}
		}

		public static string SerializeRfc3164(SyslogMessage message)
		{
			using (var stream = new MemoryStream())
			{
				SyslogRfc3164MessageSerializer.SerializeAsync(message, stream, default(CancellationToken)).Wait();
				stream.Flush();
				stream.Position = 0;

				using (var reader = new StreamReader(stream, Encoding.UTF8))
					return reader.ReadLine();
			}
		}
	}
}