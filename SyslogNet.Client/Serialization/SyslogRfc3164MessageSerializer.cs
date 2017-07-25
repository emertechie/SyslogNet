using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SyslogNet.Client.Transport;

namespace SyslogNet.Client.Serialization
{
	public class SyslogRfc3164MessageSerializer
	{
		public static async Task SerializeAsync(SyslogMessage message, Stream stream)
		{
			var priorityValue = ((int)message.Facility << 3) + (int)message.Severity;

			string timestamp = null;
			if (message.DateTimeOffset.HasValue)
			{
				var dt = message.DateTimeOffset.Value;
				var day = dt.Day < 10 ? " " + dt.Day : dt.Day.ToString(); // Yes, this is stupid but it's in the spec
				timestamp = string.Concat(dt.ToString("MMM "), day, dt.ToString(" HH:mm:ss"));
			}

			var headerBuilder = new StringBuilder();
			headerBuilder.Append("<").Append(priorityValue).Append(">");
			headerBuilder.Append(timestamp).Append(" ");
			headerBuilder.Append(message.HostName).Append(" ");
			headerBuilder.Append((message.AppName?.EnsureMaxLength(32) ?? "") + ":");
			headerBuilder.Append(message.Message ?? "");

			byte[] asciiBytes = Encoding.ASCII.GetBytes(headerBuilder.ToString());
			await stream.WriteAsync(asciiBytes, 0, asciiBytes.Length);
		}
	}
}
