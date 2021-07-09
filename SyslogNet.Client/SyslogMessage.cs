using System;
using System.Collections.Generic;

namespace SyslogNet.Client
{
	public class SyslogMessage
	{
		public static Facility DefaultFacility = Facility.UserLevelMessages;
		public static Severity DefaultSeverity = Severity.Informational;


		public void Send(SyslogOptions options)
		{
			options.Sender.Send(this, options.Serializer);
		}



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
			Facility = facility;
			Severity = severity;
			AppName = appName;
			Message = message;
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
			DateTimeOffset = dateTimeOffset;
			Facility = facility;
			Severity = severity;
			HostName = hostName;
			AppName = appName;
			Message = message;
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
			ProcId = procId;
			MsgId = msgId;
			StructuredDataElements = structuredDataElements;
		}

		public int Version
		{
			get { return 1; }
		}

		public Facility Facility { get; set; }

		public Severity Severity { get; set; }

		public DateTimeOffset? DateTimeOffset { get; set; }

		public string HostName { get; set; }

		public string AppName { get; set; }

		public string ProcId { get; set; }

		public string MsgId { get; set; }

		public string Message { get; set; }

		public IEnumerable<StructuredDataElement> StructuredDataElements { get; set; }
	}
}
