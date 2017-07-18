using System;

namespace SyslogNet.Client
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
		private readonly StructuredDataElement[] structuredDataElements;
		private readonly DateTimeOffset? dateTimeOffset;
		
		public static Facility DefaultFacility = Facility.UserLevelMessages;
		public static Severity DefaultSeverity = Severity.Informational;

		/// <summary>
		/// Convenience overload for sending local syslog messages with default facility (UserLevelMessages)
		/// </summary>
		public SyslogMessage(
			Severity severity,
			string appName,
			string message)
		: this(DefaultFacility, severity, appName, message)
		{
		}

		/// <summary>
		/// Constructor for use when sending local syslog messages
		/// </summary>
		public SyslogMessage(
			Facility facility,
			Severity severity,
			string appName,
			string message)
		{
			this.facility = facility;
			this.severity = severity;
			this.appName = appName;
			this.message = message;
		}

		/// <summary>
		/// Constructor for use when sending RFC 3164 messages
		/// </summary>
		public SyslogMessage(
			DateTimeOffset? dateTimeOffset,
			Facility facility,
			Severity severity,
			string hostName,
			string appName,
			string message)
		{
			this.dateTimeOffset = dateTimeOffset;
			this.facility = facility;
			this.severity = severity;
			this.hostName = hostName;
			this.appName = appName;
			this.message = message;
		}

		/// <summary>
		/// Constructor for use when sending RFC 5424 messages
		/// </summary>
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
			: this(dateTimeOffset, facility, severity, hostName, appName, message)
		{
			this.procId = procId;
			this.msgId = msgId;
			this.structuredDataElements = structuredDataElements;
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

		public DateTimeOffset? DateTimeOffset
		{
			get { return dateTimeOffset; }
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

		public StructuredDataElement[] StructuredDataElements
		{
			get { return structuredDataElements; }
		}
	}
}
