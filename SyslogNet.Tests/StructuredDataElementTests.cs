using System.Collections.Generic;
using Xunit;

namespace SyslogNet.Tests
{
	public class StructuredDataElementTests
	{
		[Fact]
		public void StructuredDataIdWillAlwaysBePrependedWithAPrivateEnterpriseNumber()
		{
			var parameters = new Dictionary<string, string>
			{
				{ "key1", "aaa" }
			};
			var sut = new StructuredDataElement("SomeSdId", parameters);

			Assert.Equal("SomeSdId@" + StructuredDataElement.DefaultPrivateEnterpriseNumber, sut.SdId);
		}

		[Fact]
		public void StructuredDataIdWillRetainProvidedPrivateEnterpriseNumber()
		{
			var parameters = new Dictionary<string, string>
			{
				{ "key1", "aaa" }
			};
			var sut = new StructuredDataElement("SomeSdId@12345", parameters);

			Assert.Equal("SomeSdId@12345", sut.SdId);
		}
	}
}