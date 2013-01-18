using System;
using System.IO;
using System.Text;

namespace SyslogNet
{
	public class SyslogRfc3164MessageSerializer : SyslogMessageSerializerBase
	{
		public const string NilValue = "-";

		public void Serialize(SyslogMessage message, Stream stream)
		{
			// TODO: Should this be in local time?
			string timestamp = message.DateTimeOffset.HasValue
				? message.DateTimeOffset.Value.ToString("MMM dd HH:mm:ss") // TODO: format is not quite right (dd)
				: null;

			var headerBuilder = new StringBuilder();
			headerBuilder.Append("<").Append(CalculatePriorityValue(message.Facility, message.Severity)).Append(">");
			// TODO: Remove use of NilValue?
			headerBuilder.Append(timestamp.FormatRfc3164SyslogField(NilValue)).Append(" ");
			headerBuilder.Append(message.HostName.FormatRfc3164SyslogField(NilValue, 255)).Append(" ");
			headerBuilder.Append(message.AppName.FormatSyslogField(NilValue, 32)).Append(":");
			headerBuilder.Append(message.Message.FormatSyslogField(NilValue));

			byte[] asciiBytes = Encoding.ASCII.GetBytes(headerBuilder.ToString());
			stream.Write(asciiBytes, 0, asciiBytes.Length);
		}

		/*
		private static byte[] buildSyslogMessage(SyslogFacility facility, SyslogSeverity priority, DateTime time, string sender, string body)
        {

            // Get sender machine name
            string machine = System.Net.Dns.GetHostName() + " ";

            // Calculate PRI field
            int calculatedPriority = (int)facility * 8 + (int)priority;
            string pri = "<" + calculatedPriority.ToString() + ">";

            string timeToString = time.ToString("MMM dd HH:mm:ss ");
            sender = sender + ": ";

            string[] strParams = { pri, timeToString, machine, sender, body };
            return Encoding.ASCII.GetBytes(string.Concat(strParams));
        }
		*/
	}

	internal static class SyslogRfc3164StringExtensions
	{
		public static string FormatRfc3164SyslogField(this string s, string nullOrWhitespaceReplacementValue, int? maxLength = null)
		{
			return String.IsNullOrWhiteSpace(s)
				? nullOrWhitespaceReplacementValue
				: maxLength.HasValue
					? s.Length > maxLength.Value ? s.Substring(0, maxLength.Value) : s
					: s;
		}

		/*public static string FormatRfc3164SyslogAsciiField(this string s, string nullOrWhitespaceReplacementValue, int maxLength, char[] charBuffer)
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
		}*/
	}
}