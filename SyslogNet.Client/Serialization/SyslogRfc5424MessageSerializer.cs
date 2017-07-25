using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using SyslogNet.Client.Transport;

namespace SyslogNet.Client.Serialization
{
	public class SyslogRfc5424MessageSerializer
	{
		public const string NilValue = "-";
		public static readonly HashSet<char> SdNameDisallowedChars = new HashSet<char>() {' ', '=', ']', '"' };

		private static readonly char[] AsciiCharsBuffer = new char[255];

		public static async Task SerializeAsync(SyslogMessage message, Stream stream)
		{
			var priorityValue = ((int)message.Facility << 3) + (int)message.Severity;

			// Note: The .Net ISO 8601 "o" format string uses 7 decimal places for fractional second. Syslog spec only allows 6, hence the custom format string
			var timestamp = message.DateTimeOffset?.ToString("yyyy-MM-ddTHH:mm:ss.ffffffK");

			var messageBuilder = new StringBuilder();
			messageBuilder.Append("<").Append(priorityValue).Append(">");
			messageBuilder.Append(message.Version);
			messageBuilder.Append(" ").Append(FormatSyslogField(timestamp, NilValue, 27));
			messageBuilder.Append(" ").Append(FormatSyslogAsciiField(message.HostName, NilValue, 255, AsciiCharsBuffer));
			messageBuilder.Append(" ").Append(FormatSyslogAsciiField(message.AppName, NilValue, 48, AsciiCharsBuffer));
			messageBuilder.Append(" ").Append(FormatSyslogAsciiField(message.ProcId, NilValue, 128, AsciiCharsBuffer));
			messageBuilder.Append(" ").Append(FormatSyslogAsciiField(message.MsgId, NilValue, 32, AsciiCharsBuffer));

			await WriteStreamAsync(stream, Encoding.ASCII, messageBuilder.ToString());

			var structuredData = message.StructuredDataElements?.ToList();
			if (structuredData != null && structuredData.Any())
			{
			    // Space
				await stream.WriteByteAsync(32);

                // Structured data
                foreach (var sdElement in structuredData)
				{
					messageBuilder.Clear()
						.Append("[")
						.Append(FormatSyslogSdnameField(sdElement.SdId, AsciiCharsBuffer));

					await WriteStreamAsync(stream, Encoding.ASCII, messageBuilder.ToString());

					foreach(var sdParam in sdElement.Parameters)
					{
						messageBuilder.Clear()
							.Append(" ")
							.Append(FormatSyslogSdnameField(sdParam.Key, AsciiCharsBuffer))
							.Append("=")
							.Append("\"")
							.Append(
								sdParam.Value?.Replace("\\", "\\\\")
									.Replace("\"", "\\\"")
									.Replace("]", "\\]") ?? string.Empty
							)
							.Append("\"");

						await WriteStreamAsync(stream, Encoding.UTF8, messageBuilder.ToString());
					}

					// ]
					await stream.WriteByteAsync(93);
				}
			}
			else
			{
				await WriteStreamAsync(stream, Encoding.ASCII, " ");
				await WriteStreamAsync(stream, Encoding.ASCII, NilValue);
			}

			if (!string.IsNullOrWhiteSpace(message.Message))
			{
				// Space
				await stream.WriteByteAsync(32);
				await stream.WriteAsync(Encoding.UTF8.GetPreamble(), 0, Encoding.UTF8.GetPreamble().Length);
				await WriteStreamAsync(stream, Encoding.UTF8, message.Message);
			}
		}

		private static async Task WriteStreamAsync(Stream stream, Encoding encoding, String data)
		{
			var streamBytes = encoding.GetBytes(data);
			await stream.WriteAsync(streamBytes, 0, streamBytes.Length);
		}
		
		public static string FormatSyslogAsciiField(string s, string replacementValue, int maxLength, char[] charBuffer, Boolean sdName = false)
		{
			s = FormatSyslogField(s, replacementValue, maxLength);

			var bufferIndex = 0;
			foreach (var c in s)
			{
				if (c < 33 || c > 126) continue;
				
				if (!sdName || !SdNameDisallowedChars.Contains(c))
				{
					charBuffer[bufferIndex++] = c;
				}
			}

			return new string(charBuffer, 0, bufferIndex);
		}

		public static string FormatSyslogSdnameField(string s, char[] charBuffer)
		{
			return FormatSyslogAsciiField(s, NilValue, 32, charBuffer, true);
		}
		
		public static string FormatSyslogField(string s, string replacementValue, int maxLength)
		{
			return string.IsNullOrWhiteSpace(s)
				? replacementValue
				: s.EnsureMaxLength(maxLength);
		}
	}
}
