using System;
using System.IO;
using System.Text;

namespace SyslogNet
{
	public class SyslogRfc3164MessageSerializer : SyslogMessageSerializerBase, ISyslogMessageSerializer
	{
		public void Serialize(SyslogMessage message, Stream stream)
		{
			var priorityValue = CalculatePriorityValue(message.Facility, message.Severity);

			string timestamp = null;
			if (message.DateTimeOffset.HasValue)
			{
				var dt = message.DateTimeOffset.Value;
				var day = dt.Day < 10 ? " " + dt.Day : dt.Day.ToString(); // Yes, this is stupid but it's in the spec
				timestamp = String.Concat(dt.ToString("MMM "), day, dt.ToString(" HH:mm:ss"));
			}

			var headerBuilder = new StringBuilder();
			headerBuilder.Append("<").Append(priorityValue).Append(">");
			headerBuilder.Append(timestamp).Append(" ");
			headerBuilder.Append(message.HostName).Append(" ");
			headerBuilder.Append(message.AppName.IfNotNullOrWhitespace(x => x.EnsureMaxLength(32) + ":"));
			headerBuilder.Append(message.Message ?? "");

			byte[] asciiBytes = Encoding.ASCII.GetBytes(headerBuilder.ToString());
			stream.Write(asciiBytes, 0, asciiBytes.Length);
		}
	}
}