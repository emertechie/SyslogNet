using System;
using Xunit;
using Xunit.Extensions;

namespace SyslogNet.Tests
{
	public class SyslogRfc3164MessageSerializerTests
	{
		private readonly SyslogRfc3164MessageSerializer sut;

		public SyslogRfc3164MessageSerializerTests()
		{
			sut = new SyslogRfc3164MessageSerializer();
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
		public void Todo()
		{
			throw new NotImplementedException();
		}

		private static SyslogMessage CreateMinimalSyslogMessage(
			Facility facility,
			Severity severity,
			DateTimeOffset? dateTimeOffset = null,
			string hostName = null,
			string appName = null,
			string procId = null,
			string msgId = null,
			string message = null)
		{
			return new SyslogMessage(dateTimeOffset, facility, severity, hostName, appName, procId, msgId, message);
		}
	}
}