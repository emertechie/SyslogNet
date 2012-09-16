using System;
using Xunit;
using Xunit.Extensions;

namespace SyslogNet.Tests
{
	public class SyslogMessageTests
	{
		[Theory]
		[InlineData(Facility.KernelMessages, Severity.Emergency, 0)]
		[InlineData(Facility.LocalUse4, Severity.Notice, 165)]
		public void CalculatesPriorityValueCorrectly(Facility facility, Severity severity, int expectedPriorityValue)
		{
			var sut = CreateSut(facility, severity);
			Assert.Equal(expectedPriorityValue, sut.PriorityValue);
		}

		[Fact]
		public void TimestampForUtcBasedDateTimeIsCorrect()
		{
			var dt1 = new DateTime(2012, 9, 15, 20, 23, 15, 999, DateTimeKind.Utc);
			var expected = "2012-09-15T20:23:15.999000Z"; // Note: Max of 6 digits allowed in fractional second part (standard .Net "o" format uses 7)

			var sut = CreateSut(dateTimeOffset: dt1);
			Assert.Equal(expected, sut.Timestamp);
		}

		[Fact]
		public void TimestampForLocalDateTimeIsCorrect()
		{
			var offset = new TimeSpan(06, 00, 00);
			var localDateTime = new DateTimeOffset(2012, 9, 15, 12, 01, 15, 999, offset);

			string expected = "2012-09-15T12:01:15.999000+06:00"; // Note: Max of 6 digits allowed in fractional second part (standard .Net "o" format uses 7)

			var sut = CreateSut(dateTimeOffset: localDateTime);
			Assert.Equal(expected, sut.Timestamp);
		}

		private static SyslogMessage CreateSut(
			Facility facility = Facility.UserLevelMessages,
			Severity severity = Severity.Informational,
			DateTimeOffset? dateTimeOffset = null,
			bool dateTimeOffsetNull = false,
			string hostName = "localhost",
			string appName = "testapp",
			string procId = "12345",
			string msgId = "TestMsgType",
			string message = "The message",
			params StructuredDataElement[] structuredDataElements)
		{
			DateTimeOffset? dt = dateTimeOffset ?? (dateTimeOffsetNull ? (DateTimeOffset?)null : DateTimeOffset.UtcNow);

			return new SyslogMessage(dt, facility, severity, hostName, appName, procId, msgId, message, structuredDataElements);
		}
	}
}
