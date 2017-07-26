using System;
using Xunit;
using Xunit.Extensions;
using static SyslogNet.Client.Tests.Serialization.SyslogSerializerExtensionsForTests;

namespace SyslogNet.Client.Tests.Serialization
{
	public class SyslogRfc3164MessageSerializerTests
	{
		[Theory]
		[InlineData(Facility.KernelMessages, Severity.Emergency, 0)]
		[InlineData(Facility.LocalUse4, Severity.Notice, 165)]
		public void CalculatesPriorityValueCorrectly(Facility facility, Severity severity, int expectedPriorityValue)
		{
			var msg = CreateMinimalSyslogMessage(facility, severity);

			var serializedMsg = SerializeRfc3164(msg);
			Assert.True(serializedMsg.StartsWith($"<{expectedPriorityValue}>"));
		}

		[Theory]
		[InlineData("2013-01-18 17:42:30.999", "Jan 18 17:42:30")]
		[InlineData("2013-01-08 17:42:30.999", "Jan  8 17:42:30")]
		public void SerializesDateTimesCorrectly(string dateTimeStr, string expectedFormat)
		{
			DateTimeOffset? dateTime = DateTime.Parse(dateTimeStr);
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, dateTime);

			var serializedMsg = SerializeRfc3164(msg);
			Assert.True(serializedMsg.StartsWith($"<11>{expectedFormat}"));
		}

		[Theory]
		[InlineData("127.0.0.1")]
		[InlineData("FooMachine")]
		public void SerializesHostNameCorrectly(string hostName)
		{
			DateTimeOffset? dateTime = DateTime.Parse("2013-01-18 17:00:00");
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, dateTime, hostName);

			var serializedMsg = SerializeRfc3164(msg);
			Assert.True(serializedMsg.StartsWith($"<11>Jan 18 17:00:00 {hostName}"));
		}

		[Fact]
		public void SerializesAppNameCorrectly()
		{
			const string appName = "MyApp";

			DateTimeOffset? dateTime = DateTime.Parse("2013-01-18 17:00:00");
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, dateTime, "FooMachine", appName);

			var serializedMsg = SerializeRfc3164(msg);
			Assert.True(serializedMsg.StartsWith($"<11>Jan 18 17:00:00 FooMachine {appName}:"));
		}

		[Fact]
		public void SerializesMessageCorrectly()
		{
			const string message = "Foo and bar";

			DateTimeOffset? dateTime = DateTime.Parse("2013-01-18 17:00:00");
			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, dateTime, "FooMachine", "MyApp", message);

			var serializedMsg = SerializeRfc3164(msg);
			Assert.True(serializedMsg.StartsWith($"<11>Jan 18 17:00:00 FooMachine MyApp:{message}"));
		}

		private static SyslogMessage CreateMinimalSyslogMessage(
			Facility facility,
			Severity severity,
			DateTimeOffset? dateTimeOffset = null,
			string hostName = null,
			string appName = null,
			string message = null)
		{
			return new SyslogMessage
			{
				Timestamp = dateTimeOffset,
				Facility = facility,
				Severity = severity,
				HostName = hostName,
				AppName = appName,
				Message = message
			};
		}
	}
}