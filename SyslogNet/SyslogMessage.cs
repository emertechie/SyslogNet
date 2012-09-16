using System;
using System.Collections.Generic;

namespace SyslogNet
{
	public class SyslogMessage
	{
		private readonly Facility facility;
		private readonly Severity severity;
		private readonly string hostName;
		private readonly string appName;
		private readonly string procId;
		private readonly string msgId;
		private readonly string message;
		private readonly IEnumerable<StructuredDataElement> structuredDataElements;
		private readonly int priorityValue;
		private readonly DateTimeOffset? dateTimeOffset;
		private readonly string timestamp;
		
		public SyslogMessage(
			DateTimeOffset? dateTimeOffset,
			Facility facility,
			Severity severity,
			string hostName,
			string appName,
			string procId,
			string msgId,
			string message,
			params StructuredDataElement[] structuredDataElements)
		{
			this.dateTimeOffset = dateTimeOffset;
			this.facility = facility;
			this.severity = severity;
			this.hostName = hostName;
			this.appName = appName;
			this.procId = procId;
			this.msgId = msgId;
			this.message = message;
			this.structuredDataElements = structuredDataElements;

			// Note: The .Net ISO 8601 "o" format string uses 7 decimal places for fractional second. Syslog spec only allows 6, hence the custom format string
			timestamp = dateTimeOffset.HasValue
				? dateTimeOffset.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffffK")
				: null;

			priorityValue = ((int)facility * 8) + (int)severity;
		}

		public int Version
		{
			get { return 1; }
		}

		public Facility Facility
		{
			get { return facility; }
		}

		public Severity Severity
		{
			get { return severity; }
		}

		public int PriorityValue
		{
			get { return priorityValue; }
		}

		public DateTimeOffset? DateTimeOffset
		{
			get { return dateTimeOffset; }
		}

		public string Timestamp
		{
			get { return timestamp; }
		}

		public string HostName
		{
			get { return hostName; }
		}

		public string AppName
		{
			get { return appName; }
		}

		public string ProcId
		{
			get { return procId; }
		}

		public string MsgId
		{
			get { return msgId; }
		}

		public string Message
		{
			get { return message; }
		}

		public IEnumerable<StructuredDataElement> StructuredDataElements
		{
			get { return structuredDataElements; }
		}
	}
}
