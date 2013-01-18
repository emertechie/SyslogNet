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
