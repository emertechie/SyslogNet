using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public enum MessageTransfer {
		OctetCounting		= 0,
		NonTransparentFraming	= 1
	}

	public class SyslogTcpSender : ISyslogMessageSender
	{
		protected String hostname;
		protected int port;

		protected TcpClient tcpClient = null;
		protected Stream transportStream = null;

		public virtual MessageTransfer messageTransfer { get; set; }
		public byte trailer { get; set; }

		public SyslogTcpSender(string hostname, int port, bool connect = true)
		{
			this.hostname = hostname;
			this.port = port;

			if (connect)
				Connect();

			messageTransfer = MessageTransfer.OctetCounting;
			trailer = 10; // LF
		}

		public void Connect()
		{
			try
			{
				tcpClient = new TcpClient(hostname, port);
				transportStream = tcpClient.GetStream();
			}
			catch
			{
				Disconnect();
				throw;
			}
		}

		public virtual void Reconnect()
		{
			Disconnect();
			Connect();
		}

		public void Disconnect()
		{
			if (transportStream != null)
			{
				transportStream.Close();
				transportStream = null;
			}

			if (tcpClient != null)
			{
				tcpClient.Close();
				tcpClient = null;
			}
		}

		public void Send(SyslogMessage message, ISyslogMessageSerializer serializer)
		{
			Send(message, serializer, true);
		}

		protected void Send(SyslogMessage message, ISyslogMessageSerializer serializer, bool flush = true)
		{
			if(transportStream == null)
			{
				throw new IOException("No transport stream exists");
			}

			using (MemoryStream memoryStream = new MemoryStream())
			{
				var datagramBytes = serializer.Serialize(message);

				if (messageTransfer.Equals(MessageTransfer.OctetCounting))
				{
					byte[] messageLength = Encoding.ASCII.GetBytes(datagramBytes.Length.ToString());
					memoryStream.Write(messageLength, 0, messageLength.Length);
					memoryStream.WriteByte(32); // Space
				}

				memoryStream.Write(datagramBytes, 0, datagramBytes.Length);

				if (messageTransfer.Equals(MessageTransfer.NonTransparentFraming))
				{
					memoryStream.WriteByte(trailer); // LF
				}

				transportStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
			}

			if (flush && !(transportStream is NetworkStream))
				transportStream.Flush();
		}

		public void Send(IEnumerable<SyslogMessage> messages, ISyslogMessageSerializer serializer)
		{
			foreach (SyslogMessage message in messages)
			{
				Send(message, serializer, false);
			}

			if (!(transportStream is NetworkStream))
				transportStream.Flush();
		}

		public void Dispose()
		{
			Disconnect();
		}
	}
}