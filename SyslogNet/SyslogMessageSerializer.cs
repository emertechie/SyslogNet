using System;
using System.IO;
using System.Text;

namespace SyslogNet
{
	public class SyslogMessageSerializer
	{
		public const string NilValue = "-";
		private readonly char[] asciiCharsBuffer = new char[255];

		public void Serialize(SyslogMessage syslogMessage, Stream stream)
		{
			var headerBuilder = new StringBuilder();
			headerBuilder.Append("<").Append(syslogMessage.PriorityValue).Append(">");
			headerBuilder.Append(syslogMessage.Version);
			headerBuilder.Append(" ").Append(syslogMessage.Timestamp.FormatSyslogField(NilValue));
			headerBuilder.Append(" ").Append(syslogMessage.HostName.FormatSyslogAsciiField(NilValue, 255, asciiCharsBuffer));
			headerBuilder.Append(" ").Append(syslogMessage.AppName.FormatSyslogAsciiField(NilValue, 48, asciiCharsBuffer));
			headerBuilder.Append(" ").Append(syslogMessage.ProcId.FormatSyslogAsciiField(NilValue, 128, asciiCharsBuffer));
			headerBuilder.Append(" ").Append(syslogMessage.MsgId.FormatSyslogAsciiField(NilValue, 32, asciiCharsBuffer));

			// TODO structured data

			bool hasMessage = !String.IsNullOrWhiteSpace(syslogMessage.Message);
			if (hasMessage)
				headerBuilder.Append(" ");

			byte[] asciiBytes = Encoding.ASCII.GetBytes(headerBuilder.ToString());
			stream.Write(asciiBytes, 0, asciiBytes.Length);

			if (hasMessage)
			{
				byte[] utf8Preamble = Encoding.UTF8.GetPreamble();
				stream.Write(utf8Preamble, 0, utf8Preamble.Length);

				byte[] messageBytes = Encoding.UTF8.GetBytes(syslogMessage.Message);
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