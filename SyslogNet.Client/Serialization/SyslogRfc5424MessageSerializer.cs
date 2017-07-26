using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SyslogNet.Client.Extensions.StreamExtensions;

namespace SyslogNet.Client.Serialization
{
	public class SyslogRfc5424MessageSerializer
	{
		private const string NilValue = "-";

		public static async Task SerializeAsync(SyslogMessage message, Stream stream, CancellationToken token)
		{
			var priorityValue = ((int) message.Facility << 3) + (int) message.Severity;

			// Note: The .Net ISO 8601 "o" format string uses 7 decimal places for fractional second.
			// Syslog spec only allows 6, hence the custom format string
			var timestamp = (message.Timestamp ?? DateTimeOffset.UtcNow).ToString("yyyy-MM-ddTHH:mm:ss.ffffffK");

			var messageBuilder = new StringBuilder()
				.Append("<")
				.Append(priorityValue)
				.Append(">1 ")
				.Append(timestamp)
				.Append(" ")
				.Append(FieldOrNil(Sanitize(message.HostName, 255)))
				.Append(" ")
				.Append(FieldOrNil(Sanitize(message.AppName, 48)))
				.Append(" ")
				.Append(FieldOrNil(Sanitize(message.ProcId, 128)))
				.Append(" ")
				.Append(FieldOrNil(Sanitize(message.MsgId, 32)));

			await WriteStreamAsync(stream, messageBuilder.ToString(), token);

			await stream.WriteByteAsync(' ', token);
			if (message.StructuredDataElements != null && message.StructuredDataElements.Any())
			{
                foreach (var sdElement in message.StructuredDataElements)
                {
	                await WriteStructuredDataElement(stream, sdElement, token);
                }
			}
			else
			{
				await WriteStreamAsync(stream, NilValue, token);
			}

			await stream.WriteByteAsync(' ', token);
			await stream.WriteAsync(Encoding.UTF8.GetPreamble(), 0, Encoding.UTF8.GetPreamble().Length, token);
			await WriteStreamAsync(stream, message.Message ?? NilValue, token);
		}

		private static readonly StringBuilder MessageBuilder = new StringBuilder();
		private static async Task WriteStructuredDataElement(Stream stream, 
													         StructuredDataElement sdElement,
													         CancellationToken token)
		{
			MessageBuilder.Clear()
				.Append("[")
				.Append(Sanitize(sdElement.SdId, 32, true));

			await WriteStreamAsync(stream, MessageBuilder.ToString(), token);

			foreach (var sdParam in sdElement.Parameters)
			{
				MessageBuilder.Clear()
					.Append(" ")
					.Append(Sanitize(sdParam.Key, 32, true))
					.Append("=")
					.Append("\"")
					.Append(
						sdParam.Value?.Replace("\\", "\\\\")
							.Replace("\"", "\\\"")
							.Replace("]", "\\]") ?? string.Empty
					)
					.Append("\"");

				await WriteStreamAsync(stream, MessageBuilder.ToString(), token);
			}

			await stream.WriteByteAsync(']', token);
		}

		private static async Task WriteStreamAsync(Stream stream, string data, CancellationToken token)
		{
			var streamBytes = Encoding.UTF8.GetBytes(data);
			await stream.WriteAsync(streamBytes, 0, streamBytes.Length, token);
		}

		private static string FieldOrNil(string value)
		{
			return string.IsNullOrWhiteSpace(value) ? NilValue : value;
		}

		private static readonly char[] Buffer = new char[255];
		private static readonly HashSet<char> SdNameDisallowedChars = new HashSet<char> { ' ', '=', ']', '"' };
		public static string Sanitize(string s, int maxLength, bool sanitizeAsSdName = false)
		{
			if (s == null) return "";
			
			var bufferIndex = 0;
			foreach (var c in s)
			{
				if (bufferIndex == maxLength) break;
				if (c < 33 || c > 126) continue;
				if (sanitizeAsSdName && SdNameDisallowedChars.Contains(c)) continue;
				
				Buffer[bufferIndex++] = c;
			}

			return new string(Buffer, 0, Math.Min(bufferIndex, maxLength));
		}
	}
}
