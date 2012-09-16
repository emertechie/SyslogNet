using System.Collections.Generic;

namespace SyslogNet
{
	public class StructuredDataElement
	{
		// RFC 5424 specifies that you must provide a private enterprise number. If none specified, using example number reserved for documentation (see RFC)
		public const string DefaultPrivateEnterpriseNumber = "32473";

		private readonly string sdId;
		private readonly Dictionary<string, string> parameters;

		public StructuredDataElement(string sdId, Dictionary<string, string> parameters)
		{
			this.sdId = sdId.Contains("@") ? sdId : sdId + "@" + DefaultPrivateEnterpriseNumber;
			this.parameters = parameters;
		}

		public string SdId
		{
			get { return sdId; }
		}

		public Dictionary<string, string> Parameters
		{
			get { return parameters; }
		}
	}
}