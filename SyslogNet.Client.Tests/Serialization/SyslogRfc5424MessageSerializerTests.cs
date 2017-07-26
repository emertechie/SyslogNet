using System;
using System.Collections.Generic;
using System.Text;
using SyslogNet.Client.Serialization;
using Xunit;
using Xunit.Extensions;
using static SyslogNet.Client.Tests.Serialization.SyslogSerializerExtensionsForTests;

namespace SyslogNet.Client.Tests.Serialization
{
	public class SyslogRfc5424MessageSerializerTests
	{
		private const string TimestampRegex = @"\d{4}-\d{2}-\d{2}.\d{2}:\d{2}:\d{2}\.\d{6}\S+";
		public SyslogRfc5424MessageSerializerTests()
		{
		}

		[Theory]
		[InlineData(Facility.KernelMessages, Severity.Emergency, 0)]
		[InlineData(Facility.LocalUse4, Severity.Notice, 165)]
		public void CalculatesPriorityValueCorrectly(Facility facility, Severity severity, int expectedPriorityValue)
		{
			var msg = CreateMinimalSyslogMessage(facility, severity);

			var serializedMsg = SerializeRfc5424(msg);
			Assert.Equal($"<{expectedPriorityValue}>1 {TimestampRegex} - - - - -",
						 serializedMsg,
						 new RegexComparer());
		}

		[Fact]
		public void CanFormatSyslogMessageWithUtcDateTime()
		{
			var utcDateTime = new DateTime(2012, 9, 15, 20, 23, 15, 999, DateTimeKind.Utc);
			var expectedTimestamp = "2012-09-15T20:23:15.999000+00:00";

			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, utcDateTime);

			var serializedMsg = SerializeRfc5424(msg);
			Assert.Equal($"<11>1 {expectedTimestamp} - - - - - -", serializedMsg);
		}

		[Fact]
		public void CanFormatSyslogMessageWithNonUtcDateTimeOffset()
		{
			var offset = TimeSpan.FromHours(2);
			var knownDateTimeOffset = new DateTimeOffset(2012, 9, 16, 9, 26, 10, 123, offset);
			var expectedTimestamp = "2012-09-16T09:26:10.123000+02:00";

			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, knownDateTimeOffset);

			var serializedMsg = SerializeRfc5424(msg);
			Assert.Equal($"<11>1 {expectedTimestamp} - - - - - -", serializedMsg);
		}

		[Theory]
		[InlineData("the.host.name", "the.host.name")]
		[InlineData("the . host . name", "the.host.name")]
		public void CanFormatSyslogMessageWithHostName(string hostName, string expectedHostName)
		{
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, hostName: hostName);

			var serializedMsg = SerializeRfc5424(msg);
			Assert.Equal($"<11>1 {TimestampRegex} {expectedHostName} - - - -",
				serializedMsg,
				new RegexComparer());
		}

		[Theory]
		[InlineData("TheAppName", "TheAppName")]
		[InlineData("The App Name", "TheAppName")]
		public void CanFormatSyslogMessageWithAppName(string appName, string expectedAppName)
		{
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, appName: appName);

			var serializedMsg = SerializeRfc5424(msg);
			Assert.Equal($"<11>1 {TimestampRegex} - {expectedAppName} - - -",
				serializedMsg,
				new RegexComparer());
		}

		[Theory]
		[InlineData("TheProcId", "TheProcId")]
		[InlineData("The Proc Id", "TheProcId")]
		public void CanFormatSyslogMessageWithProcId(string procId, string expectedProcId)
		{
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, procId: procId);

			var serializedMsg = SerializeRfc5424(msg);
			Assert.Equal($"<11>1 {TimestampRegex} - - {expectedProcId} - -",
				serializedMsg,
				new RegexComparer());
		}

		[Theory]
		[InlineData("TheMsgId", "TheMsgId")]
		[InlineData("The Msg Id", "TheMsgId")]
		public void CanFormatSyslogMessageWithMsgId(string msgId, string expectedMsgId)
		{
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, msgId: msgId);

			var serializedMsg = SerializeRfc5424(msg);
			Assert.Equal($"<11>1 {TimestampRegex} - - - {expectedMsgId} -",
				serializedMsg,
				new RegexComparer());
	    }

	    [Theory]
	    [InlineData("The message", "The message")]
	    [InlineData("メッセージ", "メッセージ")]
	    public void CanFormatSyslogMessageWithUtf8Message(string message, string expectedMessage)
	    {
	        var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, message: message);

		    var serializedMsg = SerializeRfc5424(msg);

		    var messagePrefix = $"<11>1 {TimestampRegex} - - - - - ";
		    Assert.Equal(messagePrefix,
			    serializedMsg,
			    new RegexComparer());

		    var messageIndex = 0;
	        var bom = new[] { (byte)239 /*EF*/, (byte)187 /*BB*/, (byte)191 /*BF*/ };

		    var bomChars = Encoding.UTF8.GetChars(bom);
	        foreach (var bomChar in bomChars)
	        {
	            var c = serializedMsg[messagePrefix.Length + messageIndex++];
	            Assert.Equal(bomChar, c);
	        }

	        var utf8MessagePart = serializedMsg.Substring(messagePrefix.Length + bomChars.Length);
	        Assert.Equal(expectedMessage, utf8MessagePart);
	    }

	    [Fact]
	    public void CanFormatSyslogMessageWithSingleStructuredDataElement()
	    {
	        var sdi = "test@12345";
	        var key = "key";
	        var value = "value";

            var structuredDataElements = new List<StructuredDataElement>
	        {
	            new StructuredDataElement(sdi, new Dictionary<string, string> {{key, value}})
	        };
	        var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, structuredDataElements:structuredDataElements.ToArray());

		    var serializedMsg = SerializeRfc5424(msg);

		    Assert.Equal($"<11>1 {TimestampRegex} - - - - [{sdi} {key}=\"{value}\"] -",
			    serializedMsg,
			    new RegexComparer());
	    }

	    [Fact]
	    public void CanFormatSyslogMessageWithMultipleStructuredDataElement()
	    {
	        var sdi1 = "testA@12345";
	        var key1 = "key";
	        var value1 = "value";

	        var sdi2 = "testB@23456";
	        var key2 = "key";
	        var value2 = "value";

            var structuredDataElements = new List<StructuredDataElement>
	        {
	            new StructuredDataElement(sdi1, new Dictionary<string, string> {{key1, value1}}),
	            new StructuredDataElement(sdi2, new Dictionary<string, string> {{key2, value2}}),
            };
	        var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, structuredDataElements: structuredDataElements.ToArray());

		    var serializedMsg = SerializeRfc5424(msg);

		    Assert.Equal($"<11>1 {TimestampRegex} - - - - [{sdi1} {key1}=\"{value1}\"][{sdi2} {key2}=\"{value2}\"] -",
			    serializedMsg,
			    new RegexComparer());
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
			return new SyslogMessage
			{
				Timestamp = dateTimeOffset,
				Facility = facility,
				Severity = severity,
				HostName = hostName,
				AppName = appName,
				ProcId = procId,
				MsgId = msgId,
				Message = message,
				StructuredDataElements = structuredDataElements
			};
		}
	}
}