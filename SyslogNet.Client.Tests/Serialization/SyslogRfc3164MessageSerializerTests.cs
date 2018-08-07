using System;
using SyslogNet.Client.Serialization;
using Xunit;

namespace SyslogNet.Client.Tests.Serialization
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
			Assert.StartsWith($"<{expectedPriorityValue}>", serializedMsg);
		}

		[Theory]
		[InlineData("2013-01-18 17:42:30.999", "Jan 18 17:42:30")]
		[InlineData("2013-01-08 17:42:30.999", "Jan  8 17:42:30")]
		public void SerializesDateTimesCorrectly(string dateTimeStr, string expectedFormat)
		{
			DateTimeOffset? dateTime = DateTime.Parse(dateTimeStr);
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, dateTime);

			string serializedMsg = sut.Serialize(msg);
			Assert.StartsWith($"<11>{expectedFormat}", serializedMsg);
		}

		[Theory]
		[InlineData("127.0.0.1")]
		[InlineData("FooMachine")]
		public void SerializesHostNameCorrectly(string hostName)
		{
			DateTimeOffset? dateTime = DateTime.Parse("2013-01-18 17:00:00");
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, dateTime, hostName);

			string serializedMsg = sut.Serialize(msg);
			Assert.StartsWith($"<11>Jan 18 17:00:00 {hostName}", serializedMsg);
		}

		[Fact]
		public void SerializesAppNameCorrectly()
		{
			const string appName = "MyApp";

			DateTimeOffset? dateTime = DateTime.Parse("2013-01-18 17:00:00");
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, dateTime, "FooMachine", appName);

			string serializedMsg = sut.Serialize(msg);
			Assert.StartsWith($"<11>Jan 18 17:00:00 FooMachine {appName}:", serializedMsg);
		}

		[Fact]
		public void SerializesMessageCorrectly()
		{
			const string message = "Foo and bar";

			DateTimeOffset? dateTime = DateTime.Parse("2013-01-18 17:00:00");
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, dateTime, "FooMachine", "MyApp", message);

			string serializedMsg = sut.Serialize(msg);
			Assert.StartsWith($"<11>Jan 18 17:00:00 FooMachine MyApp:{message}", serializedMsg);
		}

		private static SyslogMessage CreateMinimalSyslogMessage(
			Facility facility,
			Severity severity,
			DateTimeOffset? dateTimeOffset = null,
			string hostName = null,
			string appName = null,
			string message = null)
		{
			const string procId = null;
			const string msgId = null;

			return new SyslogMessage(dateTimeOffset, facility, severity, hostName, appName, procId, msgId, message);
		}
	}
}