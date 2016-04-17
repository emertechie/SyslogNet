using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SyslogNet.Client.Serialization
{
	public class SyslogRfc5424MessageSerializer : SyslogMessageSerializerBase, ISyslogMessageSerializer
	{
		public const string NilValue = "-";
		public static readonly HashSet<char> sdNameDisallowedChars = new HashSet<char>() {' ', '=', ']', '"' };

		private readonly char[] asciiCharsBuffer = new char[255];

		public void Serialize(SyslogMessage message, Stream stream)
		{
			var priorityValue = CalculatePriorityValue(message.Facility, message.Severity);

			// Note: The .Net ISO 8601 "o" format string uses 7 decimal places for fractional second. Syslog spec only allows 6, hence the custom format string
			var timestamp = message.DateTimeOffset.HasValue
				? message.DateTimeOffset.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffffK")
				: null;

			var messageBuilder = new StringBuilder();
			messageBuilder.Append("<").Append(priorityValue).Append(">");
			messageBuilder.Append(message.Version);
			messageBuilder.Append(" ").Append(timestamp.FormatSyslogField(NilValue));
			messageBuilder.Append(" ").Append(message.HostName.FormatSyslogAsciiField(NilValue, 255, asciiCharsBuffer));
			messageBuilder.Append(" ").Append(message.AppName.FormatSyslogAsciiField(NilValue, 48, asciiCharsBuffer));
			messageBuilder.Append(" ").Append(message.ProcId.FormatSyslogAsciiField(NilValue, 128, asciiCharsBuffer));
			messageBuilder.Append(" ").Append(message.MsgId.FormatSyslogAsciiField(NilValue, 32, asciiCharsBuffer));

			writeStream(stream, Encoding.ASCII, messageBuilder.ToString());

			AddStructuredData(message, stream, messageBuilder);

			if (!String.IsNullOrWhiteSpace(message.Message))
			{
				// Space
				stream.WriteByte(32);

				stream.Write(Encoding.UTF8.GetPreamble(), 0, Encoding.UTF8.GetPreamble().Length);
				writeStream(stream, Encoding.UTF8, message.Message);
			}
		}

		private void AddStructuredData(SyslogMessage message, Stream stream, StringBuilder messageBuilder)
		{
			// Structured data
			var structuredData = message.StructuredDataElements.ToList();
			if (structuredData.Any())
			{
				foreach (StructuredDataElement sdElement in structuredData)
				{
					messageBuilder.Clear()
						.Append(" ")
						.Append("[")
						.Append(sdElement.SdId.FormatSyslogSdnameField(asciiCharsBuffer));

					writeStream(stream, Encoding.ASCII, messageBuilder.ToString());

					foreach(System.Collections.Generic.KeyValuePair<string, string> sdParam in sdElement.Parameters)
					{
						messageBuilder.Clear()
							.Append(" ")
							.Append(sdParam.Key.FormatSyslogSdnameField(asciiCharsBuffer))
							.Append("=")
							.Append("\"")
							.Append(
								sdParam.Value != null ?
								sdParam.Value
										.Replace("\\", "\\\\")
										.Replace("\"", "\\\"")
										.Replace("]", "\\]")
									:
									String.Empty
							)
							.Append("\"");

						writeStream(stream, Encoding.UTF8, messageBuilder.ToString());
					}

					// ]
					stream.WriteByte(93);
				}
			}
			else
			{
				// " -"
				writeStream(stream, Encoding.ASCII, " ");
				writeStream(stream, Encoding.ASCII, NilValue);
			}
		}

		private void writeStream(Stream stream, Encoding encoding, String data)
		{
			byte[] streamBytes = encoding.GetBytes(data);
			stream.Write(streamBytes, 0, streamBytes.Length);
		}
	}

	internal static class StringExtensions
	{
		public static string IfNotNullOrWhitespace(this string s, Func<string, string> action)
		{
			return String.IsNullOrWhiteSpace(s) ? s : action(s);
		}

		public static string FormatSyslogField(this string s, string replacementValue, int? maxLength = null)
		{
			return String.IsNullOrWhiteSpace(s)
				? replacementValue
				: maxLength.HasValue ? EnsureMaxLength(s, maxLength.Value) : s;
		}

		public static string EnsureMaxLength(this string s, int maxLength)
		{
			return String.IsNullOrWhiteSpace(s)
				? s
				: s.Length > maxLength ? s.Substring(0, maxLength) : s;
		}

		public static string FormatSyslogAsciiField(this string s, string replacementValue, int maxLength, char[] charBuffer, Boolean sdName = false)
		{
			s = FormatSyslogField(s, replacementValue, maxLength);

			int bufferIndex = 0;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (c >= 33 && c <= 126)
				{
					if (!sdName || !SyslogRfc5424MessageSerializer.sdNameDisallowedChars.Contains(c))
					{
						charBuffer[bufferIndex++] = c;
					}
				}
			}

			return new string(charBuffer, 0, bufferIndex);
		}

		public static string FormatSyslogSdnameField(this string s, char[] charBuffer)
		{
			return FormatSyslogAsciiField(s, SyslogRfc5424MessageSerializer.NilValue, 32, charBuffer, true);
		}
	}
}
