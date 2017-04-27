using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SyslogNet.Client.Serialization;

namespace SyslogNet.Client.Transport
{
	public enum MessageTransfer {
		OctetCounting		= 0,
		NonTransparentFraming	= 1
	}

	public class SyslogTcpSender : ISyslogMessageSender, IDisposable
	{
		protected String hostname;
		protected int port;

		protected TcpClient tcpClient = null;
		protected Stream transportStream = null;

		public virtual MessageTransfer messageTransfer { get; set; }
		public byte trailer { get; set; }

		public SyslogTcpSender(string hostname, int port)
		{
			this.hostname = hostname;
			this.port = port;

			Connect();

			messageTransfer = MessageTransfer.OctetCounting;
			trailer = 10; // LF
		}

		protected void Connect()
		{
			try
			{
#if NET4_0
                tcpClient = new TcpClient(hostname, port);
#else
                tcpClient = new TcpClient(AddressFamily.InterNetwork);
                tcpClient.ConnectAsync(hostname, port).Wait();
#endif
                transportStream = tcpClient.GetStream();
			}
			catch
			{
				Dispose();
				throw;
			}
		}

		public virtual void Reconnect()
		{
			Dispose();
			Connect();
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

#if NET4_0
                transportStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
#else
                ArraySegment<byte> buffer;
                
                if(memoryStream.TryGetBuffer(out buffer))
                {
                    transportStream.Write(buffer.Array, 0, buffer.Count);
                }
#endif
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
			if (transportStream != null)
			{
#if NET4_0
                transportStream.Close();
#else
                transportStream.Dispose();
#endif
                transportStream = null;
			}

			if (tcpClient != null)
			{
#if NET4_0
                tcpClient.Close();
#else
                tcpClient.Dispose();
#endif
                tcpClient = null;
			}
		}
	}
}