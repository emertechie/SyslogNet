using System;
using System.Collections.Generic;

namespace SyslogNet.Client
{
	public class SyslogMessage
	{
		public Facility Facility { get; set; }

		public Severity Severity { get; set; }

		public DateTimeOffset? Timestamp { get; set; }

		public string HostName { get; set; }

		public string AppName { get; set; }

		public string ProcId { get; set; }

		public string MsgId { get; set; }

		public string Message { get; set; }

		public ICollection<StructuredDataElement> StructuredDataElements { get; set; }
	}
}
