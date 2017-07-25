using System;
using System.Collections.Generic;

namespace SyslogNet.Client
{
	public class SyslogMessage
	{
		public const int Version = 1;

		public Facility Facility { get; set; }

		public Severity Severity { get; set; }

		public DateTimeOffset? DateTimeOffset { get; set; }

		public string HostName { get; set; }

		public string AppName { get; set; }

		public string ProcId { get; set; }

		public string MsgId { get; set; }

		public string Message { get; set; }

		public ICollection<StructuredDataElement> StructuredDataElements { get; set; }
	}
}
