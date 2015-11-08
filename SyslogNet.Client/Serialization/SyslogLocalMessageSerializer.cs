using System;
using System.Text;
using System.IO;

namespace SyslogNet.Client.Serialization
{
	public class SyslogLocalMessageSerializer : SyslogMessageSerializerBase, ISyslogMessageSerializer
	{
		public Encoding Encoding { get; set; }

		// Default constructor: produce no BOM in local syslog messages
		public SyslogLocalMessageSerializer() : this(false) { ; }

		// Optionally produce a BOM in local syslog messages by passing true here
		// (This can produce problems with some older syslog programs, so default is false)
		public SyslogLocalMessageSerializer(bool useBOM) {
			Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: useBOM);
		}

		public void Serialize(SyslogMessage message, Stream stream)
		{
			// Local syslog serialization only cares about the Message string
			if (!String.IsNullOrWhiteSpace(message.Message))
			{
				byte[] streamBytes = Encoding.GetBytes(message.Message);
				stream.Write(streamBytes, 0, streamBytes.Length);
			}
		}
	}
}
