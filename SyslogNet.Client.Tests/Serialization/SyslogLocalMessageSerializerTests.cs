using System;
using SyslogNet.Client.Serialization;
using Xunit;
using Xunit.Extensions;

namespace SyslogNet.Client.Tests.Serialization
{
	public  class SyslogLocalMessageSerializerTests
	{
		private readonly SyslogLocalMessageSerializer sut;

		public SyslogLocalMessageSerializerTests()
		{
			sut = new SyslogLocalMessageSerializer();
		}

		[Fact]
		public void SerializesMessageCorrectly()
		{
			const string message = "Foo and bar";

			var msg = CreateMinimalSyslogMessage(Facility.UserLevelMessages, Severity.Error, "MyApp", message);

			string serializedMsg = sut.Serialize(msg);
			Assert.Equal(serializedMsg, message);
		}

		private static SyslogMessage CreateMinimalSyslogMessage(
			Facility facility,
			Severity severity,
			string appName = null,
			string message = null)
		{
			return new SyslogMessage(facility, severity, appName, message);
		}
	}
}
