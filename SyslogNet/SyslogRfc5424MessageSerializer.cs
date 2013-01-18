using System;
using System.IO;
using System.Text;

namespace SyslogNet
{
	public class SyslogRfc5424MessageSerializer : SyslogMessageSerializerBase
	{
		public const string NilValue = "-";
		private readonly char[] asciiCharsBuffer = new char[255];

		public void Serialize(SyslogMessage message, Stream stream)
		{
			// Note: The .Net ISO 8601 "o" format string uses 7 decimal places for fractional second. Syslog spec only allows 6, hence the custom format string
			var timestamp = message.DateTimeOffset.HasValue
				? message.DateTimeOffset.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffffK")
				: null;

			var headerBuilder = new StringBuilder();
			headerBuilder.Append("<").Append(CalculatePriorityValue(message.Facility, message.Severity)).Append(">");
			headerBuilder.Append(message.Version);
			headerBuilder.Append(" ").Append(timestamp.FormatSyslogField(NilValue));
			headerBuilder.Append(" ").Append(message.HostName.FormatSyslogAsciiField(NilValue, 255, asciiCharsBuffer));
			headerBuilder.Append(" ").Append(message.AppName.FormatSyslogAsciiField(NilValue, 48, asciiCharsBuffer));
			headerBuilder.Append(" ").Append(message.ProcId.FormatSyslogAsciiField(NilValue, 128, asciiCharsBuffer));
			headerBuilder.Append(" ").Append(message.MsgId.FormatSyslogAsciiField(NilValue, 32, asciiCharsBuffer));

			// TODO structured data

			bool hasMessage = !String.IsNullOrWhiteSpace(message.Message);
			if (hasMessage)
				headerBuilder.Append(" ");

			byte[] asciiBytes = Encoding.ASCII.GetBytes(headerBuilder.ToString());
			stream.Write(asciiBytes, 0, asciiBytes.Length);

			if (hasMessage)
			{
				byte[] utf8Preamble = Encoding.UTF8.GetPreamble();
				stream.Write(utf8Preamble, 0, utf8Preamble.Length);

				byte[] messageBytes = Encoding.UTF8.GetBytes(message.Message);
				stream.Write(messageBytes, 0, messageBytes.Length);
			}
		}
	}

	internal static class StringExtensions
	{
		public static string FormatSyslogField(this string s, string nullOrWhitespaceReplacementValue, int? maxLength = null)
		{
			return String.IsNullOrWhiteSpace(s)
				? nullOrWhitespaceReplacementValue
				: maxLength.HasValue
					? s.Length > maxLength.Value ? s.Substring(0, maxLength.Value) : s
					: s;
		}

		public static string FormatSyslogAsciiField(this string s, string nullOrWhitespaceReplacementValue, int maxLength, char[] charBuffer)
		{
			s = FormatSyslogField(s, nullOrWhitespaceReplacementValue, maxLength);

			int bufferIndex = 0;
			for (int i = 0; i < s.Length; i++)
			{
				char c = s[i];
				if (c >= 33 && c <= 126)
					charBuffer[bufferIndex++] = c;
			}

			return new string(charBuffer, 0, bufferIndex);
		}
	}
}