using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SyslogNet.Client.Serialization
{
	public class SyslogRfc3164MessageSerializer
	{
		public static async Task SerializeAsync(SyslogMessage message, Stream stream, CancellationToken token)
		{
			var priorityValue = ((int) message.Facility << 3) + (int) message.Severity;

			var dt = message.Timestamp ?? DateTimeOffset.UtcNow;
			var day = dt.Day < 10 ? " " + dt.Day : dt.Day.ToString();
			var timestamp = string.Concat(dt.ToString("MMM "), day, dt.ToString(" HH:mm:ss"));

			var serializedMessage = new StringBuilder()
				.Append("<")
				.Append(priorityValue)
				.Append(">")
				.Append(timestamp)
				.Append(" ")
				.Append(message.HostName ?? "-")
				.Append(" ")
				.Append(EnsureMaxLength(message.AppName, 32))
				.Append(":")
				.Append(message.Message ?? "");

			var asciiBytes = Encoding.ASCII.GetBytes(serializedMessage.ToString());
			await stream.WriteAsync(asciiBytes, 0, asciiBytes.Length, token);
		}
		
		public static string EnsureMaxLength(string s, int maxLength)
		{
			return string.IsNullOrWhiteSpace(s)
				? ""
				: s.Length <= maxLength 
					? s 
					: s.Substring(0, maxLength);
		}

	}
}
