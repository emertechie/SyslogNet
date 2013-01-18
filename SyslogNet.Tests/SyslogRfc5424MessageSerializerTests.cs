using System;
using System.Text;
using Xunit;
using Xunit.Extensions;

namespace SyslogNet.Tests
{
	public class SyslogRfc5424MessageSerializerTests
	{
		private readonly SyslogRfc5424MessageSerializer sut;

		public SyslogRfc5424MessageSerializerTests()
		{
			sut = new SyslogRfc5424MessageSerializer();
		}

		[Theory]
		[InlineData(Facility.KernelMessages, Severity.Emergency, 0)]
		[InlineData(Facility.LocalUse4, Severity.Notice, 165)]
		public void CalculatesPriorityValueCorrectly(Facility facility, Severity severity, int expectedPriorityValue)
		{
			var msg = CreateMinimalSyslogMessage(facility, severity);

			string serializedMsg = sut.Serialize(msg);
			Assert.Equal(String.Format("<{0}>1 - - - - -", expectedPriorityValue), serializedMsg);
		}

		[Fact]
		public void CanFormatSyslogMessageWithUtcDateTime()
		{
			var utcDateTime = new DateTime(2012, 9, 15, 20, 23, 15, 999, DateTimeKind.Utc);
			var expectedTimestamp = "2012-09-15T20:23:15.999000+00:00";

			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, utcDateTime);

			string serializedMsg = sut.Serialize(msg);
			Assert.Equal(String.Format("<11>1 {0} - - - -", expectedTimestamp), serializedMsg);
		}

		[Fact]
		public void CanFormatSyslogMessageWithNonUtcDateTimeOffset()
		{
			var offset = TimeSpan.FromHours(2);
			var knownDateTimeOffset = new DateTimeOffset(2012, 9, 16, 9, 26, 10, 123, offset);
			var expectedTimestamp = "2012-09-16T09:26:10.123000+02:00";

			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, knownDateTimeOffset);

			string serializedMsg = sut.Serialize(msg);
			Assert.Equal(String.Format("<11>1 {0} - - - -", expectedTimestamp), serializedMsg);
		}

		[Theory]
		[InlineData("the.host.name", "the.host.name")]
		[InlineData("the . host . name", "the.host.name")]
		public void CanFormatSyslogMessageWithHostName(string hostName, string expectedHostName)
		{
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, hostName: hostName);

			string serializedMsg = sut.Serialize(msg);
			Assert.Equal(String.Format("<11>1 - {0} - - -", expectedHostName), serializedMsg);
		}

		[Theory]
		[InlineData("TheAppName", "TheAppName")]
		[InlineData("The App Name", "TheAppName")]
		public void CanFormatSyslogMessageWithAppName(string appName, string expectedAppName)
		{
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, appName: appName);

			string serializedMsg = sut.Serialize(msg);
			Assert.Equal(String.Format("<11>1 - - {0} - -", expectedAppName), serializedMsg);
		}

		[Theory]
		[InlineData("TheProcId", "TheProcId")]
		[InlineData("The Proc Id", "TheProcId")]
		public void CanFormatSyslogMessageWithProcId(string procId, string expectedProcId)
		{
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, procId: procId);

			string serializedMsg = sut.Serialize(msg);
			Assert.Equal(String.Format("<11>1 - - - {0} -", expectedProcId), serializedMsg);
		}

		[Theory]
		[InlineData("TheMsgId", "TheMsgId")]
		[InlineData("The Msg Id", "TheMsgId")]
		public void CanFormatSyslogMessageWithMsgId(string msgId, string expectedMsgId)
		{
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, msgId: msgId);

			string serializedMsg = sut.Serialize(msg);
			Assert.Equal(String.Format("<11>1 - - - - {0}", expectedMsgId), serializedMsg);
		}

		[Theory]
		[InlineData("The message", "The message")]
		[InlineData("メッセージ", "メッセージ")]
		public void CanFormatSyslogMessageWithUtf8Message(string message, string expectedMessage)
		{
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, message: message);

			string serializedMsg = sut.Serialize(msg);

			string messagePrefix = "<11>1 - - - - - ";
			Assert.True(serializedMsg.StartsWith(messagePrefix));

			int messageIndex = 0;
			var bom = new[] { (byte)239 /*EF*/, (byte)187 /*BB*/, (byte)191 /*BF*/ };

			char[] bomChars = Encoding.UTF8.GetChars(bom);
			foreach (char bomChar in bomChars)
			{
				char c = serializedMsg[messagePrefix.Length + messageIndex++];
				Assert.Equal(bomChar, c);
			}

			string utf8MessagePart = serializedMsg.Substring(messagePrefix.Length + bomChars.Length);
			Assert.Equal(expectedMessage, utf8MessagePart);
		}

		private static SyslogMessage CreateMinimalSyslogMessage(
			Facility facility,
			Severity severity,
			DateTimeOffset? dateTimeOffset = null,
			string hostName = null,
			string appName = null,
			string procId = null,
			string msgId = null,
			string message = null,
			params StructuredDataElement[] structuredDataElements)
		{
			return new SyslogMessage(dateTimeOffset, facility, severity, hostName, appName, procId, msgId, message, structuredDataElements);
		}
	}
}